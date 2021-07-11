namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record RegistrationDeleteModel : NotificationBaseModel
    {
        /// <summary>
        ///  Registration Id
        /// </summary>
        public string RegistrationId { get; init; }
        
    }
}