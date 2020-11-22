using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts
{
    /// <summary>
    ///     Describes a single error of a BadRequest (400) response that can be fixed by the api consumer.
    /// </summary>
    [DataContract]
    public class BadRequestErrorContract
    {
        /// <summary>
        ///     Details what went wrong with the request and how to fix it.
        /// </summary>
        [DataMember]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        ///     Returns a type to differentiate errors such as SerializationFailure or ValidationFailure.
        /// </summary>
        [DataMember]
        public string Type { get; set; } = "Unknown";

        public override string ToString()
        {
            return $"{nameof(BadRequestErrorContract)} - {nameof(Type)}: {Type}, {nameof(Message)}: {Message}";
        }
    }
}