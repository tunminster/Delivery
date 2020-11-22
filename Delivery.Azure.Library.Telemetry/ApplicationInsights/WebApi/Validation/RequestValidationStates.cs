using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Validation
{
    [DataContract]
    public enum RequestValidationStates
    {
        [EnumMember] NotSupported,
        [EnumMember] SerializationFailure,
        [EnumMember] ValidationFailure
    }
}