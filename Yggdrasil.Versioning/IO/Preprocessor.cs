using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CodingSeb.ExpressionEvaluator;

namespace Yggdrasil.Versioning.IO
{
	/// <summary>
	/// Preprocessor for text-files inspired by C.
	/// </summary>
	/// <remarks>
	/// Supports basic versions of
	/// - #define
	/// - #undef
	/// - #if
	/// - #else
	/// - #elif
	/// - #endif
	/// - #include
	/// </remarks>
	public class Preprocessor
	{
		private ExpressionEvaluator _evaluator = new ExpressionEvaluator();

		private readonly Regex _commentRegex = new Regex(@"//.*", RegexOptions.Compiled);
		private readonly Regex _defineRegex = new Regex(@"#define\s+(?<identifier>[a-z][a-z0-9_]*)\s+(?<value>[a-z0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private readonly Regex _numberRegex = new Regex(@"^[-+]?[0-9]+$", RegexOptions.Compiled);
		private readonly Regex _undefRegex = new Regex(@"#undef\s+(?<identifier>[a-z][a-z0-9]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private readonly Regex _includeRegex = new Regex(@"#include\s+""(?<path>[^""]+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private readonly Regex _identifierRegex = new Regex(@"^[a-z][a-z0-9]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Defines variable, making the given identifier available in the
		/// processed data.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="value"></param>
		public void Define(string identifier, object value)
		{
			_evaluator.Variables[identifier] = value;
		}

		/// <summary>
		/// Undefines variable, making the given identifier unavailable in
		/// the processed data.
		/// </summary>
		/// <param name="identifier"></param>
		public void Undefine(string identifier)
		{
			_evaluator.Variables.Remove(identifier);
		}

		/// <summary>
		/// Undefines all variables, making all current identifiers
		/// unavailable in the processed data.
		/// </summary>
		public void UndefineAll()
		{
			_evaluator.Variables.Clear();
		}

		/// <summary>
		/// Returns variable by name via out if it exists. Returns whether
		/// the variable is defined or not.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetDefined(string identifier, out object value)
		{
			return _evaluator.Variables.TryGetValue(identifier, out value);
		}

		/// <summary>
		/// Returns true if a variable with the given name was defined.
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public bool DefinitionExists(string identifier)
		{
			return _evaluator.Variables.ContainsKey(identifier);
		}

		/// <summary>
		/// Processes the given file and returns it as a fully processed
		/// string.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public string ProcessFile(string filePath)
		{
			var result = new StringBuilder();
			var workingDirPath = Path.GetDirectoryName(filePath);

			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				foreach (var line in this.ProcessLines(filePath, workingDirPath, fs))
					result.AppendLine(line);
			}

			return result.ToString();
		}

		/// <summary>
		/// Processes given file and returns the processed lines.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public IEnumerable<string> ProcessLines(string filePath)
		{
			var workingDirPath = Path.GetDirectoryName(filePath);

			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				foreach (var line in this.ProcessLines(filePath, workingDirPath, fs))
					yield return line;
			}
		}

