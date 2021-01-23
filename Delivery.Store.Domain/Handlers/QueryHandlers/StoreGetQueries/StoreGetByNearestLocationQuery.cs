using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries
{
    public class StoreGetByNearestLocationQuery : IQuery<List<StoreContract>>
    {
        public StoreGetByNearestLocationQuery(string cacheKey, int numberOfObjectPerPage, int pageNumber, double latitude, double longitude, int distance)
        {
            CacheKey = cacheKey;
            NumberOfObjectPerPage = numberOfObjectPerPage;
            PageNumber = pageNumber;
            Latitude = latitude;
            Longitude = longitude;
            Distance = distance;
        }
        
        public string CacheKey { get; }
        
        public double Latitude { get; }
        
        public double Longitude { get;  } 
        public int NumberOfObjectPerPage { get;  }
        
        public int PageNumber { get;  }
        
        public int Distance { get;  }
    }
}