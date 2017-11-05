// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

namespace Yggdrasil.Scripting
{
	public interface IScript
	{
		/// <summary>
		/// Called after the script is loaded.
		/// </summary>
		/// <returns></returns>
		bool Init();
	}
}
