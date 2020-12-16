using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Delivery.Azure.Library.Core.Extensions.Json
{
    public class JsonDateTimeConverter : JsonConverter<DateTimeOffset?>
    {
        private readonly string? format;

        public JsonDateTimeConverter(string? format = null)
        {
            this.format = format;
        }

        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!reader.TryGetDateTimeOffset(out var value))
            {
                var input = reader.GetString();
                if (string.IsNullOrEmpty(input))
                {
                    return null;
                }

                value = DateTimeOffset.Parse(input);
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStringValue((string?) null);
            }
            else
            {
                if (string.IsNullOrEmpty(format))
                {
                    writer.WriteStringValue(value.GetValueOrDefault());
                }
                else
                {
                    writer.WriteStringValue(value.GetValueOrDefault().ToString(format));
                }
            }
        }
    }
}