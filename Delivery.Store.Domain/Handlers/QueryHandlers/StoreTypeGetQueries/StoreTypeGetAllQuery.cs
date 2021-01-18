using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreTypeGetQueries
{
    public class StoreTypeGetAllQuery : IQuery<List<StoreTypeContract>>
    {
        public StoreTypeGetAllQuery(string cacheKey)
        {
            CacheKey = cacheKey;
        }
        
        public string CacheKey { get; }
        
    }
}