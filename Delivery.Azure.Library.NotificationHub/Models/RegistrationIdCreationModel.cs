namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record RegistrationIdCreationModel
    {
        /// <summary>
        /// Platform notification service PNS handle
        /// </summary>
        public string Handle { get; init; } = string.Empty;

        public string CorrelationId { get; init; } = string.Empty;
        
        public string ShardKey { get; init; } = string.Empty;

        public string RingKey { get; init; } = string.Empty;
    }
}