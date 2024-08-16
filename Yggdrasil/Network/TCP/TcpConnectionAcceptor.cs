using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Yggdrasil.Network.Communication;

namespace Yggdrasil.Network.TCP
{
	/// <summary>
	/// Listens for and accepts TCP connections.
	/// </summary>
	public class TcpConnectionAcceptor<TConnection> where TConnection : TcpConnection, new()
	{
		private Socket _socket;

		private readonly List<TConnection> _connections = new List<TConnection>();

		/// <summary>
		/// Local end point the acceptor is listening on for new connections.
		/// </summary>
		public IPEndPoint LocalEndPoint { get; private set; }

		/// <summary>
		/// Returns address this acceptor is listening on.
		/// </summary>
		/// <example>
		/// 127.0.0.1:10000
		/// </example>
		public string Address { get { return this.LocalEndPoint.ToString(); } }

		/// <summary>
		/// Raised when an exception occures while accepting a connection.
		/// </summary>
		public event Action<Exception> AcceptionException;

		/// <summary>
		/// Raised when a connection was successfully accepted.
		/// </summary>
		public event Action<TConnection> ConnectionAccepted;

		/// <summary>
		/// Raised when a connection was rejected.
		/// </summary>
		public event Action<TConnection, string> ConnectionRejected;

		/// <summary>
		/// Function to use to check if an incoming connection is valid.
		/// Returning true means the connection should be accepted, false
		/// means it should be rejected.
		/// </summary>
		public Func<TConnection, bool> ConnectionChecker { get; set; }

		/// <summary>
		/// Raised when a connection was closed or lost.
		/// </summary>
		public event Action<TConnection, ConnectionCloseType> ConnectionClosed;

		/// <summary>
		/// Creates new instance of TcpConnectionAcceptor, that will listen
		/// on the given end point.
		/// </summary>
		/// <param name="localEndPoint">End point to listen on.</param>
		public TcpConnectionAcceptor(IPEndPoint localEndPoint)
		{
			this.LocalEndPoint = localEndPoint;
		}

		/// <summary>
		/// Creates new instance of TcpConnectionAcceptor, that will listen
		/// on the given IP and port.
		/// </summary>
		/// <param name="host">IP to listen on.</param>
		/// <param name="port">Port to listen on.</param>
		public TcpConnectionAcceptor(string host, int port)
			: this(new IPEndPoint(IPAddress.Parse(host), port))
		{
		}

		/// <summary>
		/// Creates new instance of TcpConnectionAcceptor, that will listen
		/// on any IP and the given port.
		/// </summary>
		/// <param name="port">Port to listen on.</param>
		public TcpConnectionAcceptor(int port)
			: this(new IPEndPoint(IPAddress.Any, port))
		{
		}

		/// <summary>
		/// Creates new instance of TcpConnectionAcceptor, that will listen
		/// on any IP and a random free port.
		/// </summary>
		public TcpConnectionAcceptor()
			: this(new IPEndPoint(IPAddress.Any, 0))
		{
		}

		/// <summary>
		/// Starts listening for new connections.
		/// </summary>
		public void Listen()
		{
			this.Listen(10);
		}

		/// <summary>
		/// Starts listening for new connections.
		/// </summary>
		/// <param name="backlog">Maximum queue for new connections.</param>
		public void Listen(int backlog)
		{
			this.ResetSocket();

			_socket.Bind(this.LocalEndPoint);
			this.LocalEndPoint = (IPEndPoint)_socket.LocalEndPoint;
			_socket.Listen(backlog);

			this.BeginAccept();
		}

		/// <summary>
		/// Stop accepting new connections.
		/// </summary>
		public void Stop()
		{
			this.ResetSocket();
		}

		/// <summary>
		/// Resets socket, disconnecting everybody. Does not restart
		/// listening automatically.
		/// </summary>
		private void ResetSocket()
		{
			if (_socket != null)
			{
				try { _socket.Shutdown(SocketShutdown.Both); }
				catch { }
				try { _socket.Close(2); }
				catch { }
			}

			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Starts accepting new connections.
		/// </summary>
		private void BeginAccept()
		{
			_socket.BeginAccept(this.OnAccept, null);
		}

		/// <summary>
		/// Called when a client connected.
		/// </summary>
		/// <param name="ar"></param>
		private void OnAccept(IAsyncResult ar)
		{
			try
			{
				var connectionSocket = _socket.EndAccept(ar);

				var connection = new TConnection();
				connection.Init(connectionSocket);

				if (this.ConnectionChecker != null && !this.ConnectionChecker.Invoke(connection))
				{
					this.ConnectionRejected?.Invoke(connection, "Connection rejected by server.");
					connection.Close(ConnectionCloseType.Rejected);
					return;
				}

				connection.Closed += this.OnClosed;

				this.OnAccepted(connection);

				connection.BeginReceive();

				this.BeginAccept();
			}
			catch (ObjectDisposedException)
			{
				// Don't BeginAccept if disposed.
			}
			catch (Exception ex)
			{
				this.AcceptionException?.Invoke(ex);

				this.BeginAccept();
			}
		}

		/// <summary>
		/// Called when a connection was accepted.
		/// </summary>
		/// <param name="conn"></param>
		private void OnAccepted(TConnection conn)
		{
			lock (_connections)
				_connections.Add(conn);

			this.ConnectionAccepted?.Invoke(conn);
		}

		/// <summary>
		/// Called when a connection was closed.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="type"></param>
		private void OnClosed(TcpConnection conn, ConnectionCloseType type)
		{
			var tConn = conn as TConnection;

			lock (_connections)
				_connections.Remove(tConn);

			this.ConnectionClosed?.Invoke(tConn, type);
		}

		/// <summary>
		/// Returns a list of all active connections.
		/// </summary>
		/// <returns></returns>
		public TConnection[] GetAllConnections()
		{
			lock (_connections)
				return _connections.ToArray();
		}
	}
}
