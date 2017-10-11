// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yggdrasil.Data
{
	public class ApplicationData
	{
		private Dictionary<Type, IDatabase> _databases = new Dictionary<Type, IDatabase>();

		/// <summary>
		/// Creates new instance, loading all databases found in the
		/// executing assembly.
		/// </summary>
		public ApplicationData()
		{
			this.AutoLoad();
		}

		/// <summary>
		/// Loads all databases found in the executing assembly.
		/// </summary>
		private void AutoLoad()
		{
			var asm = Assembly.GetExecutingAssembly();
			var types = asm.GetTypes();

			foreach (var type in types.Where(a => !a.IsAbstract && !a.IsInterface && a.GetInterfaces().Contains(typeof(IDatabase))))
			{
				var instance = (IDatabase)Activator.CreateInstance(type);
				_databases[type] = instance;
			}
		}

		/// <summary>
		/// Returns database with given type.
		/// </summary>
		/// <typeparam name="TDatabase"></typeparam>
		/// <returns></returns>
		public TDatabase Get<TDatabase>() where TDatabase : IDatabase
		{
			_databases.TryGetValue(typeof(TDatabase), out var result);
			return (TDatabase)result;
		}
	}
}
