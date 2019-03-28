using System.IO;

namespace Yggdrasil.Data
{
	/// <summary>
	/// Extentions for Stream.
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		/// Copies the contents of the first stream into the second one.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		public static void CopyTo(this Stream input, Stream output)
		{
			var buffer = new byte[16 * 1024];
			int bytesRead;

			while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
				output.Write(buffer, 0, bytesRead);
		}
	}
}
