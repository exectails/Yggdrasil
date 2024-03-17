using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Yggdrasil.Scripting;

namespace Yggdrasil.Test.Scripting
{
	public class ScriptLoaderTests
	{
		private readonly ITestOutputHelper _output;

		public ScriptLoaderTests(ITestOutputHelper output)
		{
			_output = output;
		}

		public static int Test = 0;
		public static readonly Dictionary<string, IScript> Scripts = [];

		private ScriptLoader CreateLoader()
		{
			var loader = new ScriptLoader();
			loader.References.Add(typeof(ScriptLoaderTests).Assembly.Location);

			return loader;
		}

		[Fact]
		public void LoadFromList()
		{
			var testScript = @"
using Yggdrasil.Scripting;
using Yggdrasil.Test.Scripting;

class TestScript1 : IScript
{
	public bool Init()
	{
		ScriptLoaderTests.Test = 1;
		return true;
	}
}
";
			var tmpFilePath = Path.GetTempFileName() + ".cs";
			File.WriteAllText(tmpFilePath, testScript);

			AssertEx.DoesNotThrow(() =>
			{
				try
				{
					var loader = this.CreateLoader();
					loader.LoadFromList(new[] { tmpFilePath });

					if (loader.LoadingExceptions.Count > 0)
					{
						_output.WriteLine("LoadingExceptions");

						foreach (var err in loader.LoadingExceptions)
							_output.WriteLine(err.ToString());
					}
				}
				catch (CompilerErrorException ex)
				{
					_output.WriteLine("CompilerErrorException");

					foreach (var err in ex.Errors)
						_output.WriteLine(err.ToString());

					throw;
				}
			});

			Assert.Equal(1, Test);

			File.Delete(tmpFilePath);
		}

		[Fact]
		public void CallFunction()
		{
			var testScript = @"
using Yggdrasil.Scripting;
using Yggdrasil.Test.Scripting;

class TestScript2 : IScript, IFoobarer
{
	public bool Init()
	{
		ScriptLoaderTests.Scripts[""TestScript2""] = this;
		return true;
	}

	public int Foobar()
	{
		return 2;
	}
}
";
			var tmpFilePath = Path.GetTempFileName() + ".cs";
			File.WriteAllText(tmpFilePath, testScript);

			AssertEx.DoesNotThrow(() =>
			{
				try
				{
					var loader = this.CreateLoader();
					loader.LoadFromList(new[] { tmpFilePath });

					if (loader.LoadingExceptions.Count > 0)
					{
						_output.WriteLine("LoadingExceptions");

						foreach (var err in loader.LoadingExceptions)
							_output.WriteLine(err.ToString());
					}
				}
				catch (CompilerErrorException ex)
				{
					_output.WriteLine("CompilerErrorException");

					foreach (var err in ex.Errors)
						_output.WriteLine(err.ToString());

					throw;
				}
			});

			var script = Scripts["TestScript2"] as IFoobarer;
			Assert.NotNull(script);

			Test = script.Foobar();
			Assert.Equal(2, Test);

			File.Delete(tmpFilePath);
		}

		[Fact]
		public void Precompile()
		{
			Test = 0;

			var testScript = "ScriptLoaderTests.Test = 42;";

			var tmpFilePath = Path.GetTempFileName() + ".cs";
			File.WriteAllText(tmpFilePath, testScript);

			AssertEx.DoesNotThrow(() =>
			{
				try
				{
					var loader = this.CreateLoader();
					loader.AddPrecompiler(new TestPrecompiler());
					loader.LoadFromList(new[] { tmpFilePath });

					if (loader.LoadingExceptions.Count > 0)
					{
						_output.WriteLine("LoadingExceptions");

						foreach (var err in loader.LoadingExceptions)
							_output.WriteLine(err.ToString());
					}
				}
				catch (CompilerErrorException ex)
				{
					_output.WriteLine("CompilerErrorException");

					foreach (var err in ex.Errors)
						_output.WriteLine(err.ToString());

					throw;
				}
			});

			Assert.Equal(42, Test);

			File.Delete(tmpFilePath);
		}
	}

	public interface IFoobarer
	{
		int Foobar();
	}

	public class TestPrecompiler : IPrecompiler
	{
		public bool Precompile(string filePath, ref string script)
		{
			script = @"
using Yggdrasil.Scripting;
using Yggdrasil.Test.Scripting;

class TestScript2 : IScript
{
	public bool Init()
	{
		" + script + @"
		return true;
	}
}";
			return true;
		}
	}
}
