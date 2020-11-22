using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Exceptions;
using Delivery.Azure.Library.Core.Extensions.Strings;

namespace Delivery.Azure.Library.Core.Extensions.Json
{
    /// <summary>
	///     Provides a standardized way work with and create json objects
	/// </summary>
	public static class JsonExtensions
	{
		/// <summary>
		///     The default json serialization settings which will be used if not supplied as an argument to ConvertToJson or
		///     ConvertFromJson
		/// </summary>
		public static JsonSerializerOptions GetDefaultJsonSerializerOptions() => new JsonSerializerOptions
		{
			WriteIndented = false,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Disallow,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			Converters = {new JsonStringEnumConverter(), new JsonDateTimeConverter()}
		};

		/// <summary>
		///     The default json deserialization settings which will be used if not supplied as an argument to ConvertToJson or
		///     ConvertFromJson
		/// </summary>
		public static JsonSerializerOptions GetDefaultJsonDeserializerOptions() => new JsonSerializerOptions
		{
			WriteIndented = false,
			ReferenceHandler = ReferenceHandler.Preserve,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Disallow,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			Converters = {new JsonStringEnumConverter(), new JsonDateTimeConverter()}
		};

		/// <summary>
		///     Serializes an object to a raw JSON string
		/// </summary>
		/// <param name="originalObject"></param>
		/// <param name="jsonSerializerOptions">Serialization options to use</param>
		public static byte[] ConvertToJsonBytes(this object? originalObject, JsonSerializerOptions? jsonSerializerOptions = null)
		{
			try
			{
				if (originalObject == null)
				{
					return Array.Empty<byte>();
				}

				var rawJsonBytes = JsonSerializer.SerializeToUtf8Bytes(originalObject, jsonSerializerOptions ?? GetDefaultJsonSerializerOptions());
				return rawJsonBytes;
			}
			catch (Exception exception)
			{
				var message = $"Cannot serialize object to string: {originalObject}";
				throw new JsonException(message, exception);
			}
		}

		/// <summary>
		///     Serializes an object to a raw JSON string
		/// </summary>
		/// <param name="originalObject"></param>
		/// <param name="jsonSerializerOptions">Serialization options to use</param>
		public static string ConvertToJson(this object? originalObject, JsonSerializerOptions? jsonSerializerOptions = null)
		{
			try
			{
				if (originalObject == null)
				{
					return string.Empty;
				}

				var rawJsonString = JsonSerializer.Serialize(originalObject, jsonSerializerOptions ?? GetDefaultJsonSerializerOptions());
				return rawJsonString;
			}
			catch (Exception exception)
			{
				var message = $"Cannot serialize object to string: {originalObject}";
				throw new JsonException(message, exception);
			}
		}

		/// <summary>
		///     Deserializes a raw JSON string to a typed object
		/// </summary>
		/// <typeparam name="T">Expected type of the object</typeparam>
		/// <param name="stream">Raw JSON stream</param>
		/// <param name="jsonSerializerOptions">Serialization options to use</param>
		public static async Task<T> ConvertFromJsonAsync<T>(this Stream stream, JsonSerializerOptions? jsonSerializerOptions = null)
		{
			try
			{
				var deserialized = await JsonSerializer.DeserializeAsync<T>(stream, jsonSerializerOptions ?? GetDefaultJsonDeserializerOptions());
				return deserialized!;
			}
			catch (Exception exception)
			{
				var message = $"Cannot deserialize json into type {typeof(T).Name}: {stream}";
				throw new JsonException(message, exception);
			}
		}

		/// <summary>
		///     Deserializes a raw JSON string to a typed object
		/// </summary>
		/// <typeparam name="T">Expected type of the object</typeparam>
		/// <param name="rawJsonString">Raw JSON string</param>
		/// <param name="jsonSerializerOptions">Serialization options to use</param>
		public static T ConvertFromJson<T>(this string rawJsonString, JsonSerializerOptions? jsonSerializerOptions = null)
		{
			try
			{
				if (string.IsNullOrEmpty(rawJsonString))
				{
					if (typeof(T).GetInterfaces().Any(type => type.IsAssignableFrom(typeof(IEnumerable))))
					{
						rawJsonString = "[]";
					}
					else
					{
						rawJsonString = "{}";
					}
				}

				var deserialized = JsonSerializer.Deserialize<T>(rawJsonString, jsonSerializerOptions ?? GetDefaultJsonDeserializerOptions());
				return deserialized!;
			}
			catch (Exception exception)
			{
				var message = $"Cannot deserialize json into type {typeof(T).Name}: {rawJsonString}";
				throw new JsonException(message, exception);
			}
		}

