// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Yggdrasil.Network.WebSocket
{
	public class HttpResponse
	{
		/// <summary>
		/// Get or sets the HTTP version.
		/// </summary>
		public string HttpVersion { get; set; } = "HTTP/1.1";

		/// <summary>
		/// Gets or set the status code.
		/// </summary>
		public int StatusCode { get; set; } = 200;

		/// <summary>
		/// Gets or set the status text.
		/// </summary>
		public string StatusMessage { get; set; } = "OK";

		/// <summary>
		/// The headers sent with the request.
		/// </summary>
		public OrderedDictionary Headers { get; } = new OrderedDictionary();

		/// <summary>
		/// Gets or sets the response's content. The Content-Length header
		/// is automatically included if this is set.
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// Creates new instance for an "OK" response.
		/// </summary>
		public HttpResponse()
		{
		}

		/// <summary>
		/// Creates new instance with the given status.
		/// </summary>
		/// <param name="statusCode"></param>
		/// <param name="message"></param>
		public HttpResponse(int statusCode, string message)
		{
			this.StatusCode = statusCode;
			this.StatusMessage = message;
		}

		/// <summary>
		/// Returns HTTP response as string that can be sent to the client.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			byte[] contentBytes = null;
			string contentUtf8 = null;

			if (this.Content != null && this.Content.Length != 0)
			{
				contentBytes = Encoding.UTF8.GetBytes(this.Content);
				contentUtf8 = Encoding.UTF8.GetString(contentBytes);

				this.Headers["Content-Length"] = contentBytes.Length.ToString();
			}

			var response = new StringBuilder();

			response.AppendFormat("{0} {1} {2}\r\n", this.HttpVersion, this.StatusCode, this.StatusMessage);
			foreach (DictionaryEntry header in this.Headers)
				response.AppendFormat("{0}: {1}\r\n", header.Key, header.Value);

			if (contentBytes != null)
			{
				response.Append("\r\n");
				response.Append(contentUtf8);
				response.Append("\r\n");
			}

			response.Append("\r\n");

			return response.ToString();
		}

		/// <summary>
		/// Returns response as a byte array that can be sent to the client.
		/// </summary>
		/// <returns></returns>
		public byte[] ToBytes()
		{
			var response = this.ToString();
			var responseBytes = Encoding.UTF8.GetBytes(response);

			return responseBytes;
		}
	}
}
