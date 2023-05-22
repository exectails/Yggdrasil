using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Yggdrasil.Variables;
using Yggdrasil.Variables.DefaultGetters;
using static Yggdrasil.Variables.VariableContainer<string>;

namespace Yggdrasil.Test.Variables
{
	public class VariableContainerTests
	{
		[Fact]
		public void CreateDefaults()
		{
			var container = new VariableContainer<string>();
			container.AutoCreate = true; // Default

			var b1 = container.Create(new ByteVariable("b1"));
			Assert.Equal(0, b1.Value);
			Assert.Equal(byte.MinValue, b1.MinValue);
			Assert.Equal(byte.MaxValue, b1.MaxValue);

			var b2 = container.Create(new SByteVariable("b2"));
			Assert.Equal(0, b2.Value);
			Assert.Equal(sbyte.MinValue, b2.MinValue);
			Assert.Equal(sbyte.MaxValue, b2.MaxValue);

			var s1 = container.Create(new ShortVariable("s1"));
			Assert.Equal(0, s1.Value);
			Assert.Equal(short.MinValue, s1.MinValue);
			Assert.Equal(short.MaxValue, s1.MaxValue);

			var s2 = container.Create(new UShortVariable("s2"));
			Assert.Equal(0, s2.Value);
			Assert.Equal(ushort.MinValue, s2.MinValue);
			Assert.Equal(ushort.MaxValue, s2.MaxValue);

			var i1 = container.Create(new IntVariable("i1"));
			Assert.Equal(0, i1.Value);
			Assert.Equal(int.MinValue, i1.MinValue);
			Assert.Equal(int.MaxValue, i1.MaxValue);

			var i2 = container.Create(new UIntVariable("i2"));
			Assert.Equal(0U, i2.Value);
			Assert.Equal(uint.MinValue, i2.MinValue);
			Assert.Equal(uint.MaxValue, i2.MaxValue);

			var l1 = container.Create(new LongVariable("l1"));
			Assert.Equal(0L, l1.Value);
			Assert.Equal(long.MinValue, l1.MinValue);
			Assert.Equal(long.MaxValue, l1.MaxValue);

			var l2 = container.Create(new ULongVariable("l2"));
			Assert.Equal(0UL, l2.Value);
			Assert.Equal(ulong.MinValue, l2.MinValue);
			Assert.Equal(ulong.MaxValue, l2.MaxValue);

			var f1 = container.Create(new FloatVariable("f1"));
			Assert.Equal(0f, f1.Value);
			Assert.Equal(float.MinValue, f1.MinValue);
			Assert.Equal(float.MaxValue, f1.MaxValue);

			var d1 = container.Create(new DoubleVariable("f2"));
			Assert.Equal(0d, d1.Value);
			Assert.Equal(double.MinValue, d1.MinValue);
			Assert.Equal(double.MaxValue, d1.MaxValue);

			var str1 = container.Create(new StringVariable("str1"));
			Assert.Equal(null, str1.Value);
		}

		[Fact]
		public void CreateMinMax()
		{
			var container = new VariableContainer<string>();
			container.AutoCreate = true; // Default

			var b1 = container.Create(new ByteVariable("b1", 0, byte.MinValue / 2, byte.MaxValue / 2));
			Assert.Equal(0, b1.Value);
			Assert.Equal(byte.MinValue / 2, b1.MinValue);
			Assert.Equal(byte.MaxValue / 2, b1.MaxValue);

			var b2 = container.Create(new SByteVariable("b2", 0, sbyte.MinValue / 2, sbyte.MaxValue / 2));
			Assert.Equal(0, b2.Value);
			Assert.Equal(sbyte.MinValue / 2, b2.MinValue);
			Assert.Equal(sbyte.MaxValue / 2, b2.MaxValue);

			var s1 = container.Create(new ShortVariable("s1", 0, short.MinValue / 2, short.MaxValue / 2));
			Assert.Equal(0, s1.Value);
			Assert.Equal(short.MinValue / 2, s1.MinValue);
			Assert.Equal(short.MaxValue / 2, s1.MaxValue);

			var s2 = container.Create(new UShortVariable("s2", 0, ushort.MinValue / 2, ushort.MaxValue / 2));
			Assert.Equal(0, s2.Value);
			Assert.Equal(ushort.MinValue / 2, s2.MinValue);
			Assert.Equal(ushort.MaxValue / 2, s2.MaxValue);

			var i1 = container.Create(new IntVariable("i1", 0, int.MinValue / 2, int.MaxValue / 2));
			Assert.Equal(0, i1.Value);
			Assert.Equal(int.MinValue / 2, i1.MinValue);
			Assert.Equal(int.MaxValue / 2, i1.MaxValue);

			var i2 = container.Create(new UIntVariable("i2", 0, uint.MinValue / 2, uint.MaxValue / 2));
			Assert.Equal(0U, i2.Value);
			Assert.Equal(uint.MinValue / 2, i2.MinValue);
			Assert.Equal(uint.MaxValue / 2, i2.MaxValue);

			var l1 = container.Create(new LongVariable("l1", 0, long.MinValue / 2, long.MaxValue / 2));
			Assert.Equal(0L, l1.Value);
			Assert.Equal(long.MinValue / 2, l1.MinValue);
			Assert.Equal(long.MaxValue / 2, l1.MaxValue);

			var l2 = container.Create(new ULongVariable("l2", 0, ulong.MinValue / 2, ulong.MaxValue / 2));
			Assert.Equal(0UL, l2.Value);
			Assert.Equal(ulong.MinValue / 2, l2.MinValue);
			Assert.Equal(ulong.MaxValue / 2, l2.MaxValue);

			var f1 = container.Create(new FloatVariable("f1", 0, float.MinValue / 2, float.MaxValue / 2));
			Assert.Equal(0f, f1.Value);
			Assert.Equal(float.MinValue / 2, f1.MinValue);
			Assert.Equal(float.MaxValue / 2, f1.MaxValue);

			var d1 = container.Create(new DoubleVariable("f2", 0, double.MinValue / 2, double.MaxValue / 2));
			Assert.Equal(0d, d1.Value);
			Assert.Equal(double.MinValue / 2, d1.MinValue);
			Assert.Equal(double.MaxValue / 2, d1.MaxValue);
		}

