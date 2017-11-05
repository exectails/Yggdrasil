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
			var tmpFilePath = Path.GetTempFileName();
			File.WriteAllText(tmpFilePath, testScript);

			Assert.DoesNotThrow(() =>
			{
				try
				{
					var loader = new ScriptLoader(new CSharpCodeProvider());
					loader.LoadFromList(new[] { tmpFilePath });
				}
				catch (CompilerErrorException ex)
				{
					foreach (var err in ex.Errors)
						Console.WriteLine(err);
					throw;
				}
			});

			Assert.Equal(1, Test);
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
			var tmpFilePath = Path.GetTempFileName();
			File.WriteAllText(tmpFilePath, testScript);

			Assert.DoesNotThrow(() =>
			{
				try
				{
					var loader = new ScriptLoader(new CSharpCodeProvider());
					loader.LoadFromList(new[] { tmpFilePath });
				}
				catch (CompilerErrorException ex)
				{
					foreach (var err in ex.Errors)
						Console.WriteLine(err);
					throw;
				}
			});

			var script = Scripts["TestScript2"] as IFoobarer;
			Assert.NotNull(script);

			Test = script.Foobar();

			Assert.Equal(2, Test);
		}
	}

	public interface IFoobarer
	{
		int Foobar();
	}
}
