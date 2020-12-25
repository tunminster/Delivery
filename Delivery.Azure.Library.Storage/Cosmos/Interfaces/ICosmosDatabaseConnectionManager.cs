using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Storage.Cosmos.Connections;

namespace Delivery.Azure.Library.Storage.Cosmos.Interfaces
{
    /// <summary>
    ///  Provide access to the <see cref="CosmosClient"/>
    /// </summary>
    public interface ICosmosDatabaseConnectionManager : IConnectionManager<CosmosDatabaseConnection>
    {
        
    }
}