		[Fact]
		public void CreateDynamically()
		{
			var container = new VariableContainer<string>();
			container.AutoCreate = true; // Default

			Assert.Equal(false, container.Has("i1"));
			Assert.Equal(false, container.Has("i2"));
			Assert.Equal(false, container.Has("i3"));
			Assert.Equal(false, container.Has("i4"));

			var i1 = container.Int("i1", 42);
			Assert.Equal(42, i1.Value);

			var i2 = container.Int("i2").Value = 2;
			Assert.Equal(2, container.Int("i2").Value);

			var i3 = container.Int("i3", i1 * i2);
			Assert.Equal(84, i3.Value);

			container.AutoCreate = false;
			Assert.Throws<NullReferenceException>(() => container.Int("i4").Value = 21);

			Assert.Equal(true, container.Has("i1"));
			Assert.Equal(true, container.Has("i2"));
			Assert.Equal(true, container.Has("i3"));
			Assert.Equal(false, container.Has("i4"));
		}

		[Fact]
		public void ValueChanged()
		{
			var container = new VariableContainer<string>();

			var i1 = container.Int("i1", 3);
			var i2 = container.Int("i2");

			i1.ValueChanged += variable => i2.Value = (variable as IntVariable).Value * 2;

			Assert.Equal(3, i1.Value);
			Assert.Equal(0, i2.Value);

			i1.Value = 5;
			Assert.Equal(5, i1.Value);
			Assert.Equal(10, i2.Value);

			i1.Value *= 2;
			Assert.Equal(10, i1.Value);
			Assert.Equal(20, i2.Value);

			i1.Value /= 10;
			Assert.Equal(1, i1.Value);
			Assert.Equal(2, i2.Value);
		}

		[Fact]
		public void TypeMismatch()
		{
			var container = new VariableContainer<string>();

			container.Create(new StringVariable("str1", "3"));
			Assert.Equal("3", container.String("str1").Value);

			container.String("str1").Value = "1";
			Assert.Throws<TypeMismatchException>(() => container.Float("str1").Value = 1);
			Assert.Equal("1", container.String("str1").Value);
		}

		[Fact]
		public void Compare()
		{
			var container = new VariableContainer<string>();

			var foobarVar = container.Float("foobar", 0);

			for (var i = 0; i < 21; ++i)
				foobarVar.Value += i;

			foobarVar.Value *= 2;

			// Compare
			Console.WriteLine(foobarVar < 420);
			Console.WriteLine(foobarVar > 420);
			Console.WriteLine(foobarVar >= 420);
			Console.WriteLine(foobarVar <= 420);
			Console.WriteLine(foobarVar == 420);
			Console.WriteLine(foobarVar != 420);
			Console.WriteLine(420 == foobarVar);
			Console.WriteLine(420 != foobarVar);
		}

		[Fact]
		public void MinMax()
		{
			var container = new VariableContainer<string>();

			var i1 = container.Create(new IntVariable("i1", 10, 0, 50));
			Assert.Equal(10, i1.Value);
			Assert.Equal(0, i1.MinValue);
			Assert.Equal(50, i1.MaxValue);

			i1.Value += 100;
			Assert.Equal(50, i1.Value);

			i1.Value -= 100;
			Assert.Equal(0, i1.Value);

			i1.MinValue = 5;
			Assert.Equal(5, i1.Value);
			Assert.Equal(5, i1.MinValue);
			Assert.Equal(50, i1.MaxValue);

			i1.Value += 10;
			Assert.Equal(15, i1.Value);

			i1.MaxValue = 10;
			Assert.Equal(10, i1.Value);
			Assert.Equal(5, i1.MinValue);
			Assert.Equal(10, i1.MaxValue);

			i1.MinValue = 50;
			i1.MaxValue = 100;
			Assert.Equal(50, i1.Value);
			Assert.Equal(50, i1.MinValue);
			Assert.Equal(100, i1.MaxValue);

			i1.MinValue = 100;
			Assert.Equal(100, i1.Value);
			Assert.Equal(100, i1.MinValue);
			Assert.Equal(100, i1.MaxValue);

			i1.MinValue = 40;
			i1.MaxValue = 50;
			i1.Value = 45;
			Assert.Equal(45, i1.Value);
			Assert.Equal(40, i1.MinValue);
			Assert.Equal(50, i1.MaxValue);

			i1.MaxValue = 20;
			Assert.Equal(40, i1.Value);
			Assert.Equal(40, i1.MinValue);
			Assert.Equal(40, i1.MaxValue);
		}
	}
}
