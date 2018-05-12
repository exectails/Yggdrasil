// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Yggdrasil.IO;

namespace Yggdrasil.Scripting
{
	/// <summary>
	/// Loads and initializes .NET scripts.
	/// </summary>
	public class ScriptLoader
	{
		private CodeDomProvider _compiler;
		private List<IPrecompiler> _precompilers = new List<IPrecompiler>();
		private HashSet<string> _filePaths = new HashSet<string>();
		private Dictionary<string, Type> _types = new Dictionary<string, Type>();
		private List<IDisposable> _disposable = new List<IDisposable>();

		private readonly string[] _defaultReferences = new string[]
		{
			//"System.dll",
			//"System.Core.dll",
			//"System.Data.dll",
			//"Microsoft.CSharp.dll",
			//"System.Xml.dll",
			//"System.Xml.Linq.dll",
		};

		/// <summary>
		/// Creates new instance, using the given CodeDomProvider to compile
		/// scripts.
		/// </summary>
		/// <param name="provider"></param>
		public ScriptLoader(CodeDomProvider provider)
		{
			_compiler = provider;
		}

		/// <summary>
		/// Adds precompiler to be used on load.
		/// </summary>
		/// <param name="precompiler"></param>
		public void AddPrecompiler(IPrecompiler precompiler)
		{
			_precompilers.Add(precompiler);
		}

		/// <summary>
		/// Loads all files listed in the given file, using
		/// <see cref="FileReader"/>. If priority folders
		/// is set, the method tries to find them in those folders first,
		/// using the files from them first, in order, so files can be
		/// overriden from other folders.
		/// </summary>
		/// <example>
		/// LoadFromListFile("system/scripts/script_list.txt", "user/scripts", "system/scripts/");
		/// </example>
		/// <param name="filePath"></param>
		/// <param name="priorityFolders"></param>
		public void LoadFromListFile(string filePath, params string[] priorityFolders)
		{
			var scriptPaths = this.ReadScriptList(filePath, priorityFolders);
			this.LoadFromList(scriptPaths);
		}

