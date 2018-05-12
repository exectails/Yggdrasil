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
		/// <param name="script"></param>
		/// <returns></returns>
		string Precompile(string script);
	}
}
