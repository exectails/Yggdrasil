using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yggdrasil.Network.WebSocket
{
	/// <summary>
	/// Represents a HTTP request.
	/// </summary>
	public class HttpRequest
	{
		/// <summary>
		/// Returns the request's method.
		/// </summary>
		public string Method { get; private set; }

		/// <summary>
		/// Returns the request URI.
		/// </summary>
		public string RequestUri { get; private set; }

		/// <summary>
		/// Return the HTTP version.
		/// </summary>
		public string HttpVersion { get; private set; }

		/// <summary>
		/// The headers sent with the request.
		/// </summary>
		public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

		/// <summary>
		/// The request's body.
		/// </summary>
		public string Content { get; private set; }

		/// <summary>
		/// Creates new instance and parses request string.
		/// </summary>
		/// <param name="requestString"></param>
		public HttpRequest(string requestString)
		{
			using (var sr = new StringReader(requestString))
			{
				this.ParseRequestLine(sr);
				this.ParseHeaders(sr);
				this.ParseContent(sr);
			}
		}

		/// <summary>
		/// Parses the first line of the request.
		/// </summary>
		/// <param name="sr"></param>
		private void ParseRequestLine(StringReader sr)
		{
			var requestLine = sr.ReadLine();

			var split = requestLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 3)
				throw new FormatException("Expected 3 parameters for request line.");

			this.Method = split[0].Trim();
			this.RequestUri = split[1].Trim();
			this.HttpVersion = split[2].Trim();

		}

		/// <summary>
		/// Parses the headers.
		/// </summary>
		/// <param name="sr"></param>
		private void ParseHeaders(StringReader sr)
		{
			string line;
			while ((line = sr.ReadLine()) != null)
			{
				// Stop at the empty line
				if (line.Trim() == "")
					break;

				var colonIndex = line.IndexOf(':');
				if (colonIndex == -1)
					throw new FormatException("Encountered header without colon.");

				var name = line.Substring(0, colonIndex).Trim();
				var value = line.Substring(colonIndex + 1).Trim();

				this.Headers[name] = value;
			}
		}

		/// <summary>
		/// Parses the content.
		/// </summary>
		/// <param name="sr"></param>
		private void ParseContent(StringReader sr)
		{
			var content = new StringBuilder();

			string line;
			while ((line = sr.ReadLine()) != null)
			{
				// Stop at the empty line
				if (line.Trim() == "")
					break;

				if (content.Length != 0)
					content.AppendLine();

				content.Append(line);
			}

			this.Content = content.ToString();
		}
	}
}
