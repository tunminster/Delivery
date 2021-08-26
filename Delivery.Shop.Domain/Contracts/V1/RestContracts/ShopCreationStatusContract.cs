using System;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts
{
    public record ShopCreationStatusContract
    {
        public string StoreId { get; init; } = string.Empty;

        public string ImageUri { get; init; } = string.Empty;
        public DateTimeOffset DateCreated { get; init; }
    }
}