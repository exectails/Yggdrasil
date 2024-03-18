using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;
using Yggdrasil.Network.Communication.Messages;
using Yggdrasil.Network.TCP;

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
		private readonly Dictionary<string, List<string>> _channelSubscribers = new Dictionary<string, List<string>>();
		private readonly Dictionary<long, ICommMessage> _responseMessages = new Dictionary<long, ICommMessage>();

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
			{
				this.RemoveConnection(conn.Name);
				this.ClientDisconnected?.Invoke(conn.Name);
			}
		}

		/// <summary>
		/// Removes connection from all internal lists.
		/// </summary>
		/// <param name="name"></param>
		private void RemoveConnection(string name)
		{
			_connections.Remove(name);
			_clients.Remove(name);

			foreach (var channel in _channelSubscribers.Values)
				channel.Remove(name);
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
			client.MessageReceived += this.OnClientMessageReceived;
			client.Disconnected += this.OnDisconnected;
			client.Connect(endPoint);

			_clients[name] = client;

			this.Send(name, new HelloMessage(this.Name));
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

			this.RemoveConnection(client.Name);
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

			if (conn.Name == null && !(message is HelloMessage))
			{
				conn.Close();
				return;
			}

			switch (message)
			{
				case HelloMessage m:
				{
					conn.Name = m.Name;
					_connections[conn.Name] = conn;

					this.ClientConnected?.Invoke(conn.Name);
					break;
				}
				case SubscribeChannelMessage m:
				{
					if (!_channelSubscribers.TryGetValue(m.ChannelName, out var subscribers))
						_channelSubscribers[m.ChannelName] = subscribers = new List<string>();

					subscribers.Add(conn.Name);
					break;
				}
				case BroadcastMessage m:
				{
					this.Broadcast(m.ChannelName, m.Message);
					break;
				}
				case ResponseMessage m:
				{
					lock (_responseMessages)
						_responseMessages[m.Id] = m.Message;
					break;
				}
				default:
				{
					this.MessageReceived?.Invoke(conn.Name, message);
					break;
				}
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

			switch (message)
			{
				case HelloMessage m:
				{
					break;
				}
				case ResponseMessage m:
				{
					lock (_responseMessages)
						_responseMessages[m.Id] = m.Message;
					break;
				}
				default:
				{
					this.MessageReceived?.Invoke(client.Name, message);
					break;
				}
			}
		}

		/// <summary>
		/// Subscribes to messages broadcasted on the given channel.
		/// </summary>
		/// <param name="receiverName"></param>
		/// <param name="channelName"></param>
		public void Subscribe(string receiverName, string channelName)
		{
			this.Send(receiverName, new SubscribeChannelMessage(channelName));
		}

		/// <summary>
		/// Broadcasts object to all subscribers of the given channel.
		/// </summary>
		/// <param name="channelName"></param>
		/// <param name="message"></param>
		public void Broadcast(string channelName, ICommMessage message)
		{
			if (!_channelSubscribers.TryGetValue(channelName, out var subscriberNames))
				return;

			var buffer = this.SerializeMessage(message);

			foreach (var subscriberName in subscriberNames)
				this.Send(subscriberName, buffer);
		}

		/// <summary>
		/// Sends message to the receiver.
		/// </summary>
		/// <param name="receiverName"></param>
		/// <param name="message"></param>
		public void Send(string receiverName, ICommMessage message)
		{
			var buffer = this.SerializeMessage(message);
			this.Send(receiverName, buffer);
		}

		/// <summary>
		/// Sends buffer to the receiver.
		/// </summary>
		/// <param name="receiverName"></param>
		/// <param name="buffer"></param>
		private void Send(string receiverName, byte[] buffer)
		{
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
		/// Sends a request to the receiver and waits for a response.
		/// Returns either the response or null if the request failed.
		/// </summary>
		/// <typeparam name="TResMessage"></typeparam>
		/// <param name="receiver"></param>
		/// <param name="reqMessage"></param>
		/// <returns></returns>
		public async Task<TResMessage> RequestResponse<TResMessage>(string receiver, ICommMessage reqMessage) where TResMessage : ICommMessage
		{
			var message = new RequestMessage(reqMessage);

			var resMessage = default(ICommMessage);
			var timeout = DateTime.Now.AddSeconds(5);

			this.Send(receiver, message);

			do
			{
				await Task.Delay(1);

				lock (_responseMessages)
				{
					if (_responseMessages.TryGetValue(message.Id, out resMessage))
						_responseMessages.Remove(message.Id);
				}
			}
			while (resMessage == null && DateTime.Now < timeout);

			return (TResMessage)resMessage;
		}

		/// <summary>
		/// Serializes message to a byte array and returns it.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private byte[] SerializeMessage(ICommMessage message)
		{
			using (var ms = new MemoryStream())
			using (var bw = new BinaryWriter(ms))
			{
				var type = message.GetType();
				var typeName = type.AssemblyQualifiedName;
				var json = JsonSerializer.Serialize(message, type);

				bw.Write(typeName);
				bw.Write(json);

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
			using (var br = new BinaryReader(ms))
			{
				var typeName = br.ReadString();
				var json = br.ReadString();

				var type = Type.GetType(typeName);
				var obj = JsonSerializer.Deserialize(json, type);

				if (!(obj is ICommMessage message))
					throw new ArgumentException("Buffer did not contain a comunicator message.");

				return message;
			}
		}
	}
}
