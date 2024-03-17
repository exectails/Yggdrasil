using System;
using Xunit;
using Yggdrasil.Parameters;

namespace Yggdrasil.Test.Parameters
{
	public class ParameterCollectionTests
	{
		[Fact]
		public void GetAndSetInt()
		{
			var col1 = new ParameterCollection<int>();
			col1.CreateParametersOnSet = false;

			Assert.Throws<ArgumentException>(() => col1.SetInt(IntParamId.Foo, 100));
			Assert.Throws<ArgumentException>(() => col1.GetInt(IntParamId.Foo));

			col1.Add(IntParamId.Foo, new IntParameter());
			col1.Add(IntParamId.Bar, new FloatParameter());
			col1.Add(IntParamId.Xyz, new StringParameter());

			col1.SetInt(IntParamId.Foo, 100);
			col1.SetFloat(IntParamId.Bar, 200.3f);
			col1.SetString(IntParamId.Xyz, "300");

			Assert.Equal(100, col1.GetInt(IntParamId.Foo));
			Assert.Equal(200.3f, col1.GetFloat(IntParamId.Bar));
			Assert.Equal("300", col1.GetString(IntParamId.Xyz));

			col1.CreateParametersOnSet = true;

			AssertEx.DoesNotThrow(() => col1.SetInt(IntParamId.Abc, 400));
			Assert.Equal(400, col1.GetInt(IntParamId.Abc));
		}

		[Fact]
		public void GetAndSetEnum()
		{
			var col1 = new ParameterCollection<EnumParamId>();
			col1.CreateParametersOnSet = false;

			Assert.Throws<ArgumentException>(() => col1.SetInt(EnumParamId.Foo, 100));
			Assert.Throws<ArgumentException>(() => col1.GetInt(EnumParamId.Foo));

			col1.Add(EnumParamId.Foo, new IntParameter());
			col1.Add(EnumParamId.Bar, new FloatParameter());
			col1.Add(EnumParamId.Xyz, new StringParameter());

			col1.SetInt(EnumParamId.Foo, 100);
			col1.SetFloat(EnumParamId.Bar, 200.3f);
			col1.SetString(EnumParamId.Xyz, "300");

			Assert.Equal(100, col1.GetInt(EnumParamId.Foo));
			Assert.Equal(200.3f, col1.GetFloat(EnumParamId.Bar));
			Assert.Equal("300", col1.GetString(EnumParamId.Xyz));

			col1.CreateParametersOnSet = true;

			AssertEx.DoesNotThrow(() => col1.SetInt(EnumParamId.Abc, 400));
			Assert.Equal(400, col1.GetInt(EnumParamId.Abc));
		}

		[Fact]
		public void GetDefault()
		{
			var col1 = new ParameterCollection<int>();
			col1.CreateParametersOnSet = false;

			Assert.Throws<ArgumentException>(() => col1.SetInt(IntParamId.Foo, 100));
			Assert.Throws<ArgumentException>(() => col1.GetInt(IntParamId.Foo));
			Assert.Equal(123, col1.GetInt(IntParamId.Foo, 123));

			col1.CreateParametersOnSet = true;

			col1.SetInt(IntParamId.Bar, 345);
			Assert.Equal(345, col1.GetInt(IntParamId.Bar, 123));
		}

		[Fact]
		public void MinMax()
		{
			var col1 = new ParameterCollection<EnumParamId>();
			col1.CreateParametersOnSet = false;

			col1.Add(EnumParamId.Bar, new FloatParameter(0, 0, 1000));

			col1.SetFloat(EnumParamId.Bar, -5);
			Assert.Equal(0, col1.GetFloat(EnumParamId.Bar));

			col1.ModifyFloat(EnumParamId.Bar, 5);
			Assert.Equal(5, col1.GetFloat(EnumParamId.Bar));

			col1.ModifyFloat(EnumParamId.Bar, -10);
			Assert.Equal(0, col1.GetFloat(EnumParamId.Bar));

			col1.SetFloat(EnumParamId.Bar, 2000);
			Assert.Equal(1000, col1.GetFloat(EnumParamId.Bar));

			col1.ModifyFloat(EnumParamId.Bar, -10);
			Assert.Equal(990, col1.GetFloat(EnumParamId.Bar));

			col1.ModifyFloat(EnumParamId.Bar, 20);
			Assert.Equal(1000, col1.GetFloat(EnumParamId.Bar));
		}

