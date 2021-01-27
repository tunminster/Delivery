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
                            f.Field(x => x.StoreName)
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
                .Source()
                .ScriptFields(sf => sf
                    .ScriptField("distance", descriptor => descriptor
                        .Source(script:$"doc[\u0027location\u0027].arcDistance({query.Latitude}, {query.Longitude}) * 0.001")
                        ))
                // .Sort(sort => sort
                //     .GeoDistance(g => g
                //         .Field(f => f.Location)
                //         .Order(SortOrder.Ascending)
                //         .DistanceType(GeoDistanceType.Arc)))
                // .Aggregations(a => 
                //     a.ScriptedMetric("distances", st => 
                //         st.Script(sr => 
                //             sr.Source($"doc['location'].arcDistance({query.Latitude}, {query.Longitude}) * 0.001"))
                //      
                //         ))
                // .ScriptFields(x => x.ScriptField("distance", d => d.Source($"doc['location'].arcDistance({query.Latitude}, {query.Longitude}) * 0.001")
                //     .Params(p => new FluentDictionary<string, object>()
                //     {
                //         {"latitude",query.Latitude},
                //         {"longitude",query.Longitude}
                //     })))
                .From((query.Page - 1) * query.PageSize)
                .Size(query.PageSize));
            
            
            // var searchResponse = await elasticClient.SearchAsync<StoreContract>(s => s
            //     .AllIndices()
            //     .Query(q => 
            //         q.QueryString(qs => 
            //             qs.Fields(f => 
            //                 f.Field("storename.*")
            //                     ).Query(query.QueryString))
            //         && q.Bool(b => 
            //             b.Filter(f => 
            //                 f.GeoDistance(g => 
            //                     g.Boost(1.1)
            //                         .Name("location")
            //                         .Field(p => p.Location)
            //                         .DistanceType(GeoDistanceType.Arc)
            //                         .Location(currentLocation)
            //                         .Distance(query.Distance) // Eg: "3km"
            //                         .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
            //                     )))
            //     )
            //     .From((query.Page - 1) * query.PageSize)
            //     .Size(query.PageSize));

            foreach (var field in searchResponse.Fields)
            {
                var test = field.Value<double>("distance");
            }
                
            var storeContracts = searchResponse.Documents.ToList();

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
                    Distance = d.Value<double>("distance")
                }).ToList();

            return storeContractAndDistance;
        }
    }
}