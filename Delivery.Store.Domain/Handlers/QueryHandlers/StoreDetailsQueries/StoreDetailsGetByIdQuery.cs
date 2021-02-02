using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreDetailsQueries
{
    public class StoreDetailsGetByIdQuery : IQuery<StoreDetailsContract>
    {
        public StoreDetailsGetByIdQuery(string storeId, string cacheKey)
        {
            StoreId = storeId;
            CacheKey = cacheKey;
        }
        public string StoreId { get; }
        
        public string CacheKey { get; }
    }
}