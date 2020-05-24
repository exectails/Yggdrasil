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

	/// <summary>
	/// Implemented by scripts that need to execute code after all
	/// scripts were initialized.
	/// </summary>
	public interface IPostInitScript
	{
		/// <summary>
		/// Called after all scripts were initialized.
		/// </summary>
		void OnPostInit();
	}
}
