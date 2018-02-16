// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.CodeDom.Compiler;

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
		public ScriptLoadingException(string format, params object[] args)
			: base(string.Format(format, args))
		{
		}
	}
}
