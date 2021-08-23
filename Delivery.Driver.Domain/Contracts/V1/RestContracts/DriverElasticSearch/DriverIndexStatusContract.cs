using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverElasticSearch
{
    public record DriverIndexStatusContract
    {
        public bool Status { get; init; }
        
        public DateTimeOffset DateCreated { get; init; }
    }
}