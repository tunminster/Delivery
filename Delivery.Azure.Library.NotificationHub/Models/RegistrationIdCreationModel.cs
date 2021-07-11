namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record RegistrationIdCreationModel : NotificationBaseModel
    {
        /// <summary>
        /// Platform notification service PNS handle
        /// </summary>
        public string Handle { get; init; } = string.Empty;
    }
}