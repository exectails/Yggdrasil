// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data
{
	/// <summary>
	/// A minor issue where a mandatory value was missing.
	/// </summary>
	public class MandatoryValueException : DatabaseWarningException
	{
		/// <summary>
		/// The name of the missing value.
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <param name="key">Missing value's name.</param>
		/// <param name="obj">Object that is missing the value.</param>
		public MandatoryValueException(string path, string key, JObject obj)
			: base(path, string.Format("Missing mandatory value '{0}', in \n{1}", key, obj))
		{
			this.Key = key;
		}
	}
}
