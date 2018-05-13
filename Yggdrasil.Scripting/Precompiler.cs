namespace Yggdrasil.Scripting
{
	/// <summary>
	/// A script pre-compiler.
	/// </summary>
	public interface IPrecompiler
	{
		/// <summary>
		/// Precompiles given script and returns the modified version.
		/// </summary>
		/// <param name="filePath">The path to the original script file.</param>
		/// <param name="script">The contents of the script file.</param>
		/// <returns></returns>
		string Precompile(string filePath, string script);
	}
}
