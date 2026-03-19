using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
		private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
		private bool _isSending;

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

			try { _socket.Shutdown(SocketShutdown.Both); } catch { }
			try { _socket.Close(); } catch { }
			try { this.NotifyClosed(type); } catch { }
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
		/// Sends data via socket.
		/// </summary>
		/// <param name="data"></param>
		public virtual void Send(byte[] data)
		{
			if (this.Status != ConnectionStatus.Open)
				return;

			lock (_sendSyncLock)
			{
				_sendQueue.Enqueue(data);

				if (!_isSending)
				{
					_isSending = true;
					this.BeginSend();
				}
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
				byte[] data;

				lock (_sendSyncLock)
				{
					if (_sendQueue.Count == 0)
					{
						_isSending = false;
						return;
					}

					// Get data to send, dequeue after it was sent
					data = _sendQueue.Peek();
				}

				_socket.BeginSend(data, 0, data.Length, SocketFlags.None, this.OnSend, null);
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
				_socket.EndSend(ar);

				lock (_sendSyncLock)
				{
					_sendQueue.Dequeue();
				}

				// Try to send next packet in the queue
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
