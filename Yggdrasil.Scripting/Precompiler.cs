namespace Yggdrasil.Scripting
{
	/// <summary>
	/// A script pre-compiler.
	/// </summary>
	public interface IPrecompiler
	{
		/// <summary>
		/// Precompiles given script, returns false if the script wasn't
		/// changed.
		/// </summary>
		/// <param name="filePath">The path to the original script file.</param>
		/// <param name="script">The contents of the script file.</param>
		/// <returns></returns>
		bool Precompile(string filePath, ref string script);
	}
}
