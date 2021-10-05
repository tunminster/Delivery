namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    public record ShopOrderDeliveryAddress
    {
        public string AddressLine1 { get; init; } = string.Empty;
        
        public string AddressLine2 { get; init; } = string.Empty;
        
        public string City { get; init; } = string.Empty;
        
        public string PostalCode { get; init; } = string.Empty;
        
        public double? Latitude { get; init; }
        
        public double? Longitude { get; init; }
    }
}