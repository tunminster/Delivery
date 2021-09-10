using System;

namespace Delivery.Domain.Contracts.V1.RestContracts
{
    public record StatusContract
    {
        public bool Status { get; init; }
        public DateTimeOffset DateCreated { get; init; }
    }
}