		/// <summary>
		///     Creates a Deep Clone of any serializable type
		/// </summary>
		/// <param name="originalObject"></param>
		/// <param name="jsonSerializerOptions">Serialization settings to use</param>
		public static T DeepClone<T>(this object? originalObject, JsonSerializerOptions? jsonSerializerOptions = null)
		{
			// use deserialization settings for deep clone as it will be converted back immediately
			var serialisedObject = originalObject.ConvertToJson(jsonSerializerOptions ?? GetDefaultJsonDeserializerOptions());

			var deepClone = ConvertFromJson<T>(serialisedObject, jsonSerializerOptions ?? GetDefaultJsonDeserializerOptions());

			return deepClone;
		}

		/// <summary>
		///     Checks that the json payload supplied does not have additional properties set on it
		/// </summary>
		/// <param name="jsonElement">The node to validate</param>
		/// <param name="targetObject">The serialized object which the node should match</param>
		/// <param name="cycleCount">An incrementing count to prevent infinite recursion</param>
		/// <typeparam name="T">The type which the node represents</typeparam>
		/// <exception cref="ValidationException">Thrown on any schema validation failure</exception>
		/// <remarks>
		///     Designed primarily to prevent api consumers from sending incorrectly-structured payloads. Use validation to
		///     handle cases where properties which were expected to be set have not been set
		/// </remarks>
		public static void ValidateSchema<T>(this JsonElement jsonElement, T targetObject, int cycleCount = 0)
			where T : class
		{
			// failsafe to prevent an infinite loop for large objects
			if (cycleCount > 10000)
			{
				return;
			}

			cycleCount++;

			if (jsonElement.ValueKind == JsonValueKind.Array)
			{
				ValidateArrayItems(jsonElement, targetObject, cycleCount);
			}

			if (jsonElement.ValueKind != JsonValueKind.Object)
			{
				return;
			}

			if (targetObject is IDictionary)
			{
				return;
			}

			foreach (var jsonObject in jsonElement.EnumerateObject())
			{
				var pascalCasePropertyName = jsonObject.Name.ToPascalCase();
				var matchingProperty = targetObject.GetType().GetProperty(pascalCasePropertyName);
				if (matchingProperty == null)
				{
					throw new ValidationException($"Property with name '{jsonObject.Name}' does not exist on the expected object type ({targetObject.GetType().Name}). Check that the payload schema matches what the endpoint is expecting exactly.");
				}

				var matchingObject = matchingProperty.GetValue(targetObject);
				var jsonObjectValue = jsonObject.Value;

				ValidateElement(cycleCount, jsonObjectValue, matchingObject, jsonObject);
			}
		}

		private static void ValidateElement(int cycleCount, JsonElement jsonObjectValue, object? matchingObject, JsonProperty jsonObject)
		{
			switch (jsonObjectValue.ValueKind)
			{
				case JsonValueKind.Object:
					if (matchingObject != null)
					{
						ValidateSchema(jsonObjectValue, matchingObject, cycleCount);
					}

					break;
				case JsonValueKind.Array:
					if (!(matchingObject is IEnumerable))
					{
						throw new ValidationException($"Json property '{jsonObject.Name}' has been sent as an array ({jsonObjectValue}), but the expected type is not an array. Check that the schema matches the input sent.");
					}

					ValidateSchema(jsonObjectValue, matchingObject, cycleCount);

					break;
				case JsonValueKind.String:
				case JsonValueKind.Number:
				case JsonValueKind.True:
				case JsonValueKind.False:
				case JsonValueKind.Null:
					break;
				case JsonValueKind.Undefined:
					throw new ValidationSerializationException($"{nameof(JsonValueKind.Undefined)} is not a supported type");
				default:
					throw new NotSupportedException($"{jsonObjectValue.ValueKind} has not been implemented");
			}
		}

		private static void ValidateArrayItems<T>(JsonElement jsonElement, T targetObject, int cycleCount) where T : class
		{
			var arrayEnumerator = jsonElement.EnumerateArray();
			var matchingArrayEnumerator = ((IEnumerable) targetObject).GetEnumerator();

			for (var i = 0; i < arrayEnumerator.Count(); i++)
			{
				arrayEnumerator.MoveNext();
				matchingArrayEnumerator.MoveNext();

				var jsonArrayItem = arrayEnumerator.Current;
				var matchingArrayItem = matchingArrayEnumerator.Current;

				if (matchingArrayItem != null)
				{
					ValidateSchema(jsonArrayItem, matchingArrayItem, cycleCount);
				}
			}
		}
	}
}