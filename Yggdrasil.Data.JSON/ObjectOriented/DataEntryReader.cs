using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// A type of deserializer that reads data from a JObject and assigns it to
	/// an object.
	/// </summary>
	/// <typeparam name="TObject"></typeparam>
	public class DataEntryReader<TObject> where TObject : class, new()
	{
		private delegate void ReaderEntryFunc(JObject jObj, TObject dataObj);
		private ReaderEntryFunc _reader;

		/// <summary>
		/// Reads the data from the JObject and assigns it to the given object.
		/// </summary>
		/// <param name="jObj"></param>
		/// <param name="dataObj"></param>
		public void Read(JObject jObj, TObject dataObj)
		{
			var reader = this.GetReader();
			reader(jObj, dataObj);
		}

		/// <summary>
		/// Prepares reader to read data.
		/// </summary>
		/// <remarks>
		/// The reader gets prepared on-demand, so this method is optional,
		/// but can be used to compile the reader in advance.
		/// </remarks>
		/// <exception cref="InvalidOperationException"></exception>
		public void Prepare()
		{
			if (_reader != null)
				throw new InvalidOperationException("Reader is already prepared.");

			_reader = CreateReader<TObject>();
		}

		/// <summary>
		/// Returns the reader function, creating it on-demand.
		/// </summary>
		/// <returns></returns>
		private ReaderEntryFunc GetReader()
		{
			if (_reader == null)
				_reader = CreateReader<TObject>();

			return _reader;
		}

		/// <summary>
		/// Creates a reader function for the given object type.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="UnsupportedTypeException"></exception>
		private static ReaderEntryFunc CreateReader<TClass>()
		{
			var entryType = typeof(JObject);
			var dataType = typeof(TClass);
			var properties = dataType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(a => a.CanWrite).ToList();

			var entryParam = Expression.Parameter(entryType, "entry");
			var dataParam = Expression.Parameter(dataType, "data");

			var readerExtType = typeof(JsonExtensions);
			var extMethods = readerExtType.GetMethods(BindingFlags.Public | BindingFlags.Static);

			var expressions = properties.Select(property =>
			{
				var jsonFieldName = property.GetCustomAttribute<DataFieldAttribute>()?.Name;

				if (jsonFieldName == null)
				{
					jsonFieldName = property.Name;
					jsonFieldName = char.ToLower(jsonFieldName[0]) + jsonFieldName.Substring(1);
				}

				MethodInfo readMethod;

				if (property.PropertyType == typeof(bool))
				{
					readMethod = extMethods.First(a => a.Name == "ReadBool" && a.GetParameters().Length == 3);
				}
				else if (property.PropertyType == typeof(int))
				{
					readMethod = extMethods.First(a => a.Name == "ReadInt" && a.GetParameters().Length == 3);
				}
				else if (property.PropertyType == typeof(float))
				{
					readMethod = extMethods.First(a => a.Name == "ReadFloat" && a.GetParameters().Length == 3);
				}
				else if (property.PropertyType == typeof(string))
				{
					readMethod = extMethods.First(a => a.Name == "ReadString" && a.GetParameters().Length == 3);
				}
				else if (property.PropertyType.IsEnum)
				{
					readMethod = extMethods.First(a => a.Name == "ReadEnum" && a.GetParameters().Length == 3).MakeGenericMethod(property.PropertyType);
				}
				else
				{
					throw new UnsupportedTypeException(property.PropertyType);
				}

				var callReadMethod = Expression.Call(readMethod, entryParam, Expression.Constant(jsonFieldName), Expression.Property(dataParam, property));
				var assignExpression = Expression.Assign(Expression.Property(dataParam, property), callReadMethod);

				return (Expression)assignExpression;
			});

			var block = Expression.Block(expressions);
			var lambda = Expression.Lambda<ReaderEntryFunc>(block, entryParam, dataParam);

			return lambda.Compile();
		}
	}

	/// <summary>
	/// Marks a property as a data field with certain properties.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DataFieldAttribute : Attribute
	{
		/// <summary>
		/// Returns the name of the field in the data.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="name"></param>
		public DataFieldAttribute(string name)
		{
			this.Name = name;
		}
	}

	/// <summary>
	/// Exception thrown when a type is not supported by a reader.
	/// </summary>
	public class UnsupportedTypeException : Exception
	{
		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="type"></param>
		public UnsupportedTypeException(Type type) : base("Unsupported type " + type.Name)
		{
		}
	}
}
