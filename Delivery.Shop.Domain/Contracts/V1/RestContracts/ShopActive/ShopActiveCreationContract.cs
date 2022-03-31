namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopActive
{
    public record ShopActiveCreationContract
    {
        public bool IsActive { get; init; }
        public string ShopUserName { get; init; } = string.Empty;
    }
}