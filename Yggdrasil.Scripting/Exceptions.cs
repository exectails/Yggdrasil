using System;
using System.Collections.Generic;

namespace Yggdrasil.Scripting
{
	/// <summary>
	/// An exception that might occur while compiling scripts.
	/// </summary>
	public class CompilerErrorException : Exception
	{
		/// <summary>
		/// The errors that occurred.
		/// </summary>
		public CompilerErrorCollection Errors { get; protected set; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="errors"></param>
		public CompilerErrorException(CompilerErrorCollection errors)
		{
			this.Errors = errors;
		}
	}

	/// <summary>
	/// An exception that might occur while loading compiled scripts.
	/// </summary>
	public class ScriptLoadingException : Exception
	{
		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="message"></param>
		public ScriptLoadingException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="innerException"></param>
		public ScriptLoadingException(string typeName, Exception innerException)
			: base($"Failed to initialize '{typeName}'.", innerException)
		{
		}
	}

	/// <summary>
	/// A collection of compiler errors.
	/// </summary>
	public class CompilerErrorCollection : List<CompilerError>
	{
		/// <summary>
		/// Creates new, empty instance.
		/// </summary>
		public CompilerErrorCollection()
		{
		}

		/// <summary>
		/// Creates new instance and adds the given errors.
		/// </summary>
		/// <param name="errors"></param>
		public CompilerErrorCollection(IEnumerable<CompilerError> errors)
			: base(errors)
		{
		}
	}
}
