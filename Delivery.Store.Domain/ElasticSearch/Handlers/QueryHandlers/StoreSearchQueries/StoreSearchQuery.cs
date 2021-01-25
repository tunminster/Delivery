using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.QueryHandlers.StoreSearchQueries
{
    public class StoreSearchQuery : IQuery<List<StoreContract>>
    {
        public StoreSearchQuery(string queryString, int page, int pageSize, string nearestDistance, string storeType, double latitude, double longitude, string distance)
        {
            QueryString = queryString;
            Page = page;
            PageSize = pageSize;
            NearestDistance = nearestDistance;
            StoreType = storeType;
            Latitude = latitude;
            Longitude = longitude;
            Distance = distance;
        }
        
        public string QueryString { get; }
        
        public string NearestDistance { get; }
        
        public string StoreType { get; }
        
        public int Page { get; }
        
        public int PageSize { get; }
        
        public double Latitude { get; }
        
        public double Longitude { get; }
        
        public string Distance { get; }
    }
}