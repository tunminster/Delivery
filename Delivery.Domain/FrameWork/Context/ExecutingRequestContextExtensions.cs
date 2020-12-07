using System;
using Delivery.Azure.Library.Authentication.OpenIdConnect.Extensions;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Extensions;
using Delivery.Azure.Library.Sharding.Sharding;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Microsoft.AspNetCore.Http;

namespace Delivery.Domain.FrameWork.Context
{
    public static class ExecutingRequestContextExtensions
    {
        /// <summary>
        ///     Creates a 'headless' execution context for use in automation scenarios where there is no caller
        /// </summary>
        public static ExecutingRequestContextAdapter CreateAutomationExecutingRequestContextAdapter(string shardKey, int? ring = 0)
        {
            var shard = Shard.Create(shardKey);
            var authenticatedUser = new AuthenticatedUserContract
            {
                Role = "role1",
                ShardKey = shard.Key,
                UserEmail = $"automation@{shardKey}.com"
            };

            var requestContextAdapter = new ExecutingRequestContextAdapter(new ExecutingRequestContext
            {
                Ring = ring.GetValueOrDefault(),
                ShardKey = shard.Key,
                CorrelationId = Guid.NewGuid().ToString(),
                AuthenticatedUser = authenticatedUser
            });

            return requestContextAdapter;
        }

        public static ExecutingRequestContext GetExecutingRequestContext(this HttpRequest httpRequest)
        {
            var ring = httpRequest.GetRing();
            var shard = httpRequest.GetShard();
            var correlationId = httpRequest.GetCorrelationId();
            var partnerRole = "role1";
            var authenticatedUser = httpRequest.GetAuthenticatedUser(partnerRole);

            var executingRequestContext = new ExecutingRequestContext
            {
                Ring = ring,
                ShardKey = shard.Key,
                CorrelationId = correlationId,
                AuthenticatedUser = authenticatedUser
            };

            return executingRequestContext;
        }

        public static IExecutingRequestContextAdapter GetExecutingRequestContextAdapter(this HttpRequest httpRequest)
        {
            return new ExecutingRequestContextAdapter(httpRequest.GetExecutingRequestContext());
        }
    }
}