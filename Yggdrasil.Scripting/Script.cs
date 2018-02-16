// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

namespace Yggdrasil.Scripting
{
	/// <summary>
	/// Implemented by classes in scripts that can be initialized
	/// on load.
	/// </summary>
	public interface IScript
	{
		/// <summary>
		/// Called after the script is loaded.
		/// </summary>
		/// <returns></returns>
		bool Init();
	}
}
