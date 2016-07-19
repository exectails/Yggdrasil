// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Microsoft.CSharp.RuntimeBinder;
using System;
using Xunit;
using Yggdrasil.Scripting;

namespace Yggdrasil.Test.Scripting
{
	public class VariableManagerTests
	{
		[Fact]
		public void GettingAndSettingFields()
		{
			dynamic mgr = new VariableManager();
			mgr.Test1 = 1;
			mgr.Test2 = "two";
			mgr.Test3 = mgr.Test1 + 2;
			mgr.Test4 = mgr.Test2 + 2;

			Assert.Equal(1, mgr.Test1);
			Assert.Equal("two", mgr.Test2);
			Assert.Equal(3, mgr.Test3);
			Assert.Equal("two2", mgr.Test4);
		}

		[Fact]
		public void GettingAndSettingIndex()
		{
			var mgr = new VariableManager();
			mgr["Test1"] = 1;
			mgr["Test2"] = "two";
			mgr["Test3"] = mgr["Test1"] + 2;
			mgr["Test4"] = mgr["Test2"] + 2;

			Assert.Equal(1, mgr["Test1"]);
			Assert.Equal("two", mgr["Test2"]);
			Assert.Equal(3, mgr["Test3"]);
			Assert.Equal("two2", mgr["Test4"]);
		}

		[Fact]
		public void InitializingVariables()
		{
			dynamic mgr = new VariableManager();
			mgr.Test1 = 1;
			mgr.Test2 = "two";
			mgr.Test3 = mgr.Test1 + 2;
			mgr.Test4 = mgr.Test2 + 2;

			var mgr2 = new VariableManager(mgr.GetList());

			Assert.Equal(1, mgr["Test1"]);
			Assert.Equal("two", mgr["Test2"]);
			Assert.Equal(3, mgr["Test3"]);
			Assert.Equal("two2", mgr["Test4"]);
		}

		// The following tests don't work on Mono for some reason...
#if !__MonoCS__
		[Fact]
		public void Get()
		{
			dynamic mgr = new VariableManager();
			mgr.Test1 = (byte)1;
			mgr.Test2 = "two";
			mgr.Test3 = 3;

			Assert.Equal(1, mgr.Get<byte>("Test1", 0));
			Assert.Equal("two", mgr.Get<string>("Test2", ""));
			Assert.Equal(3, mgr.Get<int>("Test3", 0));

			Assert.Equal(127, mgr.Get<byte>("Test4", 127));

			Assert.Throws<InvalidCastException>(() => { Assert.Equal(1, mgr.Get<int>("Test1", 0)); });
		}

		[Fact]
		public void Assign()
		{
			dynamic mgr = new VariableManager();
			mgr.Test1 = (byte)1;
			mgr.Test2 = "two";
			mgr.Test3 = 3;

			byte test1 = mgr["Test1"];
			string test2 = mgr["Test2"];
			int test3 = mgr["Test3"];

			Assert.Equal((byte)1, test1);
			Assert.Equal("two", test2);
			Assert.Equal(3, test3);

			Assert.Throws<RuntimeBinderException>(() => { int test4 = mgr["Test4"]; });
		}
#endif
	}
}
