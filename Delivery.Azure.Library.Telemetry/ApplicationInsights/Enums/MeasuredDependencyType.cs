using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums
{
    [DataContract]
    public enum MeasuredDependencyType
    {
        [EnumMember] None,
        [EnumMember] Availability,
        [EnumMember] AzureBlob,
        [EnumMember] AzureCosmosDb,
        [EnumMember] AzureEventHub,
        [EnumMember] AzureQueue,
        [EnumMember] AzureServiceBus,
        [EnumMember] AzureNotificationHub,
        [EnumMember] AzureStorage,
        [EnumMember] AzureTable,
        [EnumMember] Sql,
        [EnumMember] Http,
        [EnumMember] WebService,
        [EnumMember] Redis,
        [EnumMember] ElasticSearch,
        [EnumMember] Other
    }
}