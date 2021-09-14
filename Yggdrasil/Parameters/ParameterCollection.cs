using System;
using System.Collections.Generic;
using System.Linq;

namespace Yggdrasil.Parameters
{
	/// <summary>
	/// Container for a collection of parameters.
	/// </summary>
	/// <typeparam name="TParameterType">
	/// Key type for the parameters, which they are identified by.
	/// </typeparam>
	public class ParameterCollection<TParameterType>
	{
		private readonly object _syncLock = new object();

		private readonly Dictionary<TParameterType, IParameter> _parameters = new Dictionary<TParameterType, IParameter>();

		/// <summary>
		/// Gets or sets whether parameters are created when set is called
		/// for them, but they don't exist yet.
		/// </summary>
		/// <remarks>
		/// Not creating parameters on demand provides a kind of safety,
		/// as you will have to create the parameters before they can
		/// be used. Creating them when they're set, on the other hand,
		/// allows you to address parameters without having to set them
		/// up first.
		/// </remarks>
		public bool CreateParametersOnSet { get; set; } = false;

		/// <summary>
		/// Gets or sets whether parameters are created when modify is
		/// called for them, but they don't exist yet.
		/// </summary>
		public bool CreateParametersOnModify { get; set; } = false;

		/// <summary>
		/// Adds parameter.
		/// </summary>
		/// <remarks>
		/// Replaces parameter of the same type if it exists already.
		/// </remarks>
		/// <param name="type"></param>
		/// <param name="parameter"></param>
		public void Add(TParameterType type, IParameter parameter)
		{
			lock (_syncLock)
				_parameters[type] = parameter;
		}

		/// <summary>
		/// Removes parameter.
		/// </summary>
		/// <param name="type"></param>
		public void Remove(TParameterType type)
		{
			lock (_syncLock)
				_parameters.Remove(type);
		}

		/// <summary>
		/// Returns true if this instance contains the parameter.
		/// </summary>
		/// <param name="type"></param>
		public void Contains(TParameterType type)
		{
			lock (_syncLock)
				_parameters.ContainsKey(type);
		}

		/// <summary>
		/// Returns the parameter of the given type.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the parameter doesn't exist.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if the parameter doesn't match the requested generic type.
		/// </exception>
		public TParameter Get<TParameter>(TParameterType type)
		{
			if (!this.TryGet<TParameter>(type, out var parameter))
				throw new ArgumentException($"Parameter {type} not found.");

			return parameter;
		}

		/// <summary>
		/// Returns the parameter of the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the parameter doesn't exist.
		/// </exception>
		public IParameter Get(TParameterType type)
		{
			lock (_syncLock)
			{
				if (!_parameters.TryGetValue(type, out var parameter))
					throw new ArgumentException($"Parameter {type} not found.");

				return parameter;
			}
		}

		/// <summary>
		/// Returns the parameter of the given type via out, returns false
		/// if the parameter was not found.
		/// </summary>
		/// <typeparam name="TParameter"></typeparam>
		/// <param name="type"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the parameter doesn't match the requested generic type.
		/// </exception>
		public bool TryGet<TParameter>(TParameterType type, out TParameter parameter)
		{
			parameter = default;

			lock (_syncLock)
			{
				if (!_parameters.TryGetValue(type, out var genericParameter))
					return false;

				if (!(genericParameter is TParameter))
					throw new ArgumentException($"Parameter {type} is not of type {typeof(TParameter).Name}.");

				parameter = (TParameter)genericParameter;
			}

			return true;
		}

		/// <summary>
		/// Returns the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public byte GetByte(TParameterType type)
			=> this.Get<ByteParameter>(type).Value;

		/// <summary>
		/// Returns the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public short GetShort(TParameterType type)
			=> this.Get<ShortParameter>(type).Value;

		/// <summary>
		/// Returns the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public int GetInt(TParameterType type)
			=> this.Get<IntParameter>(type).Value;

		/// <summary>
		/// Returns the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public long GetLong(TParameterType type)
			=> this.Get<LongParameter>(type).Value;

		/// <summary>
		/// Returns the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public float GetFloat(TParameterType type)
			=> this.Get<FloatParameter>(type).Value;

		/// <summary>
		/// Returns the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string GetString(TParameterType type)
			=> this.Get<StringParameter>(type).Value;

		/// <summary>
		/// Returns a list of all parameters.
		/// </summary>
		/// <returns></returns>
		public List<IParameter> GetList()
		{
			lock (_syncLock)
				return _parameters.Values.ToList();
		}

		/// <summary>
		/// Sets the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public byte SetByte(TParameterType type, byte value)
		{
			if (!this.TryGet<ByteParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnSet)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new ByteParameter());
			}

			return (parameter.Value = value);
		}

		/// <summary>
		/// Sets the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public short SetShort(TParameterType type, short value)
		{
			if (!this.TryGet<ShortParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnSet)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new ShortParameter());
			}

			return (parameter.Value = value);
		}

		/// <summary>
		/// Sets the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetInt(TParameterType type, int value)
		{
			if (!this.TryGet<IntParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnSet)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new IntParameter());
			}

			return (parameter.Value = value);
		}

		/// <summary>
		/// Sets the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public long SetLong(TParameterType type, long value)
		{
			if (!this.TryGet<LongParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnSet)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new LongParameter());
			}

			return (parameter.Value = value);
		}

		/// <summary>
		/// Sets the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public float SetFloat(TParameterType type, float value)
		{
			if (!this.TryGet<FloatParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnSet)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new FloatParameter());
			}

			return (parameter.Value = value);
		}

		/// <summary>
		/// Sets the parameter's value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public string SetString(TParameterType type, string value)
		{
			if (!this.TryGet<StringParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnSet)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new StringParameter());
			}

			return (parameter.Value = value);
		}

		/// <summary>
		/// Modifies the parameter's value and returns its new value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="modifier"></param>
		/// <returns></returns>
		public int ModifyInt(TParameterType type, int modifier)
		{
			if (!this.TryGet<IntParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnModify)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new IntParameter());
			}

			parameter.Value += modifier;
			return parameter.Value;
		}

		/// <summary>
		/// Modifies the parameter's value and returns its new value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="modifier"></param>
		/// <returns></returns>
		public float ModifyFloat(TParameterType type, float modifier)
		{
			if (!this.TryGet<FloatParameter>(type, out var parameter))
			{
				if (!this.CreateParametersOnModify)
					throw new ArgumentException($"Parameter {type} not found.");

				this.Add(type, parameter = new FloatParameter());
			}

			parameter.Value += modifier;
			return parameter.Value;
		}

		/// <summary>
		/// Sums up the values of the given numeric paramters and returns
		/// the result.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public float Sum(params TParameterType[] types)
		{
			var result = 0d;

			foreach (var type in types)
			{
				var parameter = this.Get(type);
				if (!(parameter is INumericParameter numParameter))
					throw new ArgumentException($"Parameter {type} can't be summed up, as it's not a numeric type.");

				result += numParameter.ValueDouble;
			}

			return (float)result;
		}
	}
}
