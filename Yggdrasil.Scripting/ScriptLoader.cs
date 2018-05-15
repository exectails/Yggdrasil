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
		private LinkedList<string> _tempFiles = new LinkedList<string>();

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
		/// Returns the amount of script classes that were successfully
		/// loaded and initialized.
		/// </summary>
		public int LoadedCount { get; private set; }

		/// <summary>
		/// Returns the amount of script classes that failed to initialize.
		/// </summary>
		public int FailCount { get; private set; }

		/// <summary>
		/// Returns the amount of script classes that were to be loaded.
		/// </summary>
		public int TotalCount { get; private set; }

		/// <summary>
		/// Returns the amount of files loaded.
		/// </summary>
		public int FileCount => _filePaths.Count;

		/// <summary>
		/// A list of exceptions thrown while initializing the compiled
		/// scripts.
		/// </summary>
		public List<ScriptLoadingException> LoadingExceptions { get; } = new List<ScriptLoadingException>();

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
			var cwd = Directory.GetCurrentDirectory().Replace("\\", "/") + "/";

			using (var fr = new FileReader(scriptListFile))
			{
				foreach (var line in fr)
				{
					var scriptPath = line.Value.Replace("\\", "/");
					var listDirPath = Path.GetDirectoryName(Path.GetFullPath(line.File)).Replace("\\", "/").Replace(cwd, "") + "/";

					var paths = new List<string>();
					if (!scriptPath.EndsWith("/*"))
					{
						paths.Add(scriptPath);
					}
					else
					{
						var recursive = scriptPath.EndsWith("/**/*");
						var directoryPath = Path.Combine(listDirPath, scriptPath).Replace("\\", "/").Replace(cwd, "").TrimEnd('/', '*');

						if (!Directory.Exists(directoryPath))
							continue;

						foreach (var eFilePath in Directory.EnumerateFiles(directoryPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
						{
							// Ignore "hidden" files
							if (Path.GetFileName(eFilePath).StartsWith("."))
								continue;

							paths.Add(eFilePath.Replace("\\", "/").Replace(listDirPath, ""));
						}
					}

					for (var i = 0; i < paths.Count; ++i)
					{
						var path = paths[i];

						if (path.StartsWith("/"))
						{
							path = path.Substring(1);
						}
						else
						{
							var found = false;

							foreach (var priorityPath in priorityFolders)
							{
								var combinedPath = Path.Combine(priorityPath, path).Replace("\\", "/").Replace(cwd, "");
								if (File.Exists(combinedPath))
								{
									path = combinedPath;
									found = true;
									break;
								}
							}

							if (!found)
								path = Path.Combine(listDirPath, path).Replace("\\", "/").Replace(cwd, "");
						}

						if (!result.Contains(path))
							result.Add(path);
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
			this.Unload();

			// Mono's runtime compiler throws an exception if no files are
			// specified.
			if (!_filePaths.Any())
				return;

			var assembly = this.Compile(_filePaths.Where(a => a.EndsWith(".cs")));
			this.InitAssembly(assembly);
		}

		/// <summary>
		/// Unloads all scripts, disposing them and clearing the internal
		/// script lists. Does not clear script file list, reloading after
		/// this loads the previously loaded scripts.
		/// </summary>
		public void Unload()
		{
			foreach (var disposable in _disposable)
				disposable.Dispose();

			_disposable.Clear();
			_types.Clear();

			this.LoadedCount = 0;
			this.FailCount = 0;
			this.TotalCount = 0;
			this.LoadingExceptions.Clear();
		}

		/// <summary>
		/// Unloads all scripts and clears script file list. Unlike Unload,
		/// reloading after this doesn't actually load anything.
		/// </summary>
		public void Clear()
		{
			this.Unload();
			_filePaths.Clear();
		}

		/// <summary>
		/// Creates new assembly, compiling the given files.
		/// </summary>
		/// <param name="scriptFilesList"></param>
		/// <returns></returns>
		private Assembly Compile(IEnumerable<string> scriptFilesList)
		{
			try
			{
				var filePaths = scriptFilesList.Select(a => a.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar)).ToArray();
				var mapFilePaths = filePaths.ToDictionary(a => a);
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

							content = precompilers[j].Precompile(filePath, content);

							File.WriteAllText(tmpPath, content);
							filePaths[i] = tmpPath;

							_tempFiles.AddLast(tmpPath);
							mapFilePaths[tmpPath] = filePath;
						}
					}
				}

				// Prepare parameters
				var parameters = new CompilerParameters();
				parameters.GenerateExecutable = false;
				parameters.GenerateInMemory = true;
				parameters.WarningLevel = 0;
				parameters.IncludeDebugInformation = true;

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
				{
					foreach (CompilerError error in errors)
						error.FileName = mapFilePaths[error.FileName];

					throw new CompilerErrorException(errors);
				}

				return result.CompiledAssembly;
			}
			finally
			{
				this.ClearTempFiles();
			}
		}

		/// <summary>
		/// Clears temp files created during compilation.
		/// </summary>
		private void ClearTempFiles()
		{
			foreach (var tempFile in _tempFiles)
				File.Delete(tempFile);
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
					throw new ScriptLoadingException($"Script classes must have unique names, duplicate '{typeName}' found.");

				try
				{
					var script = Activator.CreateInstance(type) as IScript;
					if (!script.Init())
					{
						if (type.GetInterfaces().Contains(typeof(IDisposable)))
						{
							try { (script as IDisposable).Dispose(); }
							catch { throw new ScriptLoadingException($"Failed to initialize and dispose '{typeName}'."); }
						}

						throw new ScriptLoadingException($"Failed to initialize '{typeName}'.");
					}

					if (type.GetInterfaces().Contains(typeof(IDisposable)))
						_disposable.Add(script as IDisposable);

					_types.Add(typeName, type);

					this.LoadedCount++;
				}
				catch (Exception ex)
				{
					this.FailCount++;
					this.LoadingExceptions.Add(new ScriptLoadingException(typeName, ex));
				}
				finally
				{
					this.TotalCount++;
				}
			}
		}

		/// <summary>
		/// Returns loaded script type by name.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public Type GetScriptType(string typeName)
		{
			_types.TryGetValue(typeName, out var result);
			return result;
		}
	}
}
