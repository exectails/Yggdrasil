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

			var args5 = new Arguments("test with \" colon : colon \" end");
			Assert.Equal("test", args5.Get(0));
			Assert.Equal("with", args5.Get(1));
			Assert.Equal("colon : colon", args5.Get(2));
			Assert.Equal("end", args5.Get(3));
			Assert.Equal(new string[] { "test", "with", "colon : colon", "end" }, args5.GetAll());

			var args6 = new Arguments("\"colon:colon\"");
			Assert.Equal("colon:colon", args6.Get(0));
			Assert.Equal(new string[] { "colon:colon" }, args6.GetAll());

			var args7 = new Arguments("/test \"colon:colon\"");
			Assert.Equal("/test", args7.Get(0));
			Assert.Equal("colon:colon", args7.Get(1));
			Assert.Equal(new string[] { "/test", "colon:colon" }, args7.GetAll());
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
			Assert.Null(args1.Get("foobar"));
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
			Assert.Null(args1.Get("foobar"));

			// Quoted value with colon
			var args2 = new Arguments(">bc1 test_text:\"this is a: test\"");

			Assert.Equal(2, args2.Count);
			Assert.Equal(new string[] { ">bc1", "\"test_text:this is a: test\"" }, args2.GetAll());

			Assert.Equal(">bc1", args2.Get(0));
			Assert.Throws<ArgumentException>(() => args2.Get(1));

			Assert.Equal("this is a: test", args2.Get("test_text"));
			Assert.Null(args2.Get("foobar"));

			// Quoted value and non-quoted value
			var args3 = new Arguments(">bc2 test_text_2:\"this is a test\" color:0x00ff12");

			Assert.Equal(3, args3.Count);
			Assert.Equal(new string[] { ">bc2", "\"test_text_2:this is a test\"", "color:0x00ff12" }, args3.GetAll());

			Assert.Equal(">bc2", args3.Get(0));
			Assert.Throws<ArgumentException>(() => args3.Get(1));

			Assert.Equal("this is a test", args3.Get("test_text_2"));
			Assert.Equal("0x00ff12", args3.Get("color"));
			Assert.Null(args3.Get("foobar"));

			// Quoted value with spaces and whitespace padding
			var args4 = new Arguments(">bc3 test_text_3:\"  this is a test \" color:0x00ff13");

			Assert.Equal(3, args4.Count);
			Assert.Equal(new string[] { ">bc3", "\"test_text_3:this is a test\"", "color:0x00ff13" }, args4.GetAll());

			Assert.Equal(">bc3", args4.Get(0));
			Assert.Throws<ArgumentException>(() => args4.Get(1));

			Assert.Equal("this is a test", args4.Get("test_text_3"));
			Assert.Equal("0x00ff13", args4.Get("color"));
			Assert.Null(args4.Get("foobar"));
		}
	}
}