		/// <summary>
		/// Returns list of files listed in given file. If priority folders
		/// is set, the method tries to find them in those folders first,
		/// using the files from them first, in order, so files can be
		/// overriden from other folders.
		/// </summary>
		/// <example>
		/// ReadScriptList("system/scripts/script_list.txt", "user/scripts", "system/scripts/");
		/// </example>
		/// <param name="scriptListFile"></param>
		/// <param name="priorityFolders"></param>
		/// <returns></returns>
		protected List<string> ReadScriptList(string scriptListFile, params string[] priorityFolders)
		{
			var result = new List<string>();

			using (var fr = new FileReader(scriptListFile))
			{
				foreach (var line in fr)
				{
					var scriptPath = line.Value;

					// Path relative to cwd
					if (scriptPath.StartsWith("/"))
					{
						scriptPath = Path.Combine(Directory.GetCurrentDirectory().Replace("\\", "/"), scriptPath.Replace("\\", "/").TrimStart('/')).Replace("\\", "/");
					}
					// Path relative to list file
					else
					{
						// Get path to the current list's directory
						var listPath = Path.GetFullPath(line.File);
						listPath = Path.GetDirectoryName(listPath);

						// Get full path to script
						scriptPath = Path.Combine(listPath, scriptPath).Replace("\\", "/");
					}

					var paths = new List<string>();
					if (!scriptPath.EndsWith("/*"))
					{
						paths.Add(scriptPath);
					}
					else
					{
						var recursive = scriptPath.EndsWith("/**/*");
						var directoryPath = scriptPath.TrimEnd('/', '*');

						if (Directory.Exists(directoryPath))
						{
							foreach (var file in Directory.EnumerateFiles(directoryPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
							{
								// Ignore "hidden" files
								if (Path.GetFileName(file).StartsWith("."))
									continue;

								paths.Add(file.Replace("\\", "/"));
							}
						}
						else
						{
							throw new ScriptLoadingException("Directory not found: {0}", directoryPath);
						}
					}

					foreach (var path in paths)
					{
						if (!result.Contains(path))
							result.Add(path);
					}
				}
			}

			// Fix paths to prioritize files
			if (priorityFolders != null && priorityFolders.Any())
			{
				for (var i = 0; i < result.Count; ++i)
				{
					var path = result[i];

					foreach (var folderPath in priorityFolders)
					{
						var combinedPath = Path.Combine(folderPath, path).Replace("\\", "/");
						if (File.Exists(combinedPath))
						{
							result[i] = combinedPath;
							break;
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Loads all files in the given list.
		/// </summary>
		/// <param name="scriptFilesList"></param>
		public void LoadFromList(IEnumerable<string> scriptFilesList)
		{
			_filePaths.UnionWith(scriptFilesList);
			this.Reload();
		}

		/// <summary>
		/// Reloads all previously loaded scripts.
		/// </summary>
		public void Reload()
		{
			this.DisposeAll();

			// Mono's runtime compiler throws an exception if no files are
			// specified.
			if (!_filePaths.Any())
				return;

			var assembly = this.Compile(_filePaths);
			this.InitAssembly(assembly);
		}

		/// <summary>
		/// Disposes all loaded scripts that are disposable.
		/// </summary>
		private void DisposeAll()
		{
			foreach (var disposable in _disposable)
				disposable.Dispose();
			_disposable.Clear();
		}

		/// <summary>
		/// Creates new assembly, compiling the given files.
		/// </summary>
		/// <param name="scriptFilesList"></param>
		/// <returns></returns>
		private Assembly Compile(IEnumerable<string> scriptFilesList)
		{
			var filePaths = scriptFilesList.Select(a => a.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar)).ToArray();
			var precompilers = _precompilers;

			if (_precompilers.Any())
			{
				for (var i = 0; i < filePaths.Length; ++i)
				{
					for (var j = 0; j < precompilers.Count; ++j)
					{
						var filePath = filePaths[i];

						var tmpPath = Path.GetTempFileName();
						var content = File.ReadAllText(filePath);

						content = precompilers[j].Precompile(content);

						File.WriteAllText(tmpPath, content);
						filePaths[i] = tmpPath;
					}
				}
			}

			// Prepare parameters
			var parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = true;
			parameters.WarningLevel = 0;

			// Add default references
			foreach (var reference in _defaultReferences)
				parameters.ReferencedAssemblies.Add(reference);

			// Get assemblies referenced by application
			var entryAssembly = Assembly.GetEntryAssembly();
			var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var toReference = new HashSet<string>();

			if (entryAssembly != null)
			{
				toReference.Add(entryAssembly.Location);

				foreach (var assemblyName in entryAssembly.GetReferencedAssemblies())
				{
					var assembly = Assembly.Load(assemblyName);
					toReference.Add(assembly.Location);
				}
			}

			foreach (var assembly in loadedAssemblies.Where(a => !a.IsDynamic))
			{
				toReference.Add(assembly.Location);
			}

			foreach (var location in toReference)
			{
				parameters.ReferencedAssemblies.Add(location);
			}

			// Compile, throw if compilation failed
			var result = _compiler.CompileAssemblyFromFile(parameters, filePaths);
			var errors = result.Errors;

			if (errors.Count != 0)
				throw new CompilerErrorException(errors);

			return result.CompiledAssembly;
		}

		/// <summary>
		/// Initializes all types in given assembly.
		/// </summary>
		/// <param name="assembly"></param>
		private void InitAssembly(Assembly assembly)
		{
			var types = assembly.GetTypes().Where(a => a.GetInterfaces().Contains(typeof(IScript)) && !a.IsAbstract);

			foreach (var type in types)
			{
				var typeName = type.Name;

				if (_types.ContainsKey(typeName))
					throw new ScriptLoadingException("Script classes must have unique names, duplicate '{0}' found.", typeName);

				try
				{
					var script = Activator.CreateInstance(type) as IScript;
					if (!script.Init())
					{
						if (type.GetInterfaces().Contains(typeof(IDisposable)))
						{
							try { (script as IDisposable).Dispose(); }
							catch { throw new ScriptLoadingException("Failed to initialize and dispose '{0}'.", typeName); }
						}

						throw new ScriptLoadingException("Failed to initialize '{0}'.", typeName);
					}

					if (type.GetInterfaces().Contains(typeof(IDisposable)))
						_disposable.Add(script as IDisposable);
				}
				catch (Exception ex)
				{
					throw new ScriptLoadingException("Failed to initialize '{0}'.{1}{2}", typeName, Environment.NewLine, ex);
				}
			}
		}
	}
}
