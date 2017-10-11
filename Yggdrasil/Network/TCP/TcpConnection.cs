// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Net;
using System.Net.Sockets;

namespace Yggdrasil.Network.TCP
{
	public abstract class TcpConnection
	{
		private const int BufferMaxSize = 4 * 1024;

		private byte[] _buffer;
		private Socket _socket;

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
			var ev = this.Closed;
			if (ev != null)
				ev(this, type);
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
		/// Called if an exception occurs while receiving data,
		/// raises ReceiveException event.
		/// </summary>
		protected virtual void OnReceiveException(Exception ex)
		{
			var ev = this.ReceiveException;
			if (ev != null)
				ev(this, ex);
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

	public enum ConnectionStatus
	{
		Closed,
		Open,
	}

	public enum ConnectionCloseType
	{
		Closed,
		Disconnected,
		Lost,
	}
}
