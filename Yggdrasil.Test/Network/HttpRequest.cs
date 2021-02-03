using System;
using Xunit;
using Yggdrasil.Network.WebSocket;

namespace Yggdrasil.Test.Network
{
	public class HttpRequestTests
	{
		[Fact]
		public void NoContent()
		{
			var requestString = @"GET /hello.htm HTTP/1.1
User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)
Host: www.somesite.com
Accept-Language: en-us
Accept-Encoding: gzip, deflate
Connection: Keep-Alive
";

			var request = new HttpRequest(requestString);

			Assert.Equal("GET", request.Method);
			Assert.Equal("/hello.htm", request.RequestUri);
			Assert.Equal("HTTP/1.1", request.HttpVersion);
			Assert.Equal(5, request.Headers.Count);
			Assert.Equal("Mozilla/4.0 (compatible; MSIE5.01; Windows NT)", request.Headers["User-Agent"]);
			Assert.Equal("www.somesite.com", request.Headers["Host"]);
			Assert.Equal("en-us", request.Headers["Accept-Language"]);
			Assert.Equal("gzip, deflate", request.Headers["Accept-Encoding"]);
			Assert.Equal("Keep-Alive", request.Headers["Connection"]);
			Assert.Equal("", request.Content);
		}

		[Fact]
		public void SingleLineContent()
		{
			var requestString = @"POST /cgi-bin/process.cgi HTTP/1.1
User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)
Host: www.somesite.com
Content-Type: application/x-www-form-urlencoded
Content-Length: 49
Accept-Language: en-us
Accept-Encoding: gzip, deflate
Connection: Keep-Alive

licenseID=string&content=string&/paramsXML=string
";

			var request = new HttpRequest(requestString);

			Assert.Equal("POST", request.Method);
			Assert.Equal("/cgi-bin/process.cgi", request.RequestUri);
			Assert.Equal("HTTP/1.1", request.HttpVersion);
			Assert.Equal(7, request.Headers.Count);
			Assert.Equal("Mozilla/4.0 (compatible; MSIE5.01; Windows NT)", request.Headers["User-Agent"]);
			Assert.Equal("www.somesite.com", request.Headers["Host"]);
			Assert.Equal("application/x-www-form-urlencoded", request.Headers["Content-Type"]);
			Assert.Equal("49", request.Headers["Content-Length"]);
			Assert.Equal("en-us", request.Headers["Accept-Language"]);
			Assert.Equal("gzip, deflate", request.Headers["Accept-Encoding"]);
			Assert.Equal("Keep-Alive", request.Headers["Connection"]);
			Assert.Equal("licenseID=string&content=string&/paramsXML=string", request.Content);
		}

		[Fact]
		public void MultiLineContent()
		{
			var requestString = @"POST /cgi-bin/process.cgi HTTP/1.1
User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)
Host: www.somesite.com
Content-Type: text/xml; charset=utf-8
Content-Length: " + (96 + Environment.NewLine.Length).ToString() + @"
Accept-Language: en-us
Accept-Encoding: gzip, deflate
Connection: Keep-Alive

<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine + @"<string xmlns=""http://somesite.com/"">string</string>
";

			var request = new HttpRequest(requestString);

			Assert.Equal("POST", request.Method);
			Assert.Equal("/cgi-bin/process.cgi", request.RequestUri);
			Assert.Equal("HTTP/1.1", request.HttpVersion);
			Assert.Equal(7, request.Headers.Count);
			Assert.Equal("Mozilla/4.0 (compatible; MSIE5.01; Windows NT)", request.Headers["User-Agent"]);
			Assert.Equal("www.somesite.com", request.Headers["Host"]);
			Assert.Equal("text/xml; charset=utf-8", request.Headers["Content-Type"]);
			Assert.Equal((96 + Environment.NewLine.Length).ToString(), request.Headers["Content-Length"]);
			Assert.Equal("en-us", request.Headers["Accept-Language"]);
			Assert.Equal("gzip, deflate", request.Headers["Accept-Encoding"]);
			Assert.Equal("Keep-Alive", request.Headers["Connection"]);
			Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + "<string xmlns=\"http://somesite.com/\">string</string>", request.Content);
		}

		[Fact]
		public void WebSocketUpgrade()
		{
			var requestString = @"GET / HTTP/1.1
Host: 127.0.0.1:8080
Connection: Upgrade
Pragma: no-cache
Cache-Control: no-cache
Upgrade: websocket
Origin: http://127.0.0.1:8080
Sec-WebSocket-Version: 13
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36
Accept-Encoding: gzip, deflate, br
Accept-Language: en-US,en;q=0.8,de-DE;q=0.6,de;q=0.4
Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits
Sec-WebSocket-Key: vLjSDjZHPYlfqo9WoJGTVw==
";

			var request = new HttpRequest(requestString);

			Assert.Equal("GET", request.Method);
			Assert.Equal("/", request.RequestUri);
			Assert.Equal("HTTP/1.1", request.HttpVersion);
			Assert.Equal(12, request.Headers.Count);
			Assert.Equal("127.0.0.1:8080", request.Headers["Host"]);
			Assert.Equal("Upgrade", request.Headers["Connection"]);
			Assert.Equal("no-cache", request.Headers["Pragma"]);
			Assert.Equal("websocket", request.Headers["Upgrade"]);
			Assert.Equal("http://127.0.0.1:8080", request.Headers["Origin"]);
			Assert.Equal("13", request.Headers["Sec-WebSocket-Version"]);
			Assert.Equal("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36", request.Headers["User-Agent"]);
			Assert.Equal("gzip, deflate, br", request.Headers["Accept-Encoding"]);
			Assert.Equal("en-US,en;q=0.8,de-DE;q=0.6,de;q=0.4", request.Headers["Accept-Language"]);
			Assert.Equal("permessage-deflate; client_max_window_bits", request.Headers["Sec-WebSocket-Extensions"]);
			Assert.Equal("vLjSDjZHPYlfqo9WoJGTVw==", request.Headers["Sec-WebSocket-Key"]);
			Assert.Equal("", request.Content);
		}
	}
}
