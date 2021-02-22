using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Storage.Blob.Connections;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Delivery.Azure.Library.Storage.Blob.Interfaces
{
    /// <summary>
    ///     Provides access to blob storage <see cref="CloudBlobClient" />
    /// </summary>
    public interface IBlobStorageConnectionManager : IConnectionManager<BlobStorageConnection>
    {
    }
}