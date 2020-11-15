using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Core.Extensions;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Delivery.Azure.Library.Telemetry.Constants;

namespace Delivery.Azure.Library.Sharding.Adapters
{
    public class ExecutingRequestContextAdapter : IExecutingRequestContextAdapter
	{
		private readonly int ring;
		private readonly IShard shard;
		private readonly string correlationId;
		private readonly AuthenticatedUserContract authenticatedUser;
		private readonly ExecutingRequestContext requestContext;

		public ExecutingRequestContextAdapter(ExecutingRequestContext? executingRequestContext)
		{
			requestContext = executingRequestContext ?? throw new InvalidOperationException($"{nameof(executingRequestContext)} must be provided: {requestContext.Format()}");
			ring = requestContext.Ring ?? throw new InvalidOperationException($"{nameof(executingRequestContext.Ring)}  must be provided: {requestContext.Ring.Format()}");
			shard = string.IsNullOrEmpty(requestContext.ShardKey) ? throw new InvalidOperationException($"{nameof(executingRequestContext.ShardKey)}  must be provided: {requestContext.ShardKey.Format()}") : Shard.Create(requestContext.ShardKey);
			correlationId = requestContext.CorrelationId ?? throw new InvalidOperationException($"{nameof(executingRequestContext.CorrelationId)}  must be provided: {requestContext.CorrelationId.Format()}");
			authenticatedUser = requestContext.AuthenticatedUser ?? throw new InvalidOperationException($"{nameof(executingRequestContext.AuthenticatedUser)}  must be provided: {requestContext.AuthenticatedUser.Format()}");

			if (string.IsNullOrEmpty(requestContext.AuthenticatedUser.ShardKey))
			{
				throw new InvalidOperationException($"{nameof(executingRequestContext.AuthenticatedUser.ShardKey)} must be provided: {requestContext.AuthenticatedUser.ShardKey.Format()}");
			}

			if (string.IsNullOrEmpty(requestContext.AuthenticatedUser.UserEmail))
			{
				throw new InvalidOperationException($"{nameof(executingRequestContext.AuthenticatedUser.UserEmail)}  must be provided: {requestContext.AuthenticatedUser.UserEmail.Format()}");
			}
		}

		public ExecutingRequestContext GetExecutingRequestContext()
		{
			return requestContext;
		}

		public Dictionary<string, string> GetTelemetryProperties()
		{
			var customProperties = new Dictionary<string, string>
			{
				{CustomProperties.Shard, shard.Key},
				{CustomProperties.CorrelationId, correlationId},
				{CustomProperties.Ring, ring.ToString()}
			};
			return customProperties;
		}

		public int GetRing()
		{
			return ring;
		}

		public IShard GetShard()
		{
			return shard;
		}

		public string GetCorrelationId()
		{
			return correlationId;
		}

		public AuthenticatedUserContract GetAuthenticatedUser()
		{
			return authenticatedUser;
		}

		public override string ToString()
		{
			return $"{GetType().Name} - {nameof(Shard)} - {shard.Format()}, Ring - {ring.Format()}";
		}
	}
}