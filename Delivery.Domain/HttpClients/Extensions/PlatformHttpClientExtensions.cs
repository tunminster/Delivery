using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Domain.HttpClients.Extensions
{
    public static class PlatformHttpClientExtensions
    {
        // public static async Task<HttpClient> WithAuthorizationHeaderAsync(this HttpClient httpClient, IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, string apiCategory)
        // {
        //     var scope = ConvertCategoryToScope(apiCategory);
        //     var cacheKey = $"{executingRequestContextAdapter.GetShard()}-{scope}";
        //     var authorizationHeader = await new BearerTokenClient(serviceProvider, new PlatformApiConfigurationDefinition(serviceProvider, executingRequestContextAdapter, scope)).GetBearerTokenAsync(cacheKey);
        //     httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeader);
        //     return httpClient;
        // }

        public static Dictionary<string, string> GetApiScopes<TEnum>() where TEnum : struct, System.Enum
        {
            var apiCategories = System.Enum.GetValues<TEnum>();
            var scopes = apiCategories.Cast<object?>().ToDictionary(apiCategory => ConvertCategoryToScope(apiCategory?.ToString()), apiCategory => $"{apiCategory} api access");
            return scopes;
        }

        private static string ConvertCategoryToScope(string? apiCategory)
        {
            if (string.IsNullOrEmpty(apiCategory))
            {
                throw new InvalidOperationException("No api category was supplied");
            }

            var scope = $"category_{apiCategory.ToLowerInvariant()}";
            return scope;
        }
    }
}