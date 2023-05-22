using System;
using System.Collections.Generic;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// Describes a numeric variable with a value that can be
		/// represented as a double.
		/// </summary>
		public interface INumericVariable : IVariable
		{
			/// <summary>
			/// Returns the variable's value as a double.
			/// </summary>
			double NumberValue { get; }
		}

		/// <summary>
		/// Base class for variables with a numeric values.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		public abstract class NumericVariable<TValue> : IVariable<TValue>, INumericVariable where TValue : IComparable<TValue>, IEquatable<TValue>, IFormattable
		{
			private TValue _value;
			private TValue _minValue;
			private TValue _maxValue;

			/// <summary>
			/// Returns the variable's underlying type.
			/// </summary>
			public abstract VariableType Type { get; }

			/// <summary>
			/// Returns the variable's identifier.
			/// </summary>
			public TIdent Ident { get; }

			/// <summary>
			/// Raised when the variable's value changed.
			/// </summary>
			public event Action<IVariable> ValueChanged;

			/// <summary>
			/// Gets or sets the variable's value.
			/// </summary>
			public virtual TValue Value
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
			/// Gets or sets the variable's minimum value.
			/// </summary>
			public virtual TValue MinValue
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
			/// Gets or sets the variable's maximum value.
			/// </summary>
			public virtual TValue MaxValue
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
			/// Returns the variable's value as a double.
			/// </summary>
			public double NumberValue => Convert.ToDouble(this.Value);

			/// <summary>
			/// Initializes numeric variable.
			/// </summary>
			/// <param name="name"></param>
			/// <param name="value"></param>
			/// <param name="minValue"></param>
			/// <param name="maxValue"></param>
			public NumericVariable(TIdent name, TValue value, TValue minValue, TValue maxValue)
			{
				this.Ident = name;

				_value = value;
				_minValue = minValue;
				_maxValue = maxValue;

				if (_maxValue.CompareTo(_minValue) < 0)
					_maxValue = _minValue;
			}

			/// <summary>
			/// Serializes the variable's value and returns it.
			/// </summary>
			/// <returns></returns>
			public abstract string Serialize();

			/// <summary>
			/// Reads the serialized value and sets it as the variable's
			/// value.
			/// </summary>
			/// <param name="value"></param>
			public abstract void Deserialize(string value);

			/// <summary>
			/// Returns whether the given value is equal to the variable's
			/// value.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator ==(NumericVariable<TValue> a, TValue b) => (a._value.CompareTo(b) == 0);

			/// <summary>
			/// Returns whether the given value is not equal to the
			/// variable's value.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator !=(NumericVariable<TValue> a, TValue b) => (a._value.CompareTo(b) != 0);

			/// <summary>
			/// Returns whether the given value is lower than or equal to
			/// the variable's value.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator <=(NumericVariable<TValue> a, TValue b) => (a._value.CompareTo(b) <= 0);

			/// <summary>
			/// Returns whether the given value is greater than or equal to
			/// the variable's value.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator >=(NumericVariable<TValue> a, TValue b) => (a._value.CompareTo(b) >= 0);

			/// <summary>
			/// Returns whether the given value is lower than the variable's
			/// value.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator <(NumericVariable<TValue> a, TValue b) => (a._value.CompareTo(b) < 0);

			/// <summary>
			/// Returns whether the given value is greater than the
			/// variable's value.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator >(NumericVariable<TValue> a, TValue b) => (a._value.CompareTo(b) > 0);

			/// <summary>
			/// Implicitly converts the variables's value to its base-type.
			/// </summary>
			/// <param name="a"></param>
			public static implicit operator TValue(NumericVariable<TValue> a) => a._value;

			/// <summary>
			/// Returns true if the given object is a numeric variable
			/// and its value is equal to this variable's value.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				if (!(obj is NumericVariable<TValue> variable))
					return false;

				return EqualityComparer<TValue>.Default.Equals(this.Value, variable.Value);
			}

			/// <summary>
			/// Returns the hash code of the variable's value.
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				return -1937169414 + EqualityComparer<TValue>.Default.GetHashCode(this.Value);
			}

			/// <summary>
			/// Returns a string representation of the variable's value.
			/// </summary>
			/// <returns></returns>
			public override string ToString()
				=> this.Value.ToString();

			/// <summary>
			/// Returns a formatted string representation of the variable's
			/// value.
			/// </summary>
			/// <param name="format"></param>
			/// <param name="formatProvider"></param>
			/// <returns></returns>
			public virtual string ToString(string format, IFormatProvider formatProvider)
				=> this.Value.ToString(format, formatProvider);
		}

		/// <summary>
		/// Returns the variable with the given identifier.
		/// </summary>
		/// <remarks>
		/// Unlike other variable getters, this method doesn't create
		/// variables automatically and only returns available ones.
		/// </remarks>
		/// <param name="ident"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the variable with the given identifier is of a
		/// different type.
		/// </exception>
		public INumericVariable Number(TIdent ident)
		{
			if (!this.TryGet<INumericVariable>(ident, out var variable))
				return null;

			return variable;
		}
	}
}
