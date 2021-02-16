using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.QueryHandlers.StoreSearchQueries
{
    public class StoreSearchQueryByStoreTypeQuery : IQuery<List<StoreContract>>
    {
        public StoreSearchQueryByStoreTypeQuery( string storeType, int page, int pageSize,double latitude, double longitude, string distance)
        {
            Page = page;
            PageSize = pageSize;
            StoreType = storeType;
            Latitude = latitude;
            Longitude = longitude;
            Distance = distance;
        }
        
        public string StoreType { get; }
        
        public int Page { get; }
        
        public int PageSize { get; }
        
        public double Latitude { get; }
        
        public double Longitude { get; }
        
        public string Distance { get; }
    }
}