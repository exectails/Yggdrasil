﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Yggdrasil.IO;

namespace Yggdrasil.Scripting
{
	/// <summary>
	/// Loads and initializes .NET scripts.
	/// </summary>
	public class ScriptLoader
	{
		private readonly List<IPrecompiler> _precompilers = new List<IPrecompiler>();
		private readonly HashSet<string> _filePaths = new HashSet<string>();
		private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
		private readonly List<IDisposable> _disposable = new List<IDisposable>();
		private readonly LinkedList<string> _tempFiles = new LinkedList<string>();
		private readonly string _cacheFilePath;

		private readonly string[] _defaultReferences = new string[]
		{
			//"System.dll",
			//"System.Core.dll",
			//"System.Data.dll",
			//"Microsoft.CSharp.dll",
			//"System.Xml.dll",
			//"System.Xml.Linq.dll",
		};

		private List<PortableExecutableReference> References = new List<PortableExecutableReference>();

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
		/// Creates new instance
		/// </summary>
		/// <param name="provider"></param>
		public ScriptLoader()
			: this(null)
		{
		}

		/// <summary>
		/// Creates new instance, caching the result in the given file.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="cacheFilePath"></param>
		public ScriptLoader(string cacheFilePath)
		{
			_cacheFilePath = cacheFilePath;
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
			// Clear cache if the list file is newer than it
			if (_cacheFilePath != null && File.Exists(_cacheFilePath))
			{
				var lastListChange = File.GetLastWriteTime(filePath);
				var lastCacheChange = File.GetLastWriteTime(_cacheFilePath);

				if (lastListChange >= lastCacheChange)
					File.Delete(_cacheFilePath);
			}

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
		/// Adds an assembly to References for use when compiling scripts
		/// </summary>
		/// <param name="assemblyDll"></param>
		/// <returns></returns>
		private bool AddAssembly(string assemblyDll)
		{
			if (string.IsNullOrEmpty(assemblyDll)) return false;

			var file = Path.GetFullPath(assemblyDll);

			if (!File.Exists(file))
			{
				// check framework or dedicated runtime app folder
				var path = Path.GetDirectoryName(typeof(object).Assembly.Location);
				file = Path.Combine(path, assemblyDll);
				if (!File.Exists(file))
					return false;
			}

			if (References.Any(r => r.FilePath == file)) return true;

			try
			{
				var reference = MetadataReference.CreateFromFile(file);
				References.Add(reference);
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Helper method to add multiple assemblies at once.
		/// </summary>
		/// <param name="assemblies"></param>
		private void AddAssemblies(params string[] assemblies)
		{
			foreach (var file in assemblies)
				this.AddAssembly(file);
		}

		/// <summary>
		/// Helper method to add default .net core references for compiling scripts.
		/// </summary>
		private void AddNetCoreDefaultReferences()
		{
			var rtPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

			this.AddAssemblies(
				Path.Combine(rtPath,"System.Private.CoreLib.dll"),
				Path.Combine(rtPath, "System.Runtime.dll"),
				Path.Combine(rtPath, "System.Console.dll"),
				Path.Combine(rtPath, "netstandard.dll"),

				Path.Combine(rtPath, "System.Text.RegularExpressions.dll"),
				Path.Combine(rtPath, "System.Linq.dll"),
				Path.Combine(rtPath, "System.Linq.Expressions.dll"),

				Path.Combine(rtPath, "System.IO.dll"),
				Path.Combine(rtPath, "System.Net.Primitives.dll"),
				Path.Combine(rtPath, "System.Net.Http.dll"),
				Path.Combine(rtPath, "System.Private.Uri.dll"),
				Path.Combine(rtPath, "System.Reflection.dll"),
				Path.Combine(rtPath, "System.ComponentModel.Primitives.dll"),
				Path.Combine(rtPath, "System.Globalization.dll"),
				Path.Combine(rtPath, "System.Collections.Concurrent.dll"),
				Path.Combine(rtPath, "System.Collections.NonGeneric.dll"),
				Path.Combine(rtPath, "Microsoft.CSharp.dll")
			);
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
				var filePaths = scriptFilesList.Select(a => a.Replace('/', Path.DirectorySeparatorChar)).ToArray();
				var latestFileChange = filePaths.Max(a => File.GetLastWriteTime(a));

				// Return cached assembly if it exists and is not older than
				// the latest changed file in the list.
				if (_cacheFilePath != null && File.Exists(_cacheFilePath))
				{
					var lastCacheChange = File.GetLastWriteTime(_cacheFilePath);
					if (latestFileChange < lastCacheChange)
						return Assembly.LoadFile(Path.GetFullPath(_cacheFilePath));
				}

				// Pre-compile files
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

							var changed = precompilers[j].Precompile(filePath, ref content);
							if (changed)
							{
								File.WriteAllText(tmpPath, content);
								filePaths[i] = tmpPath;

								_tempFiles.AddLast(tmpPath);
								mapFilePaths[tmpPath] = filePath;
							}
						}
					}
				}

				var tmpAssemblyPath = Path.GetTempFileName();
				_tempFiles.AddLast(tmpAssemblyPath);

				this.AddNetCoreDefaultReferences();

				// Add default references
				foreach (var reference in _defaultReferences)
					this.AddAssembly(reference);

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
					this.AddAssembly(location);
				}
				var trees = new List<SyntaxTree>();
				foreach (var sourceFile in filePaths)
				{
					trees.Add(SyntaxFactory.ParseSyntaxTree(File.ReadAllText(sourceFile)));
				}
				var compilation = CSharpCompilation.Create("Scripts.cs")
					.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
						optimizationLevel: OptimizationLevel.Release))
					.WithReferences(References)
					.AddSyntaxTrees(trees);

				Assembly compiledAssembly = Assembly.Load(EmitToArray(compilation));

				return compiledAssembly;
			}
			finally
			{
				this.ClearTempFiles();
			}
		}

		/// <summary>
		/// Takes a compilation and emits the resulting executable scripts as a byte array.
		/// </summary>
		/// <param name="compilation"></param>
		/// <returns></returns>
		/// <exception cref="CompilerErrorException"></exception>
		private static byte[] EmitToArray(Compilation compilation)
		{
			using (var stream = new MemoryStream())
			{
				// emit result into a stream
				var emitResult = compilation.Emit(stream);

				if (!emitResult.Success)
				{
					throw new CompilerErrorException(emitResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(y => y.ToString()).ToList());
				}

				// get the byte array from a stream
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Clears temp files created during compilation.
		/// </summary>
		private void ClearTempFiles()
		{
			foreach (var tempFile in _tempFiles)
				File.Delete(tempFile);

			_tempFiles.Clear();
		}

		/// <summary>
		/// Initializes all types in given assembly.
		/// </summary>
		/// <param name="assembly"></param>
		private void InitAssembly(Assembly assembly)
		{
			var types = assembly.GetTypes().Where(a => a.GetInterfaces().Contains(typeof(IScript)) && !a.IsAbstract);

			types = types.OrderByDescending(a =>
			{
				var attributes = a.GetCustomAttributes(typeof(PriorityAttribute), false);
				if (attributes.Length == 0)
					return 0;

				return ((PriorityAttribute)attributes[0]).Value;
			});

			var scripts = new List<IScript>();
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
					scripts.Add(script);

					this.LoadedCount++;
				}
				catch (Exception ex)
				{
					this.FailCount++;
					this.LoadingExceptions.Add(new ScriptLoadingException(typeName, ex));
					continue;
				}
				finally
				{
					this.TotalCount++;
				}
			}

			foreach (var script in scripts)
			{
				try
				{
					if (script is IPostInitScript postInitScript)
						postInitScript.OnPostInit();
				}
				catch (Exception ex)
				{
					this.LoadingExceptions.Add(new ScriptLoadingException("Failed to post init script '" + script.GetType().Name + "': " + ex));
					continue;
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
