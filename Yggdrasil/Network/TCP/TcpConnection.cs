using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Yggdrasil.Network.TCP
{
	/// <summary>
	/// A connection via TCP socket.
	/// </summary>
	public abstract class TcpConnection
	{
		private const int BufferMaxSize = 4 * 1024;

		private readonly object _firstConnectSyncLock = new object();

		private readonly byte[] _buffer = new byte[BufferMaxSize];
		private Socket _socket;

		private readonly object _sendSyncLock = new object();
		private readonly Queue<SendItem> _sendQueue = new Queue<SendItem>();
		private bool _isSending;
		private int _sendOffset;
		private Timer _coalesceTimer;
		private bool _coalescing;

		private bool _raisedConnected;

		/// <summary>
		/// Current status of the connection.
		/// </summary>
		public ConnectionStatus Status { get; private set; }

		/// <summary>
		/// Remote host address.
		/// </summary>
		public string Address { get; private set; }

		/// <summary>
		/// Gets or sets how long queued data may be held back to combine it
		/// with data queued shortly after into a single send operation.
		/// </summary>
		/// <remarks>
		/// Defaults to zero, which sends queued data as soon as possible.
		/// Setting this trades a bounded amount of latency for fewer, larger
		/// packets, which is useful for applications that emit bursts of small
		/// messages. Unlike Nagle's algorithm, the delay never exceeds this
		/// value, as it doesn't wait for the remote host to acknowledge
		/// anything.
		/// </remarks>
		public TimeSpan SendCoalescingTime { get; set; } = TimeSpan.Zero;

		/// <summary>
		/// Raised when an exception occurs while receiving data.
		/// </summary>
		public event Action<TcpConnection, Exception> ReceiveException;

		/// <summary>
		/// Raised when connection was closed.
		/// </summary>
		public event Action<TcpConnection, ConnectionCloseType> Closed;

		/// <summary>
		/// Sets socket of this connection.
		/// </summary>
		/// <param name="socket"></param>
		/// <exception cref="InvalidOperationException">
		/// Throw if connection has already been initialized.
		/// </exception>
		internal void Init(Socket socket)
		{
			if (_socket != null)
				throw new InvalidOperationException("Connection has already been initialized.");

			_socket = socket;

			this.Status = ConnectionStatus.Open;
			this.Address = ((IPEndPoint)_socket.RemoteEndPoint).ToString();

			this.ConfigureSocket(_socket);
		}

		/// <summary>
		/// Called once the connection's socket was set, before any data is
		/// sent or received, to give the connection a chance to modify the
		/// socket's options.
		/// </summary>
		/// <remarks>
		/// The socket's default options are left untouched unless this method
		/// is overridden. Latency sensitive applications will typically want
		/// to disable Nagle's algorithm here by setting NoDelay.
		/// </remarks>
		/// <param name="socket"></param>
		protected virtual void ConfigureSocket(Socket socket)
		{
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void Close()
			=> this.Close(ConnectionCloseType.Closed);

		/// <summary>
		/// Closes the connection.
		/// </summary>
		/// <param name="type"></param>
		public void Close(ConnectionCloseType type)
		{
			if (this.Status == ConnectionStatus.Closed)
				return;

			this.Status = ConnectionStatus.Closed;

			try { _coalesceTimer?.Dispose(); } catch { }
			try { _socket.Shutdown(SocketShutdown.Both); } catch { }
			try { _socket.Close(); } catch { }
			try { this.NotifyClosed(type); } catch { }

			try
			{
				var unsentItems = new List<SendItem>();

				lock (_sendSyncLock)
				{
					while (_sendQueue.Count > 0)
						unsentItems.Add(_sendQueue.Dequeue());
				}

				foreach (var item in unsentItems)
					item.SendCallback?.Invoke(item.Buffer, item.Length, PostSendType.Closed);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Calls OnClosed method and raises Closed event.
		/// </summary>
		/// <param name="type"></param>
		private void NotifyClosed(ConnectionCloseType type)
		{
			this.OnClosed(type);
			this.Closed?.Invoke(this, type);
		}

		/// <summary>
		/// Starts receiving data.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown if connection hasn't been initialized yet.
		/// </exception>
		public void BeginReceive()
		{
			if (_socket == null)
				throw new InvalidOperationException("Connection hasn't been initialized yet.");

			_socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, this.OnReceive, null);

			lock (_firstConnectSyncLock)
			{
				if (!_raisedConnected)
				{
					this.OnConnected();
					_raisedConnected = true;
				}
			}
		}

		/// <summary>
		/// Called on incoming data.
		/// </summary>
		/// <param name="ar"></param>
		private void OnReceive(IAsyncResult ar)
		{
			try
			{
				var length = _socket.EndReceive(ar);

				// If the received length is 0, the connection was closed.
				if (length == 0)
				{
					this.Status = ConnectionStatus.Closed;
					this.NotifyClosed(ConnectionCloseType.Disconnected);

					return;
				}

				this.ReceiveData(_buffer, length);

				this.BeginReceive();
			}
			// ObjectDisposedException can be thrown for various reasons,
			// such as tryting to use the socket after it was already
			// closed.
			catch (ObjectDisposedException)
			{
			}
			// SocketExceptions are thrown if the connection is
			// unexpectedly and/or abruptly closed by the client or
			// server, such as when the process was killed.
			catch (SocketException)
			{
				try { this.Close(ConnectionCloseType.Lost); } catch { }
			}
			catch (Exception ex)
			{
				try { this.OnReceiveException(ex); } catch { }
				try { this.Close(ConnectionCloseType.Disconnected); } catch { }
			}
		}

		/// <summary>
		/// Called after the connection was accepted by the server
		/// and it's ready to be used.
		/// </summary>
		protected virtual void OnConnected()
		{
		}

		/// <summary>
		/// Called when the connection was closed, raises Closed event.
		/// </summary>
		/// <param name="type"></param>
		protected virtual void OnClosed(ConnectionCloseType type)
		{
		}

		/// <summary>
		/// Called if an exception occurs while receiving data,
		/// raises ReceiveException event.
		/// </summary>
		protected virtual void OnReceiveException(Exception ex)
		{
			this.ReceiveException?.Invoke(this, ex);
		}

		/// <summary>
		/// Called on incoming data.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="length"></param>
		protected abstract void ReceiveData(byte[] buffer, int length);

		/// <summary>
		/// Sends the full byte array via socket.
		/// </summary>
		/// <remarks>
		/// The data is sent asynchronously, putting it in a send queue if
		/// another send operation is still in progress. This means the
		/// data is not neccesarily sent right away, but the method does
		/// return immediately. The data needs to remain valid until the
		/// send operation is completed. If necessary, a callback can be
		/// used to be notified when the data was handled and is no longer
		/// needed.
		/// </remarks>
		/// <param name="data"></param>
		public virtual void Send(byte[] data)
			=> this.Send(data, data.Length);

		/// <summary>
		/// Sends the given amount of bytes in data via socket.
		/// </summary>
		/// <remarks>
		/// The data is sent asynchronously, putting it in a send queue if
		/// another send operation is still in progress. This means the
		/// data is not neccesarily sent right away, but the method does
		/// return immediately. The data needs to remain valid until the
		/// send operation is completed. If necessary, a callback can be
		/// used to be notified when the data was handled and is no longer
		/// needed.
		/// </remarks>
		/// <param name="data">The data to send.</param>
		/// <param name="length">The number of bytes to send from the array.</param>
		public virtual void Send(byte[] data, int length)
			=> this.Send(data, length, null);

		/// <summary>
		/// Sends the given amount of bytes in data via socket.
		/// </summary>
		/// <remarks>
		/// The data is sent asynchronously, putting it in a send queue if
		/// another send operation is still in progress. This means the
		/// data is not neccesarily sent right away, but the method does
		/// return immediately. The data needs to remain valid until the
		/// send operation is completed. The callback can be used to be
		/// notified when the data was handled and is no longer needed.
		///
		/// The callback is invoked after the data was sent or the
		/// connection was closed while the data was still in the send
		/// queue. It's intended to be used for cleaning up resources,
		/// such as returning rented buffers to their pools.
		/// </remarks>
		/// <param name="data">The data to send.</param>
		/// <param name="length">The number of bytes to send from the array.</param>
		/// <param name="callback">The callback to invoke after the data is sent.</param>
		public virtual void Send(byte[] data, int length, SendCallback callback)
		{
			if (this.Status != ConnectionStatus.Open)
			{
				callback?.Invoke(data, length, PostSendType.Closed);
				return;
			}

			lock (_sendSyncLock)
			{
				_sendQueue.Enqueue(new SendItem(data, length, callback));

				if (_isSending || _coalescing)
					return;

				var coalescingTime = this.SendCoalescingTime;

				if (coalescingTime <= TimeSpan.Zero)
				{
					_isSending = true;
					this.BeginSend();
					return;
				}

				if (_coalesceTimer == null)
					_coalesceTimer = new Timer(this.OnCoalesceElapsed, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

				_coalescing = true;
				_coalesceTimer.Change(coalescingTime, Timeout.InfiniteTimeSpan);
			}
		}

		/// <summary>
		/// Called once the coalescing window elapsed, sending everything
		/// that was queued up during it.
		/// </summary>
		/// <param name="state"></param>
		private void OnCoalesceElapsed(object state)
		{
			lock (_sendSyncLock)
			{
				_coalescing = false;

				if (_isSending || _sendQueue.Count == 0)
					return;

				_isSending = true;
				this.BeginSend();
			}
		}

		/// <summary>
		/// Checks for queued packets and begins sending them if there are
		/// any.
		/// </summary>
		private void BeginSend()
		{
			try
			{
				List<ArraySegment<byte>> buffers;

				lock (_sendSyncLock)
				{
					if (_sendQueue.Count == 0)
					{
						_isSending = false;
						return;
					}

					// Send everything that's queued up as one operation, so a burst
					// of small messages doesn't become a burst of packets. The items
					// are dequeued once they were fully sent.
					buffers = new List<ArraySegment<byte>>(_sendQueue.Count);
					var offset = _sendOffset;

					foreach (var item in _sendQueue)
					{
						buffers.Add(new ArraySegment<byte>(item.Buffer, offset, item.Length - offset));
						offset = 0;
					}
				}

				_socket.BeginSend(buffers, SocketFlags.None, this.OnSend, null);
			}
			catch
			{
				this.Close(ConnectionCloseType.Disconnected);
			}
		}

		/// <summary>
		/// Called when data has been sent.
		/// </summary>
		/// <param name="ar"></param>
		private void OnSend(IAsyncResult ar)
		{
			try
			{
				var bytesSent = _socket.EndSend(ar);
				var sentItems = new List<SendItem>();

				lock (_sendSyncLock)
				{
					// A send doesn't necessarily cover every buffer it was given,
					// so only the items that went out completely are dequeued and
					// the rest is picked up again by the next send.
					while (_sendQueue.Count > 0)
					{
						var remaining = _sendQueue.Peek().Length - _sendOffset;

						if (bytesSent < remaining)
						{
							_sendOffset += bytesSent;
							break;
						}

						bytesSent -= remaining;
						_sendOffset = 0;
						sentItems.Add(_sendQueue.Dequeue());
					}
				}

				foreach (var item in sentItems)
					item.SendCallback?.Invoke(item.Buffer, item.Length, PostSendType.Sent);

				// Try to send the next packets in the queue
				this.BeginSend();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException)
			{
				try { this.Close(ConnectionCloseType.Lost); } catch { }
			}
			catch (Exception ex)
			{
				try { this.OnReceiveException(ex); } catch { }
				try { this.Close(ConnectionCloseType.Disconnected); } catch { }
			}
		}

		private readonly struct SendItem
		{
			public readonly byte[] Buffer;
			public readonly int Length;
			public readonly SendCallback SendCallback;

			public SendItem(byte[] buffer, int length, SendCallback sendCallback)
			{
				this.Buffer = buffer;
				this.Length = length;
				this.SendCallback = sendCallback;
			}
		}

		/// <summary>
		/// Specifies the type of a post-send callback.
		/// </summary>
		public enum PostSendType
		{
			/// <summary>
			/// The data was sent as part of a normal send operation.
			/// </summary>
			Sent,

			/// <summary>
			/// The data wasn't sent because the connection was closed
			/// while the data was still in the send queue.
			/// </summary>
			Closed,
		}

		/// <summary>
		/// A function type for post-send callbacks.
		/// </summary>
		/// <param name="data">The array that helt the sent data.</param>
		/// <param name="length">The length of the actual data in the array.</param>
		/// <param name="type">The type for the callback situation.</param>
		public delegate void SendCallback(byte[] data, int length, PostSendType type);
	}

	/// <summary>
	/// A network connection's status.
	/// </summary>
	public enum ConnectionStatus
	{
		/// <summary>
		/// Connection is closed and no data can be sent or received.
		/// </summary>
		Closed,

		/// <summary>
		/// Connection is open and data can be sent and received.
		/// </summary>
		Open,
	}

	/// <summary>
	/// The way a connection was closed.
	/// </summary>
	public enum ConnectionCloseType
	{
		/// <summary>
		/// The connection was closed by the host.
		/// </summary>
		Closed,

		/// <summary>
		/// The connection was closed by the client.
		/// </summary>
		Disconnected,

		/// <summary>
		/// The connection was lost unexpectedly.
		/// </summary>
		Lost,

		/// <summary>
		/// The connection was rejected by the acceptor.
		/// </summary>
		Rejected,
	}
}
