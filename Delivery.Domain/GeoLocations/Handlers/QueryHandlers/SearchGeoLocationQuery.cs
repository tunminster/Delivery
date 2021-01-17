using Delivery.Domain.GeoLocations.Contracts.V1.RestContracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Domain.GeoLocations.Handlers.QueryHandlers
{
    public class SearchGeoLocationQuery : IQuery<SearchGeoLocationStatusContract>
    {
        public SearchGeoLocationQuery(SearchGeoLocationContract searchGeoLocationContract)
        {
            SearchGeoLocationContract = searchGeoLocationContract;
        }
        public SearchGeoLocationContract SearchGeoLocationContract { get;  }
    }
}