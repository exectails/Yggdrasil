using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;
using Yggdrasil.Network.Framing;
using Yggdrasil.Network.TCP;

namespace Yggdrasil.Test.Network
{
	public class TcpConnectionAcceptorTests
	{
		internal void Wait(int ms)
		{
			var end = DateTime.Now.AddMilliseconds(ms);
			while (DateTime.Now < end) ;
		}

		[Fact]
		public void Listen()
		{
			var acceptor = new TcpConnectionAcceptor<TestConnection>();
			acceptor.Listen();
			Assert.Equal("0.0.0.0:" + acceptor.LocalEndPoint.Port, acceptor.Address);
			acceptor.Stop();

			acceptor = new TcpConnectionAcceptor<TestConnection>(19489);
			acceptor.Listen();
			Assert.Equal("0.0.0.0:19489", acceptor.Address);
			acceptor.Stop();

			acceptor = new TcpConnectionAcceptor<TestConnection>("127.0.0.1", 27385);
			acceptor.Listen();
			Assert.Equal("127.0.0.1:27385", acceptor.Address);
			acceptor.Stop();

			acceptor = new TcpConnectionAcceptor<TestConnection>(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 39571));
			acceptor.Listen();
			Assert.Equal("127.0.0.1:39571", acceptor.Address);
			acceptor.Stop();
		}

		[Fact]
		public void AcceptingConnections()
		{
			var connections = new List<TestConnection>();
			var exceptions = new List<Exception>();

			var acceptor = new TcpConnectionAcceptor<TestConnection>();
			acceptor.ConnectionAccepted += (conn => connections.Add(conn));
			acceptor.AcceptionException += (ex => exceptions.Add(ex));
			acceptor.Listen();

			var socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			socket1.Connect("127.0.0.1", acceptor.LocalEndPoint.Port);
			socket2.Connect("127.0.0.1", acceptor.LocalEndPoint.Port);

			// Wait a moment for the events to fire.
			Wait(50);

			Assert.True(socket1.Connected);
			Assert.True(socket2.Connected);

			if (exceptions.Count != 0)
				throw exceptions[0];

			Assert.Equal(2, connections.Count);

			foreach (var conn in connections)
				conn.Close();

			acceptor.Stop();
		}

		[Fact]
		public void DataExchange()
		{
			var connections = new List<TestConnection>();
			var exceptions = new List<Exception>();

			var acceptor = new TcpConnectionAcceptor<TestConnection>();
			acceptor.ConnectionAccepted += (conn => connections.Add(conn));
			acceptor.AcceptionException += (ex => exceptions.Add(ex));
			acceptor.Listen();

			var socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			socket1.Connect("127.0.0.1", acceptor.LocalEndPoint.Port);
			socket2.Connect("127.0.0.1", acceptor.LocalEndPoint.Port);

			Wait(50);

			var message1 = new byte[] { 1, 3, 5, 7 };
			var message2 = new byte[] { 8, 52, 45, 6 };
			var message3 = Encoding.UTF8.GetBytes("~foobar~");

			// Test receiving messages
			var framer = new LengthPrefixFramer(50);
			socket1.Send(framer.Frame(message1));
			socket1.Send(framer.Frame(message2));
			socket2.Send(framer.Frame(message3));

			Wait(50);

			Assert.Equal(2, connections[0].Messages.Count);
			Assert.Single(connections[1].Messages);

			Assert.Equal(message1, connections[0].Messages[0]);
			Assert.Equal(message2, connections[0].Messages[1]);
			Assert.Equal(message3, connections[1].Messages[0]);

			// Test receiving invalid message
			Exception receiveException = null;
			connections[0].ReceiveException += ((conn, ex) => receiveException = ex);
			socket1.Send(new byte[] { 0xFF, 0, 0, 0 });

			Wait(50);

			Assert.NotNull(receiveException);
			Assert.IsType<InvalidMessageSizeException>(receiveException);

			// Test sending message
			var message4 = new byte[] { 9, 8, 7, 6 };
			connections[1].Send(message4);

			var buffer = new byte[10];
			var len = socket2.Receive(buffer);
			Console.WriteLine(BitConverter.ToString(buffer));

			Assert.Equal(8, len);
			Assert.Equal(new byte[] { 8, 0, 0, 0, 9, 8, 7, 6, 0, 0 }, buffer);

			// Close everything
			foreach (var conn in connections)
				conn.Close();

			acceptor.Stop();
		}
	}

	public class TestConnection : TcpConnection
	{
		private LengthPrefixFramer _framer;

		public List<byte[]> Messages = new List<byte[]>();

		public TestConnection()
		{
			_framer = new LengthPrefixFramer(10);
			_framer.MessageReceived += this.OnMessageReceived;
		}

		protected override void ReceiveData(byte[] buffer, int length)
		{
			_framer.ReceiveData(buffer, length);
		}

		private void OnMessageReceived(byte[] message)
		{
			this.Messages.Add(message);
		}

		public override void Send(byte[] message)
		{
			var data = _framer.Frame(message);
			base.Send(data);
		}
	}
}
