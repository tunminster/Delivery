using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverElasticSearch;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverElasticSearch
{
    public record DriverIndexCommand(DriverContract DriverContract);
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
        
        public async Task<DriverIndexStatusContract> Handle(DriverIndexCommand command)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();

            var driverContract = command.DriverContract;
            
            var indexExist = await elasticClient.Indices.ExistsAsync($"drivers{executingRequestContextAdapter.GetShard().Key.ToLower()}");

            if (!indexExist.Exists)
            {
                var createIndexResponse = await elasticClient.Indices.CreateAsync($"drivers{executingRequestContextAdapter.GetShard().Key.ToLower()}", c => c
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
            
            var getResponse = await elasticClient.GetAsync<DriverContract>(driverContract.DriverId, d => d.Index($"drivers{executingRequestContextAdapter.GetShard().Key.ToLower()}"));

            if (getResponse.Found)
            {
                var updateResponse = await elasticClient.UpdateAsync<DriverContract>(driverContract.DriverId,
                    descriptor => descriptor.Index($"drivers{executingRequestContextAdapter.GetShard().Key.ToLower()}").Doc(driverContract));
                
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
                        .Index($"drivers{executingRequestContextAdapter.GetShard().Key.ToLower()}")
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