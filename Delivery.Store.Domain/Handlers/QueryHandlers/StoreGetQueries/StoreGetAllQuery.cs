using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries
{
    public class StoreGetAllQuery : IQuery<List<StoreContract>>
    {
        public StoreGetAllQuery(string cacheKey, int numberOfObjectPerPage, int pageNumber)
        {
            CacheKey = cacheKey;
            NumberOfObjectPerPage = numberOfObjectPerPage;
            PageNumber = pageNumber;
        }
        
        public string CacheKey { get; }
        public int NumberOfObjectPerPage { get;  }
        
        public int PageNumber { get;  }

    }
}