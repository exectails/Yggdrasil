using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class VariablesTests
	{
		[Fact]
		public void SetGet()
		{
			var vars = new Yggdrasil.Util.Variables();
			vars.SetByte("byte", 1);
			vars.SetShort("short", 2);
			vars.SetInt("int", 3);
			vars.SetLong("long", 4);
			vars.SetFloat("float", 5.6f);
			vars.SetString("string", "seven");
			vars.SetBytes("bytes", new byte[] { 8, 9 });
			vars.SetBool("bool", true);

			Assert.Equal(1, vars.GetByte("byte"));
			Assert.Equal(2, vars.GetShort("short"));
			Assert.Equal(3, vars.GetInt("int"));
			Assert.Equal(4, vars.GetLong("long"));
			Assert.Equal(5.6f, vars.GetFloat("float"));
			Assert.Equal("seven", vars.GetString("string"));
			Assert.Equal(new byte[] { 8, 9 }, vars.GetBytes("bytes"));
			Assert.Equal(true, vars.GetBool("bool"));

			Assert.Equal(0, vars.GetByte("byte2"));
			Assert.Equal(0, vars.GetShort("short2"));
			Assert.Equal(0, vars.GetInt("int2"));
			Assert.Equal(0, vars.GetLong("long2"));
			Assert.Equal(0, vars.GetFloat("float2"));
			Assert.Equal(null, vars.GetString("string2"));
			Assert.Equal(null, vars.GetBytes("bytes2"));
			Assert.Equal(false, vars.GetBool("bool2"));
		}

		[Fact]
		public void ActivateOnce()
		{
			var vars = new Yggdrasil.Util.Variables();
			vars.SetBool("bool1", true);

			Assert.Equal(false, vars.ActivateOnce("bool1"));
			Assert.Equal(true, vars.ActivateOnce("bool2"));

			Assert.Equal(true, vars.GetBool("bool1"));
			Assert.Equal(true, vars.GetBool("bool2"));
			Assert.Equal(false, vars.GetBool("bool3"));
		}
	}
}
