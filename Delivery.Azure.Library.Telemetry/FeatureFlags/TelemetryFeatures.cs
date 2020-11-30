using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Telemetry.FeatureFlags
{
    [DataContract]
    public enum TelemetryFeatures
    {
        [EnumMember] None,
        [EnumMember] DatabaseDependencyTracking,
        [EnumMember] MeasurePlatformPerformance,
        [EnumMember] VerboseCircuitBreakerTelemetry
    }
}