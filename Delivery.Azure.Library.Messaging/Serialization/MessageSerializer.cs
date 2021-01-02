using System;
using System.Text;
using System.Text.Json;
using Delivery.Azure.Library.Core.Extensions.Json;
using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Messaging.Serialization
{
    public static class MessageSerializer
    {
        /// <summary>
        ///     Deserializes the message body
        /// </summary>
        /// <typeparam name="TMessage">The message body entity type to return</typeparam>
        /// <param name="message">The message body to deserialize</param>
        /// <returns>The deserialized body</returns>
        public static TMessage Deserialize<TMessage>(this Message message)
            where TMessage : class
        {
            using var jsonDocument = JsonDocument.Parse(message.Body);
            var contentType = jsonDocument.RootElement.GetProperty("datacontenttype").GetString();
            if (contentType?.ToLowerInvariant() != "application/json")
            {
                throw new NotSupportedException($"Only cloud events in json format are currently supported. Found: {Encoding.UTF8.GetString(message.Body)}");
            }

            var data = jsonDocument.RootElement.GetProperty("data");
            var payload = data.GetRawText().ConvertFromJson<TMessage>();
            return payload;
        }
    }
}