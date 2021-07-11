namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record NotificationBaseModel
    {
        /// <summary>
        ///  Authenticated user to be register along with the device registration.
        /// </summary>
        public string Username { get; init; } = string.Empty;
        
        /// <summary>
        ///  Correlation id
        /// </summary>
        public string CorrelationId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Shard key
        /// </summary>
        public string ShardKey { get; init; } = string.Empty;

        /// <summary>
        /// Ring key
        /// </summary>
        public string RingKey { get; init; } = string.Empty;
    }
}