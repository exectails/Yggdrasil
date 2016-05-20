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
			var mgr = new TestCommandManager();
			mgr.Add(new TestCommand("test1", "-", "One test command.", Handle1));
			mgr.Add(new TestCommand("test2", "/", "Two test commands.", Handle2));

			var cmd1 = mgr.GetCommand("test1");
			var cmd2 = mgr.GetCommand("test2");

			Assert.Equal("test1", cmd1.Name);
			Assert.Equal("-", cmd1.Usage);
			Assert.Equal("One test command.", cmd1.Description);
			Assert.Equal(101, cmd1.Func());

			Assert.Equal("test2", cmd2.Name);
			Assert.Equal("/", cmd2.Usage);
			Assert.Equal("Two test commands.", cmd2.Description);
			Assert.Equal(202, cmd2.Func());
		}

		[Fact]
		public void Parsing()
		{
			var mgr = new TestCommandManager2();
			mgr.Add(new TestCommand2("test1", "<p1> <p2> <p3>", "", Handle3));

			var args1 = mgr.Parse("test1 lorem ipsum dolor");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor" }, (IEnumerable<string>)args1);

			var args2 = mgr.Parse("test1 lorem ipsum \"dolor sit amet\"");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor sit amet" }, (IEnumerable<string>)args2);

			var args3 = mgr.Parse("  test1 lorem    ipsum             dolor");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor" }, (IEnumerable<string>)args3);

			var args4 = mgr.Parse("  test1 lorem    ipsum      \"       dolor\"");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor" }, (IEnumerable<string>)args4);
		}

		private int Handle1()
		{
			return 101;
		}

		private int Handle2()
		{
			return 202;
		}

		private int Handle3(string[] args)
		{
			return args.Length;
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
			public IList<string> Parse(string input)
			{
				return this.ParseLine(input);
			}
		}

		private class TestCommand2 : Command<TestCommandHandler2>
		{
			public TestCommand2(string name, string usage, string description, TestCommandHandler2 handler)
				: base(name, usage, description, handler)
			{
			}
		}

		private delegate int TestCommandHandler2(params string[] args);
	}
}
