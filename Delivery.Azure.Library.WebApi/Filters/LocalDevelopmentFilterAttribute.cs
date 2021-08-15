using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core;
using Delivery.Azure.Library.Microservices.Hosting.Extensions;
using Delivery.Azure.Library.Sharding.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.WebApi.Filters
{
    /// <summary>
    ///     Repairs certain symbols which are lost during url encoding
    /// </summary>
    public class LocalDevelopmentFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContextRequestServices = context.HttpContext.RequestServices;
            var useInMemory = httpContextRequestServices.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<bool>("Test_Use_In_Memory", defaultValue: false);
            if (useInMemory)
            {
                var internalShardSettings = httpContextRequestServices.GetRequiredService<IInternalShardSettings>();
                var httpContextRequest = context.HttpContext.Request;

                if (!httpContextRequest.Headers.TryGetValue(HttpHeaders.Shard, out _))
                {
                    if (httpContextRequest.Headers.TryGetValue(HttpHeaders.OnBehalfOfShard, out _))
                    {
                        httpContextRequest.Headers[HttpHeaders.Shard] = httpContextRequest.Headers[HttpHeaders.OnBehalfOfShard];
                    }
                    else
                    {
                        httpContextRequest.Headers[HttpHeaders.Shard] = internalShardSettings.DefaultShardKey;
                    }

                    httpContextRequest.Headers[HttpHeaders.UserId] = "localuser";
                    httpContextRequest.Headers[HttpHeaders.UserEmail] = "localuser@local.ch";
                }

                if (!httpContextRequest.Headers.TryGetValue(HttpHeaders.Ring, out var ring))
                {
                    httpContextRequest.Headers[HttpHeaders.Ring] = httpContextRequestServices.GetRuntimeRing().ToString();
                }

                if (ring == "{{X-Ring}}")
                {
                    httpContextRequest.Headers[HttpHeaders.Ring] = 0.ToString();
                }
            }

            await next();
        }
    }
}