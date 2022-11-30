using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Converters;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderIndexAllCommand(string UserEmail);
    public class ShopOrderIndexAllCommandHandler : ICommandHandler<ShopOrderIndexAllCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderIndexAllCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(ShopOrderIndexAllCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x => x.Username == command.UserEmail);

            var orders = await databaseContext.Orders.Where(x => x.StoreId == storeUser.StoreId)
                .Include(x => x.Store)
                .Include(x => x.Address)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .ToListAsync();

            foreach (var order in orders)
            {
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
            }
            
        }
    }
}