using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.CommandHandlers.StoreIndexing
{
    public class StoreIndexCommandHandler : ICommandHandler<StoreIndexCommand, StoreIndexStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public StoreIndexCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreIndexStatusContract> HandleAsync(StoreIndexCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            var store =
                await databaseContext.Stores
                    .Include(x => x.StoreType)
                    .Include(x => x.OpeningHours)
                    .Include(x => x.StorePaymentAccount)
                    .FirstOrDefaultAsync(x =>
                    x.ExternalId == command.StoreIndexCreationContract.StoreId);

            var storeContract = new StoreContract
            {
                StoreId = store.ExternalId,
                StoreName = store.StoreName,
                AddressLine1 = store.AddressLine1,
                AddressLine2 = store.AddressLine2,
                City = store.City,
                County = store.County,
                Country = store.Country,
                ImageUri = store.ImageUri,
                Location = $"{store.Latitude}, {store.Longitude}",
                PostalCode = store.PostalCode,
                IsActive = store.IsActive,
                StoreOpeningHours = store.OpeningHours.Select(x =>
                    new StoreOpeningHourContract
                    {
                        DayOfWeek = x.DayOfWeek,
                        Open = x.Open,
                        Close = x.Close,
                        TimeZone = x.TimeZone
                    }).ToList(),
                StorePaymentAccountNumber = store.StorePaymentAccount?.AccountNumber ?? string.Empty,
                StoreType = store.StoreType.StoreTypeName
            };
            
            //await elasticClient.IndexDocumentAsync(storeContract);

            var indexExisted = await elasticClient.Indices.ExistsAsync($"stores{executingRequestContextAdapter.GetShard().Key.ToLower()}");
            if (!indexExisted.Exists)
            {
                var createIndexResponse = await elasticClient.Indices.CreateAsync($"stores{executingRequestContextAdapter.GetShard().Key.ToLower()}", c => c
                    .Map<StoreContract>(m => m.AutoMap()
                        .Properties(p => p
                            .GeoPoint(d => d
                                .Name(n =>n.Location)
                            )
                        )
                    )
                );
                
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(StoreContract)} elastic index 'stores' created {createIndexResponse.Acknowledged}", 
                        SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
            }
            
           var getResponse = await elasticClient.GetAsync<StoreContract>(storeContract.StoreId, d => d.Index($"stores{executingRequestContextAdapter.GetShard().Key.ToLower()}"));

           if (getResponse.Found)
           {
               var updateResponse = await elasticClient.UpdateAsync<StoreContract>(storeContract.StoreId,
                   descriptor => descriptor.Index($"stores{executingRequestContextAdapter.GetShard().Key.ToLower()}").Doc(storeContract));

               if (updateResponse.IsValid)
               {
                   serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                       .TrackTrace($"{nameof(StoreContract)} elastic doc updated for {storeContract.StoreId}", 
                           SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
               }
               
           }
           else
           {
               var createResponse = await elasticClient.CreateAsync(storeContract,
                   i => i
                       .Index($"stores{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                       .Id(storeContract.StoreId)
               );

               if (createResponse.IsValid)
               {
                   serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                       .TrackTrace($"{nameof(StoreContract)} elastic doc created for {storeContract.StoreId}",
                           SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
               }
           }

           var storeIndexStatusContract = new StoreIndexStatusContract
            {
                Status = true,
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            return storeIndexStatusContract;
        }
    }
}