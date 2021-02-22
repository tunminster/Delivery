using Microsoft.WindowsAzure.Storage.Blob;
using Delivery.Azure.Library.Connection.Managers;
using Delivery.Azure.Library.Connection.Managers.Interfaces;

namespace Delivery.Azure.Library.Storage.Blob.Connections
{
    public class BlobStorageConnection : Connection.Managers.Connection
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="connectionMetadata">Metadata to describe the connection</param>
        /// <param name="blobClient">Connection to a specific blob container</param>
        public BlobStorageConnection(IConnectionMetadata connectionMetadata, CloudBlobClient blobClient)
            : base(connectionMetadata)
        {
            CloudBlobClient = blobClient;
        }

        /// <summary>
        ///     Connection to the blob container
        /// </summary>
        protected internal CloudBlobClient CloudBlobClient { get; }
    }
}