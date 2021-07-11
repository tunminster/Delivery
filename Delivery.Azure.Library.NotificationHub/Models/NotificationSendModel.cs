namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record NotificationSendModel : NotificationBaseModel
    {
        public string Pns { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public string ToTag { get; init; } = string.Empty;
        
    }
}