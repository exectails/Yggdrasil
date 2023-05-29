using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Yggdrasil.Network.Communication.Messages;
using Yggdrasil.Network.TCP;
#pragma warning disable SYSLIB0011
namespace Yggdrasil.Network.Communication
{
	/// <summary>
	/// A network communicator that can be used to set up easy communication
	/// between servers and related applications.
	/// </summary>
	public class Communicator
	{
		private TcpConnectionAcceptor<Connection> _acceptor;
		private readonly Dictionary<string, Connection> _connections = new Dictionary<string, Connection>();
		private readonly Dictionary<string, Client> _clients = new Dictionary<string, Client>();
		private readonly BinaryFormatter _serializer = new BinaryFormatter();

		/// <summary>
		/// Returns the name of the communicator, which it's
		/// known under when communicating.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Returns the local end point the instance is listening on.
		/// </summary>
		public IPEndPoint LocalEndPoint { get; private set; }

		/// <summary>
		/// Called when the communicator receives a message.
		/// </summary>
		public event Action<string, ICommMessage> MessageReceived;

		/// <summary>
		/// Called when a client connected to this communicator.
		/// </summary>
		public event Action<string> ClientConnected;

		/// <summary>
		/// Called when a client disconnected from this communicator.
		/// </summary>
		public event Action<string> ClientDisconnected;

		/// <summary>
		/// Called when a connection to a communicator was closed.
		/// </summary>
		public event Action<string> Disconnected;

		/// <summary>
		/// Creates new instance that will listen on the given end point.
		/// </summary>
		/// <param name="name"></param>
		public Communicator(string name)
		{
			this.Name = name;
		}

		/// <summary>
		/// Sets up listener on the given port.
		/// </summary>
		/// <param name="port"></param>
		public void Listen(int port)
			=> this.Listen(new IPEndPoint(IPAddress.Any, port));

		/// <summary>
		/// Sets up listener on the given address.
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		public void Listen(string ip, int port)
			=> this.Listen(new IPEndPoint(IPAddress.Parse(ip), port));

		/// <summary>
		/// Sets up listener on the given end point.
		/// </summary>
		/// <param name="endPoint"></param>
		public void Listen(IPEndPoint endPoint)
		{
			this.LocalEndPoint = endPoint;

			_acceptor = new TcpConnectionAcceptor<Connection>(this.LocalEndPoint);
			_acceptor.ConnectionAccepted += this.OnConnectionAccepted;
			_acceptor.Listen();
		}

		/// <summary>
		/// Caleld when a connection from a client was accepted.
		/// </summary>
		/// <param name="conn"></param>
		private void OnConnectionAccepted(Connection conn)
		{
			conn.Closed += this.OnConnectionClosed;
			conn.MessageReceived += this.OnConnectionMessageReceived;
		}

		/// <summary>
		/// Called when a client closes the connection.
		/// </summary>
		/// <param name="tcpConn"></param>
		/// <param name="type"></param>
		private void OnConnectionClosed(TcpConnection tcpConn, ConnectionCloseType type)
		{
			var conn = (Connection)tcpConn;
			if (conn.Name != null)
				this.ClientDisconnected?.Invoke(conn.Name);
		}

		/// <summary>
		/// Connects to a communicator at the given address and saves
		/// the connection under the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public void Connect(string name, string host, int port)
			=> this.Connect(name, new IPEndPoint(IPAddress.Parse(host), port));

		/// <summary>
		/// Connects to a communicator at the given address and saves
		/// the connection under the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="endPoint"></param>
		public void Connect(string name, IPEndPoint endPoint)
		{
			if (_clients.TryGetValue(name, out var client) && client.Status == ClientStatus.Connected)
				throw new ArgumentException($"There is already a connection with the ident '{name}'.");

			client = new Client(name);
			client.Disconnected += this.OnDisconnected;
			client.MessageReceived += this.OnClientMessageReceived;
			client.Connect(endPoint);

			_clients[name] = client;

			client.Send(this.SerializeMessage(new HelloMessage() { Name = this.Name }));
		}

		/// <summary>
		/// Called when a connection from this communicator to another
		/// one was closed.
		/// </summary>
		/// <param name="tcpClient"></param>
		/// <param name="type"></param>
		private void OnDisconnected(TcpClient tcpClient, ConnectionCloseType type)
		{
			var client = (Client)tcpClient;
			this.Disconnected?.Invoke(client.Name);
		}

		/// <summary>
		/// Called when a message is received from a connection.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="buffer"></param>
		private void OnConnectionMessageReceived(Connection conn, byte[] buffer)
		{
			var message = this.DeserializeMessage(buffer);

			if (message is HelloMessage m)
			{
				conn.Name = m.Name;
				_connections[conn.Name] = conn;

				this.ClientConnected?.Invoke(conn.Name);
			}
			else if (conn.Name == null)
			{
				conn.Close();
			}
			else
			{
				this.MessageReceived?.Invoke(conn.Name, message);
			}
		}

		/// <summary>
		/// Called when a message is received from a client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="buffer"></param>
		private void OnClientMessageReceived(Client client, byte[] buffer)
		{
			var message = this.DeserializeMessage(buffer);

			if (message is HelloMessage m)
			{
			}
			else
			{
				this.MessageReceived?.Invoke(client.Name, message);
			}
		}

		/// <summary>
		/// Sends object to the receiver.
		/// </summary>
		/// <param name="receiverName"></param>
		/// <param name="message"></param>
		public void Send(string receiverName, ICommMessage message)
			=> this.Send(receiverName, null, message);

		/// <summary>
		/// Sends message to the receiver on the given channel.
		/// </summary>
		/// <param name="receiverName"></param>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		public void Send(string receiverName, string channel, ICommMessage message)
		{
			var buffer = this.SerializeMessage(message);

			if (_clients.TryGetValue(receiverName, out var client))
			{
				client.Send(buffer);
			}
			else if (_connections.TryGetValue(receiverName, out var conn))
			{
				conn.Send(buffer);
			}
			else
			{
				throw new ArgumentException($"There is no communicator with the name '{receiverName}'.");
			}
		}

		/// <summary>
		/// Serializes message to a byte array and returns it.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private byte[] SerializeMessage(ICommMessage message)
		{
			using (var ms = new MemoryStream())
			{
				_serializer.Serialize(ms, message);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Deserializes buffer to an object and returns it.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		private ICommMessage DeserializeMessage(byte[] buffer)
		{
			using (var ms = new MemoryStream(buffer))
			{
				var obj = _serializer.Deserialize(ms);
				if (!(obj is ICommMessage message))
					throw new ArgumentException("Buffer did not contain a comunicator message.");

				return message;
			}
		}
	}
}
