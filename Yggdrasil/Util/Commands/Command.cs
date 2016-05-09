// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Yggdrasil.Util.Commands
{
	/// <summary>
	/// Generalized command holder
	/// </summary>
	/// <typeparam name="TFunc"></typeparam>
	public abstract class Command<TFunc> where TFunc : class
	{
		public string Name { get; protected set; }
		public string Usage { get; protected set; }
		public string Description { get; protected set; }
		public TFunc Func { get; protected set; }

		protected Command(string name, string usage, string description, TFunc func)
		{
			if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(typeof(TFunc).Name + " is not a delegate type");

			this.Name = name;
			this.Usage = usage;
			this.Description = description;
			this.Func = func;
		}
	}
}
