// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.CodeDom.Compiler;

namespace Yggdrasil.Scripting
{
	public class CompilerErrorException : Exception
	{
		public CompilerErrorCollection Errors { get; protected set; }

		public CompilerErrorException(CompilerErrorCollection errors)
		{
			this.Errors = errors;
		}
	}

	public class ScriptLoadingException : Exception
	{
		public ScriptLoadingException(string message)
			: base(message)
		{
		}

		public ScriptLoadingException(string format, params object[] args)
			: base(string.Format(format, args))
		{
		}
	}
}
