namespace Yggdrasil.Scripting
{
	/// <summary>
	/// Represents a compiler error, giving information about where and
	/// why the compilation failed.
	/// </summary>
	public readonly struct CompilerError
	{
		public readonly string FileName;
		public readonly int Line;
		public readonly int Column;
		public readonly string ErrorText;
		public readonly bool IsWarning;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="line"></param>
		/// <param name="column"></param>
		/// <param name="errorText"></param>
		/// <param name="isWarning"></param>
		public CompilerError(string fileName, int line, int column, string errorText, bool isWarning)
		{
			this.FileName = fileName;
			this.Line = line;
			this.Column = column;
			this.ErrorText = errorText;
			this.IsWarning = isWarning;
		}

		/// <summary>
		/// Returns a string representation of the error.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(this.FileName))
				return string.Format("({0}, {1}): {2}", this.Line, this.Column, this.ErrorText);
			else
				return string.Format("{0}({1}, {2}): {3}", this.FileName, this.Line, this.Column, this.ErrorText);
		}
	}
}
