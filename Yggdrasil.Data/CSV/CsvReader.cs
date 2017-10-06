// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System.Collections.Generic;
using System.IO;

namespace Yggdrasil.Data.CSV
{
	/// <summary>
	/// Reader for CSV data.
	/// </summary>
	public class CsvReader
	{
		/// <summary>
		/// The stream this instance reads from.
		/// </summary>
		private Stream Stream { get; }

		/// <summary>
		/// The path to the file that is being read.
		/// </summary>
		public string Path { get; }

		/// <summary>
		/// The character that seperates two values.
		/// </summary>
		public char Seperator { get; }

		/// <summary>
		/// Creates a new instance, reading data from stream,
		/// using path for reference.
		/// </summary>
		/// <param name="stream">Stream to read data from.</param>
		/// <param name="path">Path of the file the data is coming from, for reference.</param>
		/// <param name="seperator">Character seperating values.</param>
		public CsvReader(Stream stream, string path, char seperator)
		{
			this.Stream = stream;
			this.Path = path;
			this.Seperator = seperator;
		}

		/// <summary>
		/// Reads the next line from the file and returns its data.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<CsvEntry> Next()
		{
			using (var reader = new StreamReader(this.Stream))
			{
				for (var i = 0; !reader.EndOfStream; ++i)
				{
					var values = this.GetValues(reader.ReadLine());
					if (values.Count < 1)
						continue;

					var line = i + 1;

					yield return new CsvEntry(values, line);
				}
			}

			yield break;
		}

		/// <summary>
		/// Reads raw data from given line.
		/// </summary>
		/// <param name="csvLine"></param>
		/// <returns></returns>
		protected List<string> GetValues(string csvLine)
		{
			var values = new List<string>();

			csvLine = csvLine.Trim();

			// Check for empty and commented lines.
			if (csvLine != string.Empty && !csvLine.StartsWith("//"))
			{
				int ptr = 0, braces = 0;
				bool inString = false, comment = false;
				for (var i = 0; i < csvLine.Length; ++i)
				{
					// End of line?
					var eol = (i == csvLine.Length - 1);

					// Quotes
					if (csvLine[i] == '"' && braces == 0)
						inString = !inString;

					// Braces
					if (!inString)
					{
						if (csvLine[i] == '{')
							braces++;
						else if (csvLine[i] == '}')
							braces--;
					}

					// Comments
					if (!inString && csvLine[i] == '/' && csvLine[i + 1] == '/')
						comment = true;

					if ((csvLine[i] == this.Seperator && braces == 0 && !inString) || eol || comment)
					{
						// Inc by one to get the last char
						if (eol) i++;

						// Get value
						var v = csvLine.Substring(ptr, i - ptr).Trim(' ', '\t', '"');

						// Trim surrounding braces
						if (v.Length >= 2 && v[0] == '{' && v[v.Length - 1] == '}')
							v = v.Substring(1, v.Length - 2);

						values.Add(v);

						// Skip over seperator
						ptr = i + 1;

						// Stop at comments
						if (comment)
							break;
					}
				}
			}

			return values;
		}
	}
}
