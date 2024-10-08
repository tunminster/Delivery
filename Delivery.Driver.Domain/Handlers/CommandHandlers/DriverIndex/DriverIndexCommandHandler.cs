using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverElasticSearch;
using Delivery.Driver.Domain.Converters;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex
{
    public record DriverIndexCommand(string DriverId);
    public class DriverIndexCommandHandler : ICommandHandler<DriverIndexCommand, DriverIndexStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverIndexCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverIndexStatusContract> HandleAsync(DriverIndexCommand command)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driver = await databaseContext.Drivers.FirstOrDefaultAsync(x => x.ExternalId == command.DriverId) ??
                         throw new InvalidOperationException($"Expected a driver by Id {command.DriverId}")
                             .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverContract = driver.ConvertToDriverContract();
            
            var indexExist = await elasticClient.Indices.ExistsAsync($"{ElasticSearchIndexConstants.DriversIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}");

            if (!indexExist.Exists)
            {
                var createIndexResponse = await elasticClient.Indices.CreateAsync($"{ElasticSearchIndexConstants.DriversIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}", c => c
                    .Map<DriverContract>(m => m.AutoMap()
                        .Properties(p => p
                            .GeoPoint(d => d
                                .Name(n =>n.Location)
                            )
                        )
                    )
                );
                
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(DriverContract)} elastic index 'drivers' created {createIndexResponse.Acknowledged}", 
                        SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
            }
            
            var getResponse = await elasticClient.GetAsync<DriverContract>(driverContract.DriverId, d => d.Index($"{ElasticSearchIndexConstants.DriversIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}"));

            if (getResponse.Found)
            {
                var updateResponse = await elasticClient.UpdateAsync<DriverContract>(driverContract.DriverId,
                    descriptor => descriptor.Index($"{ElasticSearchIndexConstants.DriversIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}").Doc(driverContract));
                
                if (updateResponse.IsValid)
                {
                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                        .TrackTrace($"{nameof(DriverContract)} elastic doc updated for {driverContract.DriverId}", 
                            SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
                }
            }
            else
            {
                var createResponse = await elasticClient.CreateAsync(driverContract,
                    i => i
                        .Index($"{ElasticSearchIndexConstants.DriversIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                        .Id(driverContract.DriverId)
                );
                
                if (createResponse.IsValid)
                {
                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                        .TrackTrace($"{nameof(DriverContract)} elastic doc created for {driverContract.DriverId}",
                            SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
                }
            }

            var driverIndexStatusContract = new DriverIndexStatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };

            return driverIndexStatusContract;
        }
    }
}