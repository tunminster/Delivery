using System;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Nest;

namespace Delivery.Store.Domain.Extensions
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["elasticsearch:url"];
            var defaultIndex = configuration["elasticsearch:index"];
            
            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex)
                .DefaultMappingFor<StoreContract>(x => x)
                ;

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
            
        }
    }
}