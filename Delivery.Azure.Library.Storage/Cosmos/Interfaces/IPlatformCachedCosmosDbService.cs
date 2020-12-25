using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;

namespace Delivery.Azure.Library.Storage.Cosmos.Interfaces
{
    public interface IPlatformCachedCosmosDbService : IPlatformCosmosDbService
    {
        Task<TDocument?> GetLatestDocumentAsync<TDocument, TContract>(string partitionKey, double minutesToCache, bool isDocumentRequired = true, DateTimeOffset? validFromDate = null, DateTimeOffset? validToDate = null)
            where TDocument : DocumentContract<TContract>, new()
            where TContract : class;
    }
}