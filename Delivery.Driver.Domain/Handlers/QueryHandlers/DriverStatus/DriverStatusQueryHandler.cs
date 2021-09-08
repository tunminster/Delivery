using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using Delivery.Driver.Domain.Converters;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverStatus
{
    public record DriverStatusQuery(string EmailAddress) : IQuery<DriverActiveStatusContract>;
    
    public class DriverStatusQueryHandler : IQueryHandler<DriverStatusQuery, DriverActiveStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverStatusQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverActiveStatusContract> Handle(DriverStatusQuery query)
        {
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();

            var searchResponse = await elasticClient.SearchAsync<DriverContract>(s => s
                .Index($"drivers{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                .Query(q =>
                    q.Match(m => m.Field(f => f.EmailAddress)
                        .Query(query.EmailAddress)
                        .ZeroTermsQuery(ZeroTermsQuery.All)))
                .Source()
                .Size(50));

            var driverList = searchResponse.Documents.ToList();

            return driverList.First(x => x.EmailAddress == query.EmailAddress).ConvertToDriverActiveStatusContract();
        }
    }
}