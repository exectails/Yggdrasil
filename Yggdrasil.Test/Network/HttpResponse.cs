// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using Xunit;
using Yggdrasil.Network.WebSocket;

namespace Yggdrasil.Test.Network
{
	public class HttpResponseTests
	{
		[Fact]
		public void OkContent()
		{
			var expected =
				"HTTP/1.1 200 OK\r\n" +
				"Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
				"Server: Apache/2.2.14 (Win32)\r\n" +
				"Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT\r\n" +
				"Content-Type: text/html\r\n" +
				"Connection: Closed\r\n" +
				"Content-Length: 56\r\n" +
				"\r\n" +
				"<html>\r\n" +
				"<body>\r\n" +
				"<h1>Hello, World!</h1>\r\n" +
				"</body>\r\n" +
				"</html>\r\n" +
				"\r\n" +
				"";

			var response = new HttpResponse();
			response.Headers["Date"] = DateTime.Parse("Mon, 27 Jul 2009 12:28:53 GMT").ToUniversalTime().ToString("r");
			response.Headers["Server"] = "Apache/2.2.14 (Win32)";
			response.Headers["Last-Modified"] = DateTime.Parse("Wed, 22 Jul 2009 19:15:56 GMT").ToUniversalTime().ToString("r");
			response.Headers["Content-Type"] = "text/html";
			response.Headers["Connection"] = "Closed";
			response.Content =
				"<html>\r\n" +
				"<body>\r\n" +
				"<h1>Hello, World!</h1>\r\n" +
				"</body>\r\n" +
				"</html>";

			var responseString = response.ToString();
			Assert.Equal(expected, responseString);
		}

		[Fact]
		public void OkNoContent()
		{
			var expected =
				"HTTP/1.1 200 OK\r\n" +
				"Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
				"Server: Apache/2.2.14 (Win32)\r\n" +
				"Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT\r\n" +
				"Content-Type: text/html\r\n" +
				"Connection: Closed\r\n" +
				"\r\n" +
				"";

			var response = new HttpResponse();
			response.Headers["Date"] = DateTime.Parse("Mon, 27 Jul 2009 12:28:53 GMT").ToUniversalTime().ToString("r");
			response.Headers["Server"] = "Apache/2.2.14 (Win32)";
			response.Headers["Last-Modified"] = DateTime.Parse("Wed, 22 Jul 2009 19:15:56 GMT").ToUniversalTime().ToString("r");
			response.Headers["Content-Type"] = "text/html";
			response.Headers["Connection"] = "Closed";

			var responseString = response.ToString();
			Assert.Equal(expected, responseString);
		}

		[Fact]
		public void WebSocketUpgrade()
		{
			var expected =
				"HTTP/1.1 101 Switching Protocols\r\n" +
				"Connection: Upgrade\r\n" +
				"Upgrade: websocket\r\n" +
				"Sec-WebSocket-Accept: test=\r\n" +
				"\r\n" +
				"";

			var response = new HttpResponse(101, "Switching Protocols");
			response.Headers["Connection"] = "Upgrade";
			response.Headers["Upgrade"] = "websocket";
			response.Headers["Sec-WebSocket-Accept"] = "test=";

			var responseString = response.ToString();
			Assert.Equal(expected, responseString);
		}
	}
}
