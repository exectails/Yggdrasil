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
		private DoubleNewLineFramer _doubleNewLineFramer;
		private WebSocketFramer _webSocketFramer;
		private bool _upgraded = false;
		private List<WebSocketFrame> _frames = new List<WebSocketFrame>();

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public WebSocketConnection()
		{
			_doubleNewLineFramer = new DoubleNewLineFramer(1024 * 8);
			_doubleNewLineFramer.MessageReceived += this.OnHttpMessageReceived;

			_webSocketFramer = new WebSocketFramer(1024 * 90);
			_webSocketFramer.MessageReceived += this.OnWebSocketMessageReceived;
		}

		/// <summary>
		/// Reads data from buffer, creating messages out of it.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="length"></param>
		protected override void ReveiveData(byte[] buffer, int length)
		{
			if (!_upgraded)
			{
				_doubleNewLineFramer.ReceiveData(buffer, length);
			}
			else
			{
				_webSocketFramer.ReceiveData(buffer, length);
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

			var response = Encoding.UTF8.GetBytes(
				"HTTP/1.1 101 Switching Protocols\r\n" +
				"Connection: Upgrade\r\n" +
				"Upgrade: websocket\r\n" +
				"Sec-WebSocket-Accept: " + serverKey + "\r\n" +
				"\r\n"
			);

			_upgraded = true;

			this.Send(response);
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
				var response = _webSocketFramer.Frame(frame.Payload, false, FrameOpCode.Close);
				this.Send(response);
				this.Close();
			}
			else if (frame.OpCode == FrameOpCode.Ping)
			{
				var response = _webSocketFramer.Frame(frame.Payload, false, FrameOpCode.Pong);
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
		/// <param name="opCode"></param>
		/// <param name="data"></param>
		protected abstract void OnMessageReceived(MessageType type, byte[] data);

		/// <summary>
		/// Sends ping frame to client.
		/// </summary>
		public void Ping()
		{
			var response = _webSocketFramer.Frame(null, false, FrameOpCode.Ping);
			this.Send(response);
		}

		/// <summary>
		/// Sends error response to client and closes connection.
		/// </summary>
		private void HandleNonWebSocketRequest()
		{
			var now = DateTime.Now.ToUniversalTime();
			var content = "This server only processes WebSocket requests.";

			var contentBytes = Encoding.UTF8.GetBytes(content);
			var contentUtf8 = Encoding.UTF8.GetString(contentBytes);

			var response = new StringBuilder();
			response.AppendFormat("HTTP/1.1 405 Method Not Allowed\r\n");
			response.AppendFormat("Date: {0:r}\r\n", now);
			response.AppendFormat("Server: Yggdrasil-WebSocketConnection\r\n");
			response.AppendFormat("Last-Modified: {0:r}\r\n", now);
			response.AppendFormat("Content-Length: {0}\r\n", contentBytes.Length);
			response.AppendFormat("Content-Type: text/html\r\n");
			response.AppendFormat("Connection: Closed\r\n");
			if (contentBytes.Length > 0)
			{
				response.AppendFormat("\r\n");
				response.Append(contentUtf8);
			}
			response.AppendFormat("\r\n\r\n");

			var responseBytes = Encoding.UTF8.GetBytes(response.ToString());

			this.Send(responseBytes);
			this.Close();
		}

		/// <summary>
		/// Sends error response to client and closes connection.
		/// </summary>
		private void HandleIncorrectVersionRequest()
		{
			var now = DateTime.Now.ToUniversalTime();
			var content = "This server only supports WebSocket version 13.";

			var contentBytes = Encoding.UTF8.GetBytes(content);
			var contentUtf8 = Encoding.UTF8.GetString(contentBytes);

			var response = new StringBuilder();
			response.AppendFormat("HTTP/1.1 400 Bad Request\r\n");
			response.AppendFormat("Date: {0:r}\r\n", now);
			response.AppendFormat("Server: Yggdrasil-WebSocketConnection\r\n");
			response.AppendFormat("Last-Modified: {0:r}\r\n", now);
			response.AppendFormat("Content-Length: {0}\r\n", contentBytes.Length);
			response.AppendFormat("Content-Type: text/html\r\n");
			response.AppendFormat("Connection: Closed\r\n");
			response.AppendFormat("Sec-WebSocket-Version: 13\r\n");
			if (contentBytes.Length > 0)
			{
				response.AppendFormat("\r\n");
				response.Append(contentUtf8);
			}
			response.AppendFormat("\r\n\r\n");

			var responseBytes = Encoding.UTF8.GetBytes(response.ToString());

			this.Send(responseBytes);
			this.Close();
		}
	}

	/// <summary>
	/// Specifies the type of a received message.
	/// </summary>
	public enum MessageType
	{
		Text,
		Binary,
	}
}
