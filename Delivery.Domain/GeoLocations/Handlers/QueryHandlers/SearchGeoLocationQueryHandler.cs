using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Domain.GeoLocations.Contracts.ModelContract;
using Delivery.Domain.GeoLocations.Contracts.V1.RestContracts;
using Delivery.Domain.QueryHandlers;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Delivery.Domain.GeoLocations.Handlers.QueryHandlers
{
    public class SearchGeoLocationQueryHandler : IQueryHandler<SearchGeoLocationQuery, SearchGeoLocationStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public SearchGeoLocationQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<SearchGeoLocationStatusContract> Handle(SearchGeoLocationQuery query)
        {
            var geoApiUri = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("Geo-Api-Uri");
            var geoApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync("Geo-Api-Key");

            string geoUrl = $"{geoApiUri}?address={query.SearchGeoLocationContract.Address}&key={geoApiKey}";

            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
            
            var dependencyData = new DependencyData(nameof(SearchGeoLocationQueryHandler), query.SearchGeoLocationContract.Address);
            var dependencyTarget = geoApiUri;
            var correlationId = executingRequestContextAdapter.GetCorrelationId();

            var geoHttpResponseMessage = await Policy.HandleResult<HttpResponseMessage>(x => x.StatusCode != HttpStatusCode.OK)
                .Or<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(value: 2))
                .ExecuteAsync(async () =>
                {
                    var geoResult = await new DependencyMeasurement(serviceProvider)
                        .ForDependency("Google Geo api", MeasuredDependencyType.Http, dependencyData.ConvertToJson(), dependencyTarget)
                        .WithCorrelationId(correlationId)
                        .TrackAsync(async () =>
                        {
                            var geoDetailResponse = await httpClient.GetAsync(geoUrl);
                            return geoDetailResponse;
                        });
                    
                    return geoResult;
                });

            var content = await geoHttpResponseMessage.Content.ReadAsStringAsync();
            var geoRootObject = content.ConvertFromJson<GeoRootObject>();
            var searchGeoLocationStatusContract = new SearchGeoLocationStatusContract();
            searchGeoLocationStatusContract.Status = geoRootObject.status;
            
            foreach (var item in geoRootObject.results)
            {
                searchGeoLocationStatusContract.Latitude = item.geometry.location.lat;
                searchGeoLocationStatusContract.Longitude = item.geometry.location.lng;
                searchGeoLocationStatusContract.AddressType = string.Join(",", item.types);
                searchGeoLocationStatusContract.CompoundCode = item.plus_code.compound_code;
                searchGeoLocationStatusContract.FormattedAddress = item.formatted_address;
            }

            return searchGeoLocationStatusContract;
        }
    }
}