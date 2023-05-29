using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Yggdrasil.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Yggdrasil.Test.Scripting
{
	public class ScriptLoaderTests
	{
		public static int Test = 0;
		public static Dictionary<string, IScript> Scripts = new Dictionary<string, IScript>();

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

			var exception = Record.Exception(() =>
			{
				try
				{
					var loader = new ScriptLoader();
					loader.LoadFromList(new[] { tmpFilePath });

					Console.WriteLine("LoadingExceptions");
					foreach (var err in loader.LoadingExceptions)
						Console.WriteLine(err);
				}
				catch (Exception)
				{
					Console.WriteLine("CompilerErrorException");
					throw;
				}
			});
			Assert.Null(exception);
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

			var exception = Record.Exception(() =>
			{
				try
				{
					var loader = new ScriptLoader();
					loader.LoadFromList(new[] { tmpFilePath });

					Console.WriteLine("LoadingExceptions");
					foreach (var err in loader.LoadingExceptions)
						Console.WriteLine(err);
				}
				catch (Exception)
				{
					Console.WriteLine("CompilerErrorException");
					throw;
				}
			});
			Assert.Null(exception);
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

			var exception = Record.Exception(() =>
			{
				try
				{
					var loader = new ScriptLoader();
					loader.AddPrecompiler(new TestPrecompiler());
					loader.LoadFromList(new[] { tmpFilePath });

					Console.WriteLine("LoadingExceptions");
					foreach (var err in loader.LoadingExceptions)
						Console.WriteLine(err);
				}
				catch (Exception)
				{
					Console.WriteLine("CompilerErrorException");
					throw;
				}
			});
			Assert.Null(exception);
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
