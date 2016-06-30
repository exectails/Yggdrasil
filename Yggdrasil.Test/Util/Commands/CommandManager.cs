// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Xunit;
using Yggdrasil.Util.Commands;

namespace Yggdrasil.Test.Util.Commands
{
	public class CommandManagerTest
	{
		[Fact]
		public void AddingAndGetting()
		{
			var mgr1 = new TestCommandManager();
			mgr1.Add(new TestCommand("test1", "-", "One test command.", Handle1));
			mgr1.Add(new TestCommand("test2", "/", "Two test commands.", Handle2));

			var cmd1 = mgr1.GetCommand("test1");
			var cmd2 = mgr1.GetCommand("test2");

			Assert.Equal("test1", cmd1.Name);
			Assert.Equal("-", cmd1.Usage);
			Assert.Equal("One test command.", cmd1.Description);
			Assert.Equal(101, cmd1.Func());

			Assert.Equal("test2", cmd2.Name);
			Assert.Equal("/", cmd2.Usage);
			Assert.Equal("Two test commands.", cmd2.Description);
			Assert.Equal(202, cmd2.Func());

			var mgr2 = new TestCommandManager2();
			mgr2.Add(new TestCommand2("test3", "<p1> <p2> <p3>", "", Handle3));

			var cmd3 = mgr2.GetCommand("test3");
			var args = new Arguments("test3 9182 foo foo:bar");
			Assert.Equal(4, cmd3.Func(args));
		}

		private int Handle1()
		{
			return 101;
		}

		private int Handle2()
		{
			return 202;
		}

		private int Handle3(Arguments args)
		{
			Assert.Equal("test3", args.Get(0));
			Assert.Equal("9182", args.Get(1));
			Assert.Equal("foo", args.Get(2));
			Assert.Equal("bar", args.Get("foo"));

			return args.Count;
		}

		// ------------------------------------------------------------------

		private class TestCommandManager : CommandManager<TestCommand, TestCommandHandler>
		{
		}

		private class TestCommand : Command<TestCommandHandler>
		{
			public TestCommand(string name, string usage, string description, TestCommandHandler handler)
				: base(name, usage, description, handler)
			{
			}
		}

		private delegate int TestCommandHandler();

		// ------------------------------------------------------------------

		private class TestCommandManager2 : CommandManager<TestCommand2, TestCommandHandler2>
		{
		}

		private class TestCommand2 : Command<TestCommandHandler2>
		{
			public TestCommand2(string name, string usage, string description, TestCommandHandler2 handler)
				: base(name, usage, description, handler)
			{
			}
		}

		private delegate int TestCommandHandler2(Arguments args);
	}
}
