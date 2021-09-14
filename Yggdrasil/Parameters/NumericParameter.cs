using System;

namespace Yggdrasil.Parameters
{
	/// <summary>
	/// Describes a numeric parameter of some object or entity.
	/// </summary>
	public interface INumericParameter : IParameter
	{
		/// <summary>
		/// Returns the parameter's value as a double.
		/// </summary>
		double ValueDouble { get; }
	}

	/// <summary>
	/// Represents a parameter that's holding a numeric value.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public abstract class NumericParameter<TValue> : INumericParameter where TValue : IComparable<TValue>, IEquatable<TValue>
	{
		private TValue _value;
		private TValue _minValue;
		private TValue _maxValue;

		/// <summary>
		/// Returns the parameter's value's type.
		/// </summary>
		public ParameterValueType ValueType { get; }

		/// <summary>
		/// Raised when the value of the parameter changes.
		/// </summary>
		public event Action<IParameter> ValueChanged;

		/// <summary>
		/// Gets or sets the parameter's value, capped at min and max value.
		/// </summary>
		public TValue Value
		{
			get => _value;
			set
			{
				var valueBefore = _value;

				if (value.CompareTo(this.MinValue) < 0)
					_value = this.MinValue;
				else if (value.CompareTo(this.MaxValue) > 0)
					_value = this.MaxValue;
				else
					_value = value;

				if (!_value.Equals(valueBefore))
					this.ValueChanged?.Invoke(this);
			}
		}

		/// <summary>
		/// Gets or sets the parameters minimum value.
		/// </summary>
		public TValue MinValue
		{
			get => _minValue;
			set
			{
				_minValue = value;

				if (value.CompareTo(_maxValue) > 0)
					_maxValue = value;

				if (_minValue.CompareTo(this.Value) > 0)
					this.Value = _minValue;
			}
		}

		/// <summary>
		/// Gets or sets the parameters maximum value.
		/// </summary>
		public TValue MaxValue
		{
			get => _maxValue;
			set
			{
				if (value.CompareTo(this.MinValue) < 0)
					_maxValue = this.MinValue;
				else
					_maxValue = value;

				if (_maxValue.CompareTo(this.Value) < 0)
					this.Value = _maxValue;
			}
		}

		/// <summary>
		/// Returns the numeric value as a double.
		/// </summary>
		public abstract double ValueDouble { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="valueType"></param>
		/// <param name="value"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		protected NumericParameter(ParameterValueType valueType, TValue value, TValue minValue, TValue maxValue)
		{
			this.ValueType = valueType;
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this.Value = value;
		}
	}

	/// <summary>
	/// Represents a parameter holding a byte value.
	/// </summary>
	public class ByteParameter : NumericParameter<byte>
	{
		/// <summary>
		/// Returns the numeric value as a double.
		/// </summary>
		public override double ValueDouble => this.Value;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public ByteParameter(byte value = default, byte minValue = byte.MinValue, byte maxValue = byte.MaxValue) : base(ParameterValueType.Byte, value, minValue, maxValue) { }
	}

	/// <summary>
	/// Represents a parameter holding a short value.
	/// </summary>
	public class ShortParameter : NumericParameter<short>
	{
		/// <summary>
		/// Returns the numeric value as a double.
		/// </summary>
		public override double ValueDouble => this.Value;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public ShortParameter(short value = default, short minValue = short.MinValue, short maxValue = short.MaxValue) : base(ParameterValueType.Short, value, minValue, maxValue) { }
	}

	/// <summary>
	/// Represents a parameter holding an int value.
	/// </summary>
	public class IntParameter : NumericParameter<int>
	{
		/// <summary>
		/// Returns the numeric value as a double.
		/// </summary>
		public override double ValueDouble => this.Value;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public IntParameter(int value = default, int minValue = int.MinValue, int maxValue = int.MaxValue) : base(ParameterValueType.Int, value, minValue, maxValue) { }

		/// <summary>
		/// Implicitly converts parameter by returning its value.
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public static implicit operator int(IntParameter param)
		{
			return param.Value;
		}
	}

	/// <summary>
	/// Represents a parameter holding a long value.
	/// </summary>
	public class LongParameter : NumericParameter<long>
	{
		/// <summary>
		/// Returns the numeric value as a double.
		/// </summary>
		public override double ValueDouble => this.Value;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public LongParameter(long value = default, long minValue = long.MinValue, long maxValue = long.MaxValue) : base(ParameterValueType.Long, value, minValue, maxValue) { }
	}

	/// <summary>
	/// Represents a parameter holding a float value.
	/// </summary>
	public class FloatParameter : NumericParameter<float>
	{
		/// <summary>
		/// Returns the numeric value as a double.
		/// </summary>
		public override double ValueDouble => this.Value;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public FloatParameter(float value = default, float minValue = float.MinValue, float maxValue = float.MaxValue) : base(ParameterValueType.Float, value, minValue, maxValue) { }

		/// <summary>
		/// Implicitly converts parameter by returning its value.
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public static implicit operator float(FloatParameter param)
		{
			return param.Value;
		}
	}
}
