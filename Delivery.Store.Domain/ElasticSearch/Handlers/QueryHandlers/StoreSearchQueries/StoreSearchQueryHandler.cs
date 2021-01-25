using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.QueryHandlers.StoreSearchQueries
{
    public class StoreSearchQueryHandler : IQueryHandler<StoreSearchQuery, List<StoreContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public StoreSearchQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<StoreContract>> Handle(StoreSearchQuery query)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();

            var currentLocation = new GeoLocation(query.Latitude, query.Longitude);
            
            
            var searchResponse = await elasticClient.SearchAsync<StoreContract>(s => s
                .AllIndices()
                .Query(q => 
                    q.QueryString(qs => 
                        qs.Fields(f => 
                            f.Field("storename.*")
                                ).Query(query.QueryString))
                    && q.Bool(b => 
                        b.Filter(f => 
                            f.GeoDistance(g => 
                                g.Boost(1.1)
                                    .Name("location")
                                    .Field(p => p.Location)
                                    .DistanceType(GeoDistanceType.Arc)
                                    .Location(currentLocation)
                                    .Distance(query.Distance) // Eg: "3km"
                                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                )))
                )
                .From((query.Page - 1) * query.PageSize)
                .Size(query.PageSize));
                
                
            var storeContracts = searchResponse.Documents.ToList();

            return storeContracts;
        }
    }
}