// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Xunit;
using Yggdrasil.Util.Commands;

namespace Yggdrasil.Test.Util.Commands
{
	public class ArgumentsTest
	{
		[Fact]
		public void ParseIndexed()
		{
			var args1 = new Arguments("test1 lorem ipsum dolor");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor" }, args1.GetAll());

			var args2 = new Arguments("test1 lorem ipsum \"dolor sit amet\"");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor sit amet" }, args2.GetAll());

			var args3 = new Arguments("  test1 lorem    ipsum             dolor");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor" }, args3.GetAll());

			var args4 = new Arguments("  test1 lorem    ipsum      \"       dolor\"");
			Assert.Equal(new string[] { "test1", "lorem", "ipsum", "dolor" }, args4.GetAll());
		}

		[Fact]
		public void ParseMixed()
		{
			var args1 = new Arguments(">item 1234 c1:0 c2:white c3:0xff00ff prefix:5678");

			Assert.Equal(6, args1.Count);
			Assert.Equal(new string[] { ">item", "1234", "c1:0", "c2:white", "c3:0xff00ff", "prefix:5678" }, args1.GetAll());

			Assert.Equal(">item", args1.Get(0));
			Assert.Equal("1234", args1.Get(1));
			Assert.Throws<ArgumentException>(() => args1.Get(2));
			Assert.Throws<ArgumentException>(() => args1.Get(-1));

			Assert.Equal("0", args1.Get("c1"));
			Assert.Equal("white", args1.Get("c2"));
			Assert.Equal("0xff00ff", args1.Get("c3"));
			Assert.Equal("5678", args1.Get("prefix"));
			Assert.Equal("5678", args1.Get("prefix"));
			Assert.Equal(null, args1.Get("foobar"));
		}

		[Fact]
		public void ParseNamed()
		{
			// Quoted value
			var args1 = new Arguments(">broadcast text:\"this is a test\"");

			Assert.Equal(2, args1.Count);
			Assert.Equal(new string[] { ">broadcast", "\"text:this is a test\"" }, args1.GetAll());

			Assert.Equal(">broadcast", args1.Get(0));
			Assert.Throws<ArgumentException>(() => args1.Get(1));

			Assert.Equal("this is a test", args1.Get("text"));
			Assert.Equal(null, args1.Get("foobar"));

			// Quoted key and value
			var args2 = new Arguments(">bc1 \"test text\":\"this is a test\"");

			Assert.Equal(2, args2.Count);
			Assert.Equal(new string[] { ">bc1", "\"test text:this is a test\"" }, args2.GetAll());

			Assert.Equal(">bc1", args2.Get(0));
			Assert.Throws<ArgumentException>(() => args2.Get(1));

			Assert.Equal("this is a test", args2.Get("test text"));
			Assert.Equal(null, args2.Get("foobar"));

			// Quoted key with spaces
			var args3 = new Arguments(">bc2 \"test text 2\":\"this is a test\" color:0x00ff12");

			Assert.Equal(3, args3.Count);
			Assert.Equal(new string[] { ">bc2", "\"test text 2:this is a test\"", "color:0x00ff12" }, args3.GetAll());

			Assert.Equal(">bc2", args3.Get(0));
			Assert.Throws<ArgumentException>(() => args3.Get(1));

			Assert.Equal("this is a test", args3.Get("test text 2"));
			Assert.Equal("0x00ff12", args3.Get("color"));
			Assert.Equal(null, args3.Get("foobar"));

			// Quoted key and value in one with spaces and whitespace padding
			var args4 = new Arguments(">bc3 \"    test text 3  :  this is a test \" color:0x00ff13");

			Assert.Equal(3, args4.Count);
			Assert.Equal(new string[] { ">bc3", "\"test text 3:this is a test\"", "color:0x00ff13" }, args4.GetAll());

			Assert.Equal(">bc3", args4.Get(0));
			Assert.Throws<ArgumentException>(() => args4.Get(1));

			Assert.Equal("this is a test", args4.Get("test text 3"));
			Assert.Equal("0x00ff13", args4.Get("color"));
			Assert.Equal(null, args4.Get("foobar"));
		}
	}
}