		/// <summary>
		/// Processes the given lines and returns them, using the working
		/// directory as a reference for relative includes.
		/// </summary>
		/// <param name="filePath">Path to the file the lines are being processed from. (Reference only.)</param>
		/// <param name="workingDirPath">Path to working directory used for relative includes.</param>
		/// <param name="stream">Stream to read the lines from.</param>
		public IEnumerable<string> ProcessLines(string filePath, string workingDirPath, Stream stream)
		{
			var lineNumber = 0;
			var skipLevel = 0;
			var ifStack = new Stack<bool>();

			using (var reader = new StreamReader(stream))
			{
				string rawLine;

				while ((rawLine = reader.ReadLine()) != null)
				{
					var line = rawLine.Trim();
					lineNumber++;

					// Skip pure lines
					if (line.StartsWith("//"))
						continue;

					// Remove trailing comments
					line = _commentRegex.Replace(line, "").Trim();

					// Handle #if, checking the expression and setting a
					// flag if the #if's body is supposed to get skipped.
					if (line.StartsWith("#if"))
					{
						var level = ifStack.Count + 1;

						// Ignore this #if if the processor is currently
						// skipping
						if (skipLevel != 0)
						{
							ifStack.Push(false);
							continue;
						}

						// Evaluate the expression
						var expression = line.Substring(3).Trim();
						var stepInside = this.EvaluateIfCondition(expression);

						ifStack.Push(stepInside);

						// If the expression evaluated to true, mark this
						// if as handled and continue. Otherwise, mark it
						// as not handled and start skipping.
						if (!stepInside)
						{
							skipLevel = level;
						}

						continue;
					}
					// Handle #elif, checking the expression and start
					// skipping it didn't evaluate to true. Only applies
					// if hasn't been handled yet.
					else if (line.StartsWith("#elif"))
					{
						var level = ifStack.Count;

						// Check that an #if preceded this #elif
						if (level == 0)
							throw new PreprocessorException($"Unexpected 'elif' without matching 'if'.", filePath, lineNumber);

						var handled = ifStack.Peek();

						// Ignore this #elif if another #if or #elif was
						// already executed
						if (handled)
						{
							skipLevel = level;
							continue;
						}

						// Ignore this #elif if the processor is currently
						// skipping from a lower level
						if (skipLevel != 0 && level != skipLevel)
							continue;

						// Evaluate the expression
						var expression = line.Substring(5).Trim();
						var stepInside = this.EvaluateIfCondition(expression);

						// Update stack with new evaluated handled state
						ifStack.Pop();
						ifStack.Push(stepInside);

						// If the expression evaluated to true, stop
						// skipping and mark the if as handled. If not,
						// start or continue skipping and mark the if as
						// not handled.
						if (stepInside)
						{
							skipLevel = 0;
						}
						else
						{
							skipLevel = level;
						}

						continue;
					}
					// Handle #else, which is entered if the #if hasn't
					// been handled yet..
					else if (line.StartsWith("#else"))
					{
						var level = ifStack.Count;

						// Check that an #if preceded this #else
						if (level == 0)
							throw new PreprocessorException($"Unexpected 'else' without matching 'if'.", filePath, lineNumber);

						var handled = ifStack.Peek();

						// Ignore this #else if an #if or #elif was
						// already executed
						if (handled)
						{
							skipLevel = level;
							continue;
						}

						// Ignore this #else if the processor is currently
						// skipping from a lower level
						if (skipLevel != 0 && level != skipLevel)
							continue;

						// Stop skipping since the #if wasn't handled yet
						// and mark the #if as handled.
						skipLevel = 0;
						ifStack.Pop();
						ifStack.Push(true);

						continue;
					}
					// Handle #endif, stopping the skipping enabled by a
					// previous #if or #elif
					else if (line.StartsWith("#endif"))
					{
						var level = ifStack.Count;

						// Check that an #if preceded this #endif
						if (level == 0)
							throw new PreprocessorException("Unexpected 'endif' without matching 'if'.", filePath, lineNumber);

						// Reset skipping if this #endif belongs to the
						// #if that initiated the skipping.
						if (level == skipLevel)
							skipLevel = 0;

						// Remove the if from tracking and lower the level
						// as we're leaving the #if.
						ifStack.Pop();

						continue;
					}

					// Skip line if an #if enabled skipping because its
					// body is supposed to be ignored
					if (skipLevel > 0)
						continue;

					// Handle #define, defining a variable
					if (line.StartsWith("#define"))
					{
						var match = _defineRegex.Match(line);
						if (!match.Success)
							throw new PreprocessorException("Invalid define.", filePath, lineNumber);

						var identifier = match.Groups["identifier"].Value;
						var value = match.Groups["value"].Value;

						if (value == "true")
							this.Define(identifier, true);
						else if (value == "false")
							this.Define(identifier, false);
						else if (_numberRegex.IsMatch(value))
							this.Define(identifier, Convert.ToInt32(value));
						else
							this.Define(identifier, value);

						continue;
					}
					// Handle #undef, undefining a variable
					else if (line.StartsWith("#undef"))
					{
						var match = _undefRegex.Match(line);
						if (!match.Success)
							throw new PreprocessorException("Invalid undef.", filePath, lineNumber);

						var identifier = match.Groups["identifier"].Value;
						this.Undefine(identifier);

						continue;
					}
					// Handle #include, which lets the the processor
					// branch off to process another file before going to
					// the next line of the current one
					else if (line.StartsWith("#include"))
					{
						var match = _includeRegex.Match(line);
						if (!match.Success)
							throw new PreprocessorException("Invalid include. Missing quotes?", filePath, lineNumber);

						var includedFilePath = match.Groups["path"].Value.Replace("\\", "/");
						if (includedFilePath.StartsWith("/"))
						{
							includedFilePath = includedFilePath.Substring(1);
							workingDirPath = Directory.GetCurrentDirectory();
						}

						var fullPath = Path.Combine(workingDirPath, includedFilePath);
						if (File.Exists(fullPath))
						{
							foreach (var includedLine in this.ProcessLines(fullPath))
								yield return includedLine;
						}

						continue;
					}
					else if (line.StartsWith("#"))
					{
						throw new PreprocessorException($"Unknown directive '{line}'", filePath, lineNumber);
					}

					yield return line;
				}
			}
		}

		/// <summary>
		/// Evaluates expression and returns whether it evalutes to true
		/// or false.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		private bool EvaluateIfCondition(string expression)
		{
			var stepInside = false;

			if (_identifierRegex.IsMatch(expression))
			{
				if (_evaluator.Variables.TryGetValue(expression, out var value) && value != null)
				{
					switch (value)
					{
						case bool boolValue: if (boolValue) stepInside = true; break;
						case int intValue: if (intValue != 0) stepInside = true; break;
						default: stepInside = true; break;
					}
				}
			}
			else
			{
				stepInside = _evaluator.Evaluate<bool>(expression);
			}

			return stepInside;
		}

		/// <summary>
		/// An exception thrown by the preprocessor when it encounters an error.
		/// </summary>
		public class PreprocessorException : Exception
		{
			/// <summary>
			/// Returns the path to the file where the error occurred.
			/// </summary>
			public string FilePath { get; }

			/// <summary>
			/// Returns the line number where the error occurred.
			/// </summary>
			public int Line { get; }

			/// <summary>
			/// Creates new instance.
			/// </summary>
			/// <param name="message"></param>
			/// <param name="filePath"></param>
			/// <param name="line"></param>
			public PreprocessorException(string message, string filePath, int line)
				: base($"Error in file '{filePath}' at line {line}: {message}")
			{
				this.FilePath = filePath;
				this.Line = line;
			}
		}
	}
}
