namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record RegistrationDeleteModel
    {
        /// <summary>
        ///  Registration Id
        /// </summary>
        public string RegistrationId { get; init; }
        
        /// <summary>
        ///  Correlation id
        /// </summary>
        public string CorrelationId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Shard key
        /// </summary>
        public string ShardKey { get; init; } = string.Empty;

        /// <summary>
        /// Ring Key
        /// </summary>
        public string RingKey { get; init; } = string.Empty;
    }
}