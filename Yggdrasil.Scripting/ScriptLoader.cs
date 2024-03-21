using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

		/// <summary>
		/// Contains references to be used when compiling scripts.
		/// </summary>
		/// <remarks>
		/// In most cases it should not be necessary to add any
		/// references to this list, as the script loader gets
		/// all references from the executing assembly automatically.
		/// There are some edge cases though, like during unit tests,
		/// where the executing assembly is not the one the scripts
		/// need.
		/// </remarks>
		public List<string> References { get; } = new List<string>();

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
		/// A list of exceptions thrown while resolving references.
		/// </summary>
		public List<ScriptLoadingException> ReferenceExceptions { get; } = new List<ScriptLoadingException>();

		/// <summary>
		/// A list of exceptions thrown while initializing the compiled
		/// scripts.
		/// </summary>
		public List<ScriptLoadingException> LoadingExceptions { get; } = new List<ScriptLoadingException>();

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public ScriptLoader()
			: this(null)
		{
		}

		/// <summary>
		/// Creates a new instance that caches the result in the given file.
		/// </summary>
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

			var csFiles = _filePaths.Where(a => a.EndsWith(".cs"));
			var assembly = this.Compile(csFiles);

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

				if (precompilers.Any())
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

				// Parse scripts
				var syntaxTrees = new List<SyntaxTree>();

				foreach (var filePath in filePaths)
				{
					var displayFilePath = filePath;

					if (mapFilePaths.TryGetValue(displayFilePath, out var mappedFilePath))
						displayFilePath = mappedFilePath;

					var source = File.ReadAllText(filePath);
					var syntaxTree = CSharpSyntaxTree.ParseText(source, path: displayFilePath);

					syntaxTrees.Add(syntaxTree);
				}

				// Prepare references

				// First we get some defaults necessary to compile a
				// standard library. Next we go through the references
				// added by the user, which might be necessary in some
				// cases. We also add assemblies referenced by the
				// explicit references, so the user doesn't have to
				// manage these manually. And finally we try to add
				// all references that the assemblies calling this
				// function may be using. All this is an effort to
				// reduce the number of references we have to add
				// manually and explicitly. Ideally we don't want
				// to have to think about them at all.

				var runTimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
				var netStandardPath = Path.Combine(runTimePath, "netstandard.dll");

				var toReference = new HashSet<string>
				{
					netStandardPath,
					typeof(object).Assembly.Location,
				};

				foreach (var userReferences in this.References)
				{
					toReference.Add(userReferences);

					var userAssembly = Assembly.LoadFile(userReferences);
					foreach (var reference in userAssembly.GetReferencedAssemblies())
					{
						var refAssembly = Assembly.Load(reference);
						toReference.Add(refAssembly.Location);
					}
				}

				var parentAssemblies = new[]
				{
					Assembly.GetEntryAssembly(),
					//Assembly.GetCallingAssembly(),
					//Assembly.GetExecutingAssembly(),
				};

				foreach (var assembly in parentAssemblies)
				{
					if (assembly == null)
						continue;

					toReference.Add(assembly.Location);

					foreach (var reference in assembly.GetReferencedAssemblies())
					{
						try
						{
							var refAssembly = Assembly.Load(reference);
							toReference.Add(refAssembly.Location);
						}
						catch (Exception ex)
						{
							this.ReferenceExceptions.Add(new ScriptLoadingException($"Failed to load reference '{reference.FullName}': {ex}"));
						}
					}
				}

				var references = toReference.Select(a => MetadataReference.CreateFromFile(a));

				// Compile scripts
				var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
				var compilation = CSharpCompilation.Create("scripts.compiled", syntaxTrees, references, options);

				using (var ms = new MemoryStream())
				{
					var compilationResult = compilation.Emit(ms);

					if (!compilationResult.Success)
					{
						var errors = new CompilerErrorCollection();

						foreach (var diagnostic in compilationResult.Diagnostics)
						{
							var isWarning = diagnostic.Severity == DiagnosticSeverity.Warning;

							if (diagnostic.Location == Location.None)
							{
								errors.Add(new CompilerError(null, 0, 0, diagnostic.ToString(), isWarning));
								continue;
							}

							var fileName = diagnostic.Location.SourceTree.FilePath;
							var lineSpan = diagnostic.Location.GetLineSpan();
							var line = lineSpan.StartLinePosition.Line + 1;
							var column = lineSpan.StartLinePosition.Character + 1;

							if (mapFilePaths.TryGetValue(fileName, out var mappedFileName))
								fileName = mappedFileName;

							errors.Add(new CompilerError(fileName, line, column, diagnostic.GetMessage(), isWarning));
						}

						throw new CompilerErrorException(errors);
					}

					ms.Seek(0, SeekOrigin.Begin);

					var assemblyBytes = ms.ToArray();
					var compiledAssembly = Assembly.Load(assemblyBytes);

					// Save assembly to file, ignoring access exceptions
					// because those are to be expected at run-time, when the
					// cached assembly was loaded into memory.
					if (_cacheFilePath != null)
					{
						try
						{
							var dirPath = Path.GetDirectoryName(_cacheFilePath);
							if (!Directory.Exists(dirPath))
								Directory.CreateDirectory(dirPath);

							File.Delete(_cacheFilePath);
							File.WriteAllBytes(_cacheFilePath, assemblyBytes);
						}
						catch (UnauthorizedAccessException)
						{
						}
					}

					return compiledAssembly;
				}
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
