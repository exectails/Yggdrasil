namespace Yggdrasil.Scripting
{
	/// <summary>
	/// Represents a compiler error, giving information about where and
	/// why the compilation failed.
	/// </summary>
	public readonly struct CompilerError
	{
		/// <summary>
		/// The name of the file where the error occurred in.
		/// </summary>
		/// <remarks>
		/// This may be null or empty if the error is not related to a
		/// specific file.
		/// </remarks>
		public readonly string FileName;

		/// <summary>
		/// Returns the number of the line where the error occurred,
		/// starting at 1.
		/// </summary>
		public readonly int Line;

		/// <summary>
		/// Returns the number of the column where the error occurred,
		/// starting at 1.
		/// </summary>
		public readonly int Column;

		/// <summary>
		/// Returns the error message.
		/// </summary>
		public readonly string ErrorText;

		/// <summary>
		/// Returns true if the error was a warning.
		/// </summary>
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
