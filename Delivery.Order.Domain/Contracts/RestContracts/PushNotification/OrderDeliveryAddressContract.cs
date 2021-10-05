namespace Delivery.Order.Domain.Contracts.RestContracts.PushNotification
{
    public record OrderDeliveryAddressContract
    {
        public string AddressLine1 { get; init; } = string.Empty;
        
        public string AddressLine2 { get; init; } = string.Empty;
        
        public string City { get; init; } = string.Empty;
        
        public string PostalCode { get; init; } = string.Empty;
        
        public double? Latitude { get; init; }
        
        public double? Longitude { get; init; }
    }
}