using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Converters;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using ServiceStack;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderIndexCommand(string OrderId);
    
    public class ShopOrderIndexCommandHandler : ICommandHandler<ShopOrderIndexCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderIndexCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> HandleAsync(ShopOrderIndexCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var order =
                await databaseContext.Orders
                    .Where(x => x.ExternalId == command.OrderId)
                    .Include(x => x.Store)
                    .Include(x => x.Address)
                    .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Product)
                    .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemMeatOptions)
                    .ThenInclude(x => x.OrderItemMeatOptionValues)
                    .FirstOrDefaultAsync() ??
                throw new InvalidOperationException($"Expected an order by order id {command.OrderId}.");
            
            var driverOrder = await databaseContext.DriverOrders
                .Where(x => x.OrderId == order.Id && x.Status != DriverOrderStatus.Rejected)
                .Include(x => x.Driver)
                .FirstOrDefaultAsync();
            
            var shopOrderContract = ShopOrderContractConverter.ConvertToContract(order, driverOrder);
            
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            var indexExist = await elasticClient.Indices.ExistsAsync($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}");

            if (!indexExist.Exists)
            {
                var createIndexResponse = await elasticClient.Indices.CreateAsync($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}", c => c
                    .Map<ShopOrderContract>(m => m.AutoMap()));
            }
            
            var getResponse = await elasticClient.GetAsync<ShopOrderContract>(shopOrderContract.OrderId, d => d.Index($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}"));

            if (getResponse.Found)
            {
                var updateResponse = await elasticClient.UpdateAsync<ShopOrderContract>(shopOrderContract.OrderId,
                    descriptor => descriptor.Index($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}").Doc(shopOrderContract));
                if (updateResponse.IsValid)
                {
                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                        .TrackTrace($"{nameof(ShopOrderContract)} elastic doc updated for {shopOrderContract.OrderId}", 
                            SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
                }
            }
            else
            {
                var createResponse = await elasticClient.CreateAsync(shopOrderContract,
                    i => i
                        .Index($"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                        .Id(shopOrderContract.OrderId)
                );
                
                if (createResponse.IsValid)
                {
                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                        .TrackTrace($"{nameof(ShopOrderContract)} elastic doc created for {shopOrderContract.OrderId}",
                            SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
                }
            }

            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}