		[Fact]
		public void Modify()
		{
			var col1 = new ParameterCollection<EnumParamId>();

			col1.CreateParametersOnModify = false;
			Assert.Throws<ArgumentException>(() => col1.ModifyFloat(EnumParamId.Foo, 100));
			Assert.False(col1.TryGet<FloatParameter>(EnumParamId.Foo, out _));

			col1.CreateParametersOnModify = true;
			AssertEx.DoesNotThrow(() => col1.ModifyFloat(EnumParamId.Foo, 400));
			Assert.True(col1.TryGet<FloatParameter>(EnumParamId.Foo, out _));
			Assert.Equal(400, col1.GetFloat(EnumParamId.Foo));
		}

		[Fact]
		public void ImplicitConversionInt()
		{
			var col1 = new ParameterCollection<EnumParamId>();
			col1.Add(EnumParamId.Bar, new IntParameter(100));

			Assert.Equal(100, col1.GetInt(EnumParamId.Bar));

			var param1 = col1.Get<IntParameter>(EnumParamId.Bar);
			var val2 = 100;
			var val3 = 150;

			var result = param1 + val2 + val3;
			col1.SetInt(EnumParamId.Bar, result);
			Assert.Equal(350, col1.GetInt(EnumParamId.Bar));
		}

		[Fact]
		public void ImplicitConversionFloat()
		{
			var col1 = new ParameterCollection<EnumParamId>();
			col1.Add(EnumParamId.Bar, new FloatParameter(100));

			Assert.Equal(100, col1.GetFloat(EnumParamId.Bar));

			var param1 = col1.Get<FloatParameter>(EnumParamId.Bar);
			var val2 = 100;
			var val3 = 150.5f;

			var result = param1 + val2 + val3;
			col1.SetFloat(EnumParamId.Bar, result);
			Assert.Equal(350.5f, col1.GetFloat(EnumParamId.Bar));
		}

		[Fact]
		public void ValueChanged()
		{
			var col1 = new ParameterCollection<EnumParamId>();
			col1.CreateParametersOnSet = false;

			var maxHp = 1000;

			col1.Add(EnumParamId.HpMax, new FloatParameter(maxHp, 0));
			col1.Add(EnumParamId.Hp, new FloatParameter(maxHp, 0, maxHp));

			col1.Get<FloatParameter>(EnumParamId.HpMax).ValueChanged += (changedParam) =>
			{
				var hpParam = col1.Get<FloatParameter>(EnumParamId.Hp);
				var hpMaxParam = (FloatParameter)changedParam;

				hpParam.MaxValue = hpMaxParam.Value;
			};

			col1.ModifyFloat(EnumParamId.Hp, -100);
			Assert.Equal(900, col1.GetFloat(EnumParamId.Hp));

			col1.ModifyFloat(EnumParamId.Hp, 200);
			Assert.Equal(1000, col1.GetFloat(EnumParamId.Hp));

			col1.SetFloat(EnumParamId.HpMax, 1300);
			col1.ModifyFloat(EnumParamId.Hp, 200);
			Assert.Equal(1200, col1.GetFloat(EnumParamId.Hp));

			col1.ModifyFloat(EnumParamId.Hp, 200);
			Assert.Equal(1300, col1.GetFloat(EnumParamId.Hp));

			col1.SetFloat(EnumParamId.HpMax, 500);
			Assert.Equal(500, col1.GetFloat(EnumParamId.Hp));
		}

		private static class IntParamId
		{
			public const int Foo = 1;
			public const int Bar = 2;
			public const int Xyz = 3;
			public const int Abc = 4;
		}

		private enum EnumParamId
		{
			Bar,
			Abc,
			Xyz,
			Foo,

			Hp,
			HpMax,
		}
	}
}
