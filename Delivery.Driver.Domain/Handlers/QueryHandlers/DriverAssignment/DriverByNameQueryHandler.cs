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
    public record DriverByNameQuery(string DriverName, int Page, int PageSize);

    public class DriverByNameQueryHandler
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverByNameQueryHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<List<DriverContract>> HandleAsync(DriverByNameQuery query)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();

            var driverContactList = await new DependencyMeasurement(serviceProvider)
                .ForDependency($"Search-{nameof(DriverContract)}", MeasuredDependencyType.ElasticSearch,
                    query.ConvertToJson(), "http://localhost:9200")
                .TrackAsync(async () =>
                {
                    var driverSearchResult = await elasticClient.SearchAsync<DriverContract>(x =>
                        x.Index(
                                $"{ElasticSearchIndexConstants.DriversIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                            .Query(q =>
                                q.QueryString(qs =>
                                    qs.Fields(qfs =>
                                        qfs.Field(f => f.FullName)
                                            .Field(f => f.EmailAddress)
                                    ).Query(query.DriverName)))
                            .Source()
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