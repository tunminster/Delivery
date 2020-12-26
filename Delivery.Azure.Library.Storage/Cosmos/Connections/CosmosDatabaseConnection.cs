
using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Delivery.Azure.Library.Storage.Cosmos.Connections
{
    public class CosmosDatabaseConnection : Connection.Managers.Connection
    {
        public CosmosDatabaseConnection(IConnectionMetadata connectionMetadata,CosmosClient cosmosClient) : base(connectionMetadata)
        {
            CosmosClient = cosmosClient;
        }
        
        /// <summary>
        ///     Connection to cosmos database
        /// </summary>
        protected internal CosmosClient CosmosClient { get; }

        public override async ValueTask DisposeAsync()
        {
            CosmosClient.Dispose();
            await base.DisposeAsync();
        }
    }
}