using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Storage.Blob.Interfaces;
using Delivery.Azure.Library.Storage.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Delivery.Azure.Library.Storage.Blob.Connections
{
    /// <summary>
    ///     Manages connections to Azure Storage Blobs, see <see cref="CloudBlobClient" />
    /// </summary>
    public class BlobStorageConnectionManager : StorageConnectionManager<BlobStorageConnection>, IBlobStorageConnectionManager
    {
        public override int PartitionCount => ServiceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault("BlobStorage_ConnectionManager_PartitionCount", defaultValue: 0);
        public override DependencyType DependencyType => DependencyType.Storage;
        public override ExternalDependency ExternalDependency => ExternalDependency.BlobStorage;

        protected override async Task<BlobStorageConnection> CreateConnectionAsync(IConnectionMetadata connectionMetadata)
        {
            var storageAccount = await GetCloudStorageAccountAsync(connectionMetadata);
            var blobClient = storageAccount.CreateCloudBlobClient();

            return new BlobStorageConnection(connectionMetadata, blobClient);
        }

        public BlobStorageConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}