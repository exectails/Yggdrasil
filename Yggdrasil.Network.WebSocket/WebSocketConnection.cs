// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yggdrasil.Network.Framing;
using Yggdrasil.Network.TCP;
using Yggdrasil.Security.Hashing;

namespace Yggdrasil.Network.WebSocket
{
	/// <summary>
	/// Represents a TCP/WebSocket connection.
	/// </summary>
	public abstract class WebSocketConnection : TcpConnection
	{
		private List<WebSocketFrame> _frames = new List<WebSocketFrame>();

		/// <summary>
		/// The HTTP framer used by this instance.
		/// </summary>
		public DoubleNewLineFramer DoubleNewLineFramer { get; protected set; }

		/// <summary>
		/// The WebSocket framer used by this instance.
		/// </summary>
		public WebSocketFramer WebSocketFramer { get; protected set; }

		/// <summary>
		/// Returns true once the connection was upgraded from HTTP to
		/// WebSocket.
		/// </summary>
		public bool Upgraded { get; protected set; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public WebSocketConnection()
		{
			this.DoubleNewLineFramer = new DoubleNewLineFramer(1024 * 8);
			this.DoubleNewLineFramer.MessageReceived += this.OnHttpMessageReceived;

			this.WebSocketFramer = new WebSocketFramer(1024 * 90);
			this.WebSocketFramer.MessageReceived += this.OnWebSocketMessageReceived;
		}

		/// <summary>
		/// Reads data from buffer, creating messages out of it.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="length"></param>
		protected override void ReveiveData(byte[] buffer, int length)
		{
			if (!this.Upgraded)
			{
				this.DoubleNewLineFramer.ReceiveData(buffer, length);
			}
			else
			{
				this.WebSocketFramer.ReceiveData(buffer, length);
			}
		}

		/// <summary>
		/// Call-back for when a full HTTP message was received.
		/// </summary>
		/// <param name="requestString"></param>
		private void OnHttpMessageReceived(string requestString)
		{
			var request = new HttpRequest(requestString);

			if (request.Headers.ContainsKey("Upgrade") && request.Headers["Upgrade"] == "websocket" && request.Headers.ContainsKey("Sec-WebSocket-Key"))
			{
				if (!request.Headers.ContainsKey("Sec-WebSocket-Version") || !int.TryParse(request.Headers["Sec-WebSocket-Version"], out var version) || version != 13)
				{
					this.HandleIncorrectVersionRequest();
					return;
				}

				this.UpgradeConnection(request);
			}
			else
			{
				this.HandleNonWebSocketRequest();
			}
		}

		/// <summary>
		/// Sends handshake response.
		/// </summary>
		/// <param name="request"></param>
		private void UpgradeConnection(HttpRequest request)
		{
			var clientKey = request.Headers["Sec-WebSocket-Key"];
			var serverKey = Convert.ToBase64String(SHA1.Encode(Encoding.UTF8.GetBytes(clientKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

			var response = new HttpResponse(101, "Switching Protocols");
			response.Headers["Connection"] = "Upgrade";
			response.Headers["Upgrade"] = "websocket";
			response.Headers["Sec-WebSocket-Accept"] = serverKey;

			var responseBytes = response.ToBytes();

			this.Upgraded = true;

			this.Send(responseBytes);
		}

		/// <summary>
		/// Call-back for when a full WebSocket frame was received.
		/// </summary>
		/// <param name="buffer"></param>
		private void OnWebSocketMessageReceived(byte[] buffer)
		{
			var frame = new WebSocketFrame(buffer);

			if (frame.OpCode == FrameOpCode.Close)
			{
				var response = this.WebSocketFramer.Frame(frame.Payload, false, FrameOpCode.Close);
				this.Send(response);
				this.Close();
			}
			else if (frame.OpCode == FrameOpCode.Ping)
			{
				var response = this.WebSocketFramer.Frame(frame.Payload, false, FrameOpCode.Pong);
				this.Send(response);
			}
			else if (frame.OpCode == FrameOpCode.Pong)
			{
				// Received pong
			}
			else if (frame.OpCode == FrameOpCode.TextData || frame.OpCode == FrameOpCode.BinaryData || frame.OpCode == FrameOpCode.Continuation)
			{
				if (frame.OpCode == FrameOpCode.Continuation && _frames.Count == 0)
					throw new InvalidOperationException("Continue frame sent without any frames to be continued.");

				// Add frames to a list until the last one (Fin == true)
				// comes in, at which point the data is combined to the
				// full message.
				_frames.Add(frame);

				if (!frame.Fin)
					return;

				var type = (_frames[0].OpCode == FrameOpCode.TextData ? MessageType.Text : MessageType.Binary);

				byte[] payload;
				if (_frames.Count == 1)
				{
					payload = _frames[0].Payload;
				}
				else
				{
					payload = _frames.SelectMany(a => a.Payload).ToArray();
				}

				_frames.Clear();

				this.OnMessageReceived(type, payload);
			}
		}

		/// <summary>
		/// Called when a new, complete message was received.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="data"></param>
		protected abstract void OnMessageReceived(MessageType type, byte[] data);

		/// <summary>
		/// Sends ping frame to client.
		/// </summary>
		public void Ping()
		{
			var response = this.WebSocketFramer.Frame(null, false, FrameOpCode.Ping);
			this.Send(response);
		}

		/// <summary>
		/// Sends error response to client and closes connection.
		/// </summary>
		private void HandleNonWebSocketRequest()
		{
			var response = new HttpResponse(400, "Bad Request");
			response.Headers["Date"] = DateTime.Now.ToUniversalTime().ToString("r");
			response.Headers["Server"] = "Yggdrasil-WebSocketConnection";
			response.Headers["Content-Type"] = "text/html";
			response.Headers["Connection"] = "Closed";
			response.Content = "This server only processes WebSocket requests.";

			var responseBytes = response.ToBytes();

			this.Send(responseBytes);
			this.Close();
		}

		/// <summary>
		/// Sends error response to client and closes connection.
		/// </summary>
		private void HandleIncorrectVersionRequest()
		{
			var response = new HttpResponse(400, "Bad Request");
			response.Headers["Date"] = DateTime.Now.ToUniversalTime().ToString("r");
			response.Headers["Server"] = "Yggdrasil-WebSocketConnection";
			response.Headers["Content-Type"] = "text/html";
			response.Headers["Connection"] = "Closed";
			response.Headers["Sec-WebSocket-Version"] = "13";
			response.Content = "This server only supports WebSocket version 13.";

			var responseBytes = response.ToBytes();

			this.Send(responseBytes);
			this.Close();
		}
	}

	/// <summary>
	/// Specifies the type of a received message.
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Plain text data.
		/// </summary>
		Text,

		/// <summary>
		/// Binary data.
		/// </summary>
		Binary,
	}
}
