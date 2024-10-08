using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Domain.Constants;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverAssignment
{
    public record DriverByNearestLocationQuery : IQuery<List<DriverContract>>
    {
        /// <summary>
        ///  Service area latitude
        /// </summary>
        public double Latitude { get; init; }
        
        /// <summary>
        ///  Service area longitude
        /// </summary>
        public double Longitude { get; init; }

        /// <summary>
        ///  Distance 
        /// </summary>
        public string Distance { get; init; } = "10km";
        
        /// <summary>
        ///  Current page
        /// </summary>
        public int Page { get; init; }
        
        /// <summary>
        ///  Page size
        /// </summary>
        public int PageSize { get; init; }
    }
    
    public class DriverByNearestLocationQueryHandler : IQueryHandler<DriverByNearestLocationQuery, List<DriverContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverByNearestLocationQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<DriverContract>> Handle(DriverByNearestLocationQuery query)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            
            var storeLocation = new GeoLocation(query.Latitude, query.Longitude);
            
            var driverContactList = await new DependencyMeasurement(serviceProvider)
                .ForDependency($"Search-{nameof(DriverContract)}", MeasuredDependencyType.ElasticSearch, query.ConvertToJson(), "http://localhost:9200")
                .TrackAsync(async () =>
                {
                    var driverSearchResult = await elasticClient.SearchAsync<DriverContract>(x =>
                        x.Index(
                                $"{ElasticSearchIndexConstants.DriversIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                            .Query(q =>
                                q.Bool(bl => 
                                    bl.Filter(fl => 
                                        fl.Terms(tm => 
                                            tm.Field(fd => fd.IsOrderAssigned).Terms(false))))
                                && q.Bool(ba =>
                                    ba.Filter(baf => 
                                        baf.Terms(tm =>
                                            tm.Field(fd => fd.Approved).Terms(true))))
                                && q.Bool(bat => 
                                    bat.Filter(flat => 
                                        flat.Terms(tm => 
                                            tm.Field(fd => fd.IsActive).Terms(true))))
                                && q.Bool(b =>
                                    b.Filter(f =>
                                        f.GeoDistance(g =>
                                            g.Boost(1.1)
                                                .Name("location")
                                                .Field(p => p.Location)
                                                .DistanceType(GeoDistanceType.Arc)
                                                .Location(storeLocation)
                                                .Distance(query.Distance) // eg: 20km
                                                .ValidationMethod(GeoValidationMethod.IgnoreMalformed))))
                                )
                            .Source()
                            .ScriptFields(sf => sf
                                .ScriptField("distance", descriptor => descriptor
                                    .Source(script:$"doc[\u0027location\u0027].arcDistance({query.Latitude}, {query.Longitude}) * 0.001")
                                ))
                            .From((query.Page - 1) * query.PageSize)
                            .Size(query.PageSize));
                    
                    var driverContracts = driverSearchResult.Documents
                        .Zip(driverSearchResult.Fields, (s, d) => new DriverContract
                        {
                            DriverId = s.DriverId,
                            FullName = s.FullName,
                            EmailAddress = s.EmailAddress,
                            VehicleType = s.VehicleType,
                            ImageUri = s.ImageUri,
                            Location = s.Location,
                            Radius = s.Radius,
                            IsActive = s.IsActive,
                            IsOrderAssigned = s.IsOrderAssigned, 
                            Approved = s.Approved
                        }).ToList();
                    
                    return driverContracts;
                });

            return driverContactList;
        }
    }
}