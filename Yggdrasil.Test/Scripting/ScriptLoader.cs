// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;
using Xunit;
using Yggdrasil.Scripting;

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

			Assert.DoesNotThrow(() =>
			{
				try
				{
					var loader = new ScriptLoader(new CSharpCodeProvider());
					loader.LoadFromList(new[] { tmpFilePath });

					Console.WriteLine("LoadingExceptions");
					foreach (var err in loader.LoadingExceptions)
						Console.WriteLine(err);
				}
				catch (CompilerErrorException ex)
				{
					Console.WriteLine("CompilerErrorException");
					foreach (var err in ex.Errors)
						Console.WriteLine(err);
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

			Assert.DoesNotThrow(() =>
			{
				try
				{
					var loader = new ScriptLoader(new CSharpCodeProvider());
					loader.LoadFromList(new[] { tmpFilePath });

					Console.WriteLine("LoadingExceptions");
					foreach (var err in loader.LoadingExceptions)
						Console.WriteLine(err);
				}
				catch (CompilerErrorException ex)
				{
					Console.WriteLine("CompilerErrorException");
					foreach (var err in ex.Errors)
						Console.WriteLine(err);
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

			Assert.DoesNotThrow(() =>
			{
				try
				{
					var loader = new ScriptLoader(new CSharpCodeProvider());
					loader.AddPrecompiler(new TestPrecompiler());
					loader.LoadFromList(new[] { tmpFilePath });

					Console.WriteLine("LoadingExceptions");
					foreach (var err in loader.LoadingExceptions)
						Console.WriteLine(err);
				}
				catch (CompilerErrorException ex)
				{
					Console.WriteLine("CompilerErrorException");
					foreach (var err in ex.Errors)
						Console.WriteLine(err);
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
