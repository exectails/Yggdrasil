// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
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

		private byte[] _buffer;
		private Socket _socket;

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
		/// Creates new instance.
		/// </summary>
		public TcpConnection()
		{
			_buffer = new byte[BufferMaxSize];
		}

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
		{
			if (this.Status == ConnectionStatus.Closed)
				return;

			this.Status = ConnectionStatus.Closed;

			try { _socket.Shutdown(SocketShutdown.Both); }
			catch { }
			try { _socket.Close(); }
			catch { }

			this.OnClosed(ConnectionCloseType.Closed);
		}

		/// <summary>
		/// Called when the connection was closed, raises Closed event.
		/// </summary>
		/// <param name="type"></param>
		protected virtual void OnClosed(ConnectionCloseType type)
		{
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

			if (!_raisedConnected)
			{
				this.OnConnected();
				_raisedConnected = true;
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

				if (length == 0)
				{
					this.Status = ConnectionStatus.Closed;
					this.OnClosed(ConnectionCloseType.Disconnected);

					return;
				}

				this.ReveiveData(_buffer, length);

				this.BeginReceive();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException)
			{
				this.Status = ConnectionStatus.Closed;
				this.OnClosed(ConnectionCloseType.Lost);
			}
			catch (Exception ex)
			{
				this.OnReceiveException(ex);
				this.Close();
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
		protected abstract void ReveiveData(byte[] buffer, int length);

		/// <summary>
		/// Sends data via socket.
		/// </summary>
		/// <param name="data"></param>
		public virtual void Send(byte[] data)
		{
			if (this.Status == ConnectionStatus.Open)
				_socket.Send(data);
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
	}
}
