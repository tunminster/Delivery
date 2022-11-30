using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Converters;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverOrderIndexCommand(DriverOrderIndexCreationContract DriverOrderIndexCreationContract);
    
    public class DriverOrderIndexCommandHandler : ICommandHandler<DriverOrderIndexCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderIndexCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(DriverOrderIndexCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driver = await databaseContext.Drivers.SingleAsync(x =>
                x.ExternalId == command.DriverOrderIndexCreationContract.DriverId);

            var order = await databaseContext.Orders.SingleOrDefaultAsync(x =>
                x.ExternalId == command.DriverOrderIndexCreationContract.OrderId) ?? throw new InvalidOperationException($"Expected order by order id: {command.DriverOrderIndexCreationContract.OrderId}.");

            var driverOrder = await databaseContext.DriverOrders
                .SingleOrDefaultAsync(x => x.DriverId == driver.Id && x.OrderId == order.Id);

            if (driverOrder != null)
            {
                var driverOrderContract = driverOrder.ConvertToDriverOrderContract(order, driver);
                var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
                var indexExist = await elasticClient.Indices.ExistsAsync($"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}");

                if (!indexExist.Exists)
                {
                    var createIndexResponse = await elasticClient.Indices.CreateAsync($"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}", c => c
                        .Map<DriverOrderContract>(m => m.AutoMap()));
                }
                
                var getResponse = await elasticClient.GetAsync<DriverOrderContract>(driverOrderContract.Id, d => d.Index($"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}"));
                if (getResponse.Found)
                {
                    var updateResponse = await elasticClient.UpdateAsync<DriverOrderContract>(driverOrderContract.Id,
                        descriptor => descriptor.Index($"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}").Doc(driverOrderContract));
                    
                    if (updateResponse.IsValid)
                    {
                        serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                            .TrackTrace($"{nameof(DriverOrderContract)} elastic doc updated for {driverOrderContract.Id}", 
                                SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
                    }
                }
                else
                {
                    var createResponse = await elasticClient.CreateAsync(driverOrderContract,
                        i => i
                            .Index($"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                            .Id(driverOrderContract.Id)
                    );
                    
                    if (createResponse.IsValid)
                    {
                        serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                            .TrackTrace($"{nameof(DriverOrderContract)} elastic doc created for {driverOrderContract.Id}",
                                SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
                    }
                }
            }
        }
    }
}