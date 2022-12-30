using System;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;

namespace Delivery.Azure.Library.Contracts.Contracts
{
    public record AnonymousExecutingRequestContext : IVersionedContract
    {
        public static AnonymousExecutingRequestContext Anonymous => new();

        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
        
        public int? Ring { get; set; }
        
        public int Version => 1;
    }
}