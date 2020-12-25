using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace Delivery.Azure.Library.Storage.Cosmos.Serializers
{
    /// <summary>
    ///   The default Cosmos json.net serializer
    /// </summary>
    internal sealed class CosmosTextJsonSerializer : CosmosSerializer
    {
        private static ReadOnlySpan<byte> Utf8Bom => new byte[] {0xEF, 0xBB, 0xBF};
        private const int UnSeekableStreamInitialRentSize = 4096;
        private readonly JsonSerializerOptions jsonSerializerOptions;
        
        internal CosmosTextJsonSerializer(JsonSerializerOptions jsonSerializerSettings)
        {
            jsonSerializerOptions = jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings));
        }
        
        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (stream.CanSeek
                    && stream.Length == 0)
                {
                    return default!;
                }

                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T) (object) stream;
                }

                return Deserialize<T>(stream, jsonSerializerOptions);
            }
        }

        public override Stream ToStream<T>(T input)
        {
	        MemoryStream streamPayload = new();
	        using Utf8JsonWriter utf8JsonWriter = new(streamPayload, new JsonWriterOptions {Indented = jsonSerializerOptions.WriteIndented});
	        JsonSerializer.Serialize(utf8JsonWriter, input, jsonSerializerOptions);
	        streamPayload.Position = 0;
	        return streamPayload;
        }
        
        /// <summary>
        ///   https://github.com/dotnet/runtime/blob/master/src/libraries/System.Text.Json/src/System/Text/Json/Document/JsonDocument.Parse.cs#L577.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T Deserialize<T>(
			Stream stream,
			JsonSerializerOptions jsonSerializerOptions)
		{
			if (stream is MemoryStream memoryStream
			    && memoryStream.TryGetBuffer(out var buffer))
			{
				if (buffer.Count >= Utf8Bom.Length
				    && Utf8Bom.SequenceEqual(buffer.AsSpan(start: 0, Utf8Bom.Length)))
				{
					// Skip 3 BOM bytes
					return JsonSerializer.Deserialize<T>(buffer.AsSpan(Utf8Bom.Length), jsonSerializerOptions)!;
				}

				return JsonSerializer.Deserialize<T>(buffer, jsonSerializerOptions)!;
			}

			var written = 0;
			byte[]? rented = null;

			var utf8Bom = Utf8Bom;

			try
			{
				if (stream.CanSeek)
				{
					// Ask for 1 more than the length to avoid resizing later,
					// which is unnecessary in the common case where the stream length doesn't change.
					var expectedLength = Math.Max(utf8Bom.Length, stream.Length - stream.Position) + 1;
					rented = ArrayPool<byte>.Shared.Rent(checked((int) expectedLength));
				}
				else
				{
					rented = ArrayPool<byte>.Shared.Rent(UnSeekableStreamInitialRentSize);
				}

				int lastRead;

				// Read up to 3 bytes to see if it's the UTF-8 BOM
				do
				{
					lastRead = stream.Read(
						rented,
						written,
						utf8Bom.Length - written);

					written += lastRead;
				} while (lastRead > 0 && written < utf8Bom.Length);

				// If we have 3 bytes, and they're the BOM, reset the write position to 0.
				if (written == utf8Bom.Length &&
				    utf8Bom.SequenceEqual(rented.AsSpan(start: 0, utf8Bom.Length)))
				{
					written = 0;
				}

				do
				{
					if (rented.Length == written)
					{
						byte[] toReturn = rented;
						rented = ArrayPool<byte>.Shared.Rent(checked(toReturn.Length * 2));
						Buffer.BlockCopy(toReturn, srcOffset: 0, rented, dstOffset: 0, toReturn.Length);
						// Holds document content, clear it.
						ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
					}

					lastRead = stream.Read(rented, written, rented.Length - written);
					written += lastRead;
				} while (lastRead > 0);

				return JsonSerializer.Deserialize<T>(rented.AsSpan(start: 0, written), jsonSerializerOptions)!;
			}
			finally
			{
				if (rented != null)
				{
					// Holds document content, clear it before returning it.
					rented.AsSpan(start: 0, written).Clear();
					ArrayPool<byte>.Shared.Return(rented);
				}
			}
		}
    }
}