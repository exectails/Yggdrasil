using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yggdrasil.Network.Communication
{
	/// <summary>
	/// A converter for interfaces.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class InterfaceConverter<T> : JsonConverter<T> where T : class
	{
		private const string TypePropertyName = "Type";
		private const string DataPropertyName = "Data";

		/// <summary>
		/// Writes the value to the JSON writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="options"></param>
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString(TypePropertyName, value.GetType().AssemblyQualifiedName);
			writer.WritePropertyName(DataPropertyName);
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
			writer.WriteEndObject();
		}

		/// <summary>
		/// Reads a value from the JSON reader.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="typeToConvert"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="JsonException"></exception>
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException();

			reader.Read(); // Object start

			if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != TypePropertyName)
				throw new JsonException();

			reader.Read(); // Property name

			var typeName = reader.GetString();
			var type = Type.GetType(typeName) ?? throw new JsonException($"Unable to find type: {typeName}");

			reader.Read(); // Property name

			if (reader.GetString() != DataPropertyName)
				throw new JsonException();

			reader.Read(); // Object start

			var result = JsonSerializer.Deserialize(ref reader, type, options) as T;

			reader.Read(); // Object end

			return result;
		}

		/// <summary>
		/// Determines whether the specified type can be converted.
		/// </summary>
		/// <param name="typeToConvert"></param>
		/// <returns></returns>
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(T).IsAssignableFrom(typeToConvert);
		}
	}
}
