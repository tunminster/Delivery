using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Database.Context;
using Delivery.Domain.Constants;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderSearch;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopOrderSearch
{
    public record ShopOrderSearchQuery : IQuery<List<ShopOrderContract>>
    {
        public string Email { get; init; } = string.Empty;
        public SearchOrderQueryContract SearchOrderQueryContract { get; init; } = new();
    }
    
    public class ShopOrderSearchQueryHandler : IQueryHandler<ShopOrderSearchQuery, List<ShopOrderContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderSearchQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<ShopOrderContract>> Handle(ShopOrderSearchQuery query)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            // todo: cache
            var storeUser = await databaseContext.StoreUsers.Where(x => x.Username == query.Email)
                .Include(x => x.Store).SingleOrDefaultAsync();
            
            var shopOrderContractList = await new DependencyMeasurement(serviceProvider)
                .ForDependency($"Index-{nameof(ShopOrderContract)}", MeasuredDependencyType.ElasticSearch, query.ConvertToJson(), "http://localhost:9200")
                .TrackAsync(async () =>
                {
                    var shopOrderContractResult = await elasticClient.SearchAsync<ShopOrderContract>(x =>
                        x.Index(
                                $"{ElasticSearchIndexConstants.ShopOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                            .Query(q =>
                                  q.Match(m => 
                                    m.Field(fl => 
                                        fl.StoreId)
                                        .Query(storeUser.Store.ExternalId)
                                        .Operator(Operator.And)
                                        .ZeroTermsQuery(ZeroTermsQuery.All))
                                  &&
                                    q.Bool(b => 
                                        b.Filter(ft => 
                                            ft.Terms( tms => 
                                                tms.Field(fld => fld.Status).Terms(query.SearchOrderQueryContract.Status))))
                                  && 
                                  q.QueryString(queryString => 
                                      queryString
                                          .Boost(1.1)
                                          .Fields(fs => 
                                          fs.Field(f1 => f1.OrderId)
                                          ).Query(query.SearchOrderQueryContract.FreeTextSearch)
                                          .DefaultOperator(Operator.Or)
                                          .AllowLeadingWildcard()
                                          .Escape())
                                  
                            )
                            .Source()
                            .From((query.SearchOrderQueryContract.Page - 1) * query.SearchOrderQueryContract.PageSize)
                            .Size(query.SearchOrderQueryContract.PageSize));
                    
                    var shopOrderContracts = shopOrderContractResult.Documents
                        .Zip(shopOrderContractResult.Fields, (s, d) => new ShopOrderContract
                        {
                            StoreId = s.StoreId,
                            OrderId = s.OrderId,
                            OrderType = s.OrderType,
                            Status = s.Status,
                            ShopOrderItems = s.ShopOrderItems,
                            Subtotal = s.Subtotal,
                            TotalAmount = s.TotalAmount,
                            PlatformServiceFee = s.PlatformServiceFee,
                            DeliveryFee = s.DeliveryFee,
                            Tax = s.Tax, 
                            BusinessServiceFee = s.BusinessServiceFee,
                            PreparationTime = s.PreparationTime,
                            PickupTime = s.PickupTime,
                            DateCreated = s.DateCreated,
                            IsPreparationCompleted = s.IsPreparationCompleted,
                            ShopOrderDriver = s.ShopOrderDriver,
                            ShopOrderDeliveryAddress = s.ShopOrderDeliveryAddress
                        }).ToList();
                    
                    return shopOrderContracts;
                });
            
            return shopOrderContractList;
        }
    }
}