using System;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval
{
    public record ShopApprovalStatusContract(bool Status, DateTimeOffset DateCreated);
}