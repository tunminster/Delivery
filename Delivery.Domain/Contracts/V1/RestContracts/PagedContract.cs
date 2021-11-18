using System.Collections.Generic;

namespace Delivery.Domain.Contracts.V1.RestContracts
{
    public abstract record PagedContract<T> where T : class, new()
    {
        public int TotalPages { get; init; }
        
        public List<T> Data { get; init; }
    }
}