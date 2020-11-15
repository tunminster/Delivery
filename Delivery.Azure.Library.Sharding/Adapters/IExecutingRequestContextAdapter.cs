using System.Collections.Generic;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Sharding.Adapters
{
    public interface IExecutingRequestContextAdapter
    {
        int GetRing();
        IShard GetShard();
        string GetCorrelationId();
        AuthenticatedUserContract GetAuthenticatedUser();
        ExecutingRequestContext GetExecutingRequestContext();
        Dictionary<string, string> GetTelemetryProperties();
    }
}