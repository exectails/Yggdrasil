// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Net;
using System.Net.Sockets;

namespace Yggdrasil.Network
{
	public abstract class TcpClient
	{
		private const int BufferMaxSize = 4 * 1024;

		private byte[] _buffer;
		private Socket _socket;

		/// <summary>
		/// Current status of the connection.
		/// </summary>
		public ConnectionStatus Status { get; private set; }

		/// <summary>
		/// Address of the local end point.
		/// </summary>
		public string LocalAddress
		{
			get
			{
				if (_socket == null)
					throw new InvalidOperationException("Client isn't connected yet.");

				return ((IPEndPoint)_socket.LocalEndPoint).ToString();
			}
		}

		/// <summary>
		/// Address of the remote end point.
		/// </summary>
		public string RemoteAddress
		{
			get
			{
				if (_socket == null)
					throw new InvalidOperationException("Client isn't connected yet.");

				return ((IPEndPoint)_socket.RemoteEndPoint).ToString();
			}
		}

		/// <summary>
		/// Raised when an exception occurs while receiving data.
		/// </summary>
		public event Action<TcpClient, Exception> ReceiveException;

		/// <summary>
		/// Raised when connection was closed.
		/// </summary>
		public event Action<TcpClient, ConnectionCloseType> Disconnected;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public TcpClient()
		{
			_buffer = new byte[BufferMaxSize];
		}

		/// <summary>
		/// Connects to host.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public void Connect(string host, int port)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
			this.Connect(remoteEndPoint);
		}

		/// <summary>
		/// Connects to remote end point.
		/// </summary>
		/// <param name="remoteEndPoint"></param>
		public void Connect(IPEndPoint remoteEndPoint)
		{
			if (_socket != null)
				throw new InvalidOperationException("Create a new TcpClient to establish a new connection.");

			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.Connect(remoteEndPoint);

			this.Status = ConnectionStatus.Open;
			this.BeginReceive();
		}

		/// <summary>
		/// Disconnects client.
		/// </summary>
		public void Disconnect()
		{
			if (this.Status == ConnectionStatus.Closed)
				return;

			this.Status = ConnectionStatus.Closed;

			try { _socket.Shutdown(SocketShutdown.Both); }
			catch { }
			try { _socket.Close(); }
			catch { }

			this.OnDisconnect(ConnectionCloseType.Closed);
		}

		/// <summary>
		/// Called when the client is disconnected in some way, raises
		/// Closed event.
		/// </summary>
		/// <param name="type"></param>
		protected virtual void OnDisconnect(ConnectionCloseType type)
		{
			var ev = this.Disconnected;
			if (ev != null)
				ev(this, type);
		}

		/// <summary>
		/// Starts receiving data.
		/// </summary>
		private void BeginReceive()
		{
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
					this.OnDisconnect(ConnectionCloseType.Disconnected);

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
				this.OnDisconnect(ConnectionCloseType.Lost);
			}
			catch (Exception ex)
			{
				this.OnReceiveException(ex);
				this.Disconnect();
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
}
