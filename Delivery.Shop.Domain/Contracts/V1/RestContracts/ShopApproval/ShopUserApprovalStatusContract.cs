using System;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval
{
    public record ShopUserApprovalStatusContract(bool Status, DateTimeOffset DateCreated);
}