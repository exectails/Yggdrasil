// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Yggdrasil.Network.Framing;
using Yggdrasil.Network.TCP;

namespace Yggdrasil.Network.WebSocket
{
	/// <summary>
	/// Represents a TCP/WebSocket connection.
	/// </summary>
	public abstract class WebSocketConnection : TcpConnection
	{
		private DoubleNewLineFramer _doubleNewLineFramer;
		private WebSocketFramer _webSocketFramer;
		private bool _shookHands = false;
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
			if (!_shookHands)
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
			// Chrome upgrade request example
			// GET / HTTP/1.1
			// Host: 127.0.0.1:8080
			// Connection: Upgrade
			// Pragma: no-cache
			// Cache-Control: no-cache
			// Upgrade: websocket
			// Origin: http://127.0.0.1:8080
			// Sec-WebSocket-Version: 13
			// User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36
			// Accept-Encoding: gzip, deflate, br
			// Accept-Language: en-US,en;q=0.8,de-DE;q=0.6,de;q=0.4
			// Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits
			// Sec-WebSocket-Key: vLjSDjZHPYlfqo9WoJGTVw==

			if (requestString.Contains("Upgrade: websocket") && requestString.Contains("Sec-WebSocket-Key: "))
			{
				var clientKey = new Regex("Sec-WebSocket-Key: (.*)").Match(requestString).Groups[1].Value.Trim();
				var serverKey = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(clientKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

				var response = Encoding.UTF8.GetBytes(
					"HTTP/1.1 101 Switching Protocols\r\n" +
					"Connection: Upgrade\r\n" +
					"Upgrade: websocket\r\n" +
					"Sec-WebSocket-Accept: " + serverKey + "\r\n" +
					"\r\n"
				);

				_shookHands = true;

				this.Send(response);
			}
			else
			{
				this.HandleNonWebSocketRequest();
			}
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

				var opCode = _frames[0].OpCode;

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

				this.OnMessageReceived(opCode, payload);
			}
		}

		/// <summary>
		/// Called when a new, complete message was received.
		/// </summary>
		/// <param name="opCode"></param>
		/// <param name="data"></param>
		protected abstract void OnMessageReceived(FrameOpCode opCode, byte[] data);

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
	}
}
