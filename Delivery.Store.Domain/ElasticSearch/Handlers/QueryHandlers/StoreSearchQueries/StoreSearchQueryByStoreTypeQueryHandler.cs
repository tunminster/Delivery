using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.QueryHandlers.StoreSearchQueries
{
    public class StoreSearchQueryByStoreTypeQueryHandler : IQueryHandler<StoreSearchQueryByStoreTypeQuery, List<StoreContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public StoreSearchQueryByStoreTypeQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<StoreContract>> Handle(StoreSearchQueryByStoreTypeQuery query)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            var currentLocation = new GeoLocation(query.Latitude, query.Longitude);
            
            var searchResponse = await elasticClient.SearchAsync<StoreContract>(s => s
                .AllIndices()
                .Query(q => 
                    q.Match(m => 
                        m.Field(f => 
                            f.StoreType)
                        .Query(query.StoreType)
                        .Operator(Operator.And)
                        .ZeroTermsQuery(ZeroTermsQuery.All))
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
                .Source()
                .ScriptFields(sf => sf
                    .ScriptField("distance", descriptor => descriptor
                        .Source(script:$"doc[\u0027location\u0027].arcDistance({query.Latitude}, {query.Longitude}) * 0.001")
                    ))
                .From((query.Page - 1) * query.PageSize)
                .Size(query.PageSize));
            
            var storeContractAndDistance = searchResponse.Documents
                .Zip(searchResponse.Fields, (s, d) => new StoreContract
                {
                    StoreId = s.StoreId,
                    StoreName = s.StoreName,
                    AddressLine1 = s.AddressLine1,
                    AddressLine2 = s.AddressLine2,
                    City = s.City,
                    County = s.County,
                    Country = s.Country,
                    ImageUri = s.ImageUri,
                    Location = s.Location,
                    PostalCode = s.PostalCode,
                    StoreType = s.StoreType,
                    Distance = d.Value<double>("distance")
                }).ToList();

            return storeContractAndDistance;
        }
    }
}