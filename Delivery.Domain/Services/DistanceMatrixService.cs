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
using Delivery.Domain.Contracts.V1.RestContracts.DistanceMatrix;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Delivery.Domain.Services
{
    public class DistanceMatrixService
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DistanceMatrixService(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<DistanceMatrixResponseContract> GetDistanceAsync(
            DistanceMatrixRequestContract distanceMatrixRequestContract)
        {
            var distanceApiUri = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("Distance-Api-Uri");
            var distanceApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync("Distance-Api-Key");
            
            string distanceUrl = $"{distanceApiUri}?destinations={distanceMatrixRequestContract.DestinationLatitude},{distanceMatrixRequestContract.DestinationLongitude}&origins={distanceMatrixRequestContract.SourceLatitude},{distanceMatrixRequestContract.DestinationLongitude}&key={distanceApiKey}";
            
            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
            
            var dependencyData = new DependencyData(nameof(DistanceMatrixRequestContract), distanceMatrixRequestContract);
            var dependencyTarget = distanceApiUri;
            var correlationId = executingRequestContextAdapter.GetCorrelationId();
            
            var distanceHttpResponseMessage = await Policy.HandleResult<HttpResponseMessage>(x => x.StatusCode != HttpStatusCode.OK)
                .Or<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(value: 2))
                .ExecuteAsync(async () =>
                {
                    var distanceResult = await new DependencyMeasurement(serviceProvider)
                        .ForDependency("Google Distance api", MeasuredDependencyType.Http, dependencyData.ConvertToJson(), dependencyTarget)
                        .WithCorrelationId(correlationId)
                        .TrackAsync(async () =>
                        {
                            var distanceDetailResponse = await httpClient.GetAsync(distanceUrl);
                            return distanceDetailResponse;
                        });
                    
                    return distanceResult;
                });
            
            var content = await distanceHttpResponseMessage.Content.ReadAsStringAsync();
            var distanceMatrixResponseContract = content.ConvertFromJson<DistanceMatrixResponseContract>();

            return distanceMatrixResponseContract;
        }
    }
}