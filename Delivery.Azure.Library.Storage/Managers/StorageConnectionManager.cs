using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Connection.Managers;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Storage.Blob.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

namespace Delivery.Azure.Library.Storage.Managers
{
    /// <summary>
    ///     Manages connections to Azure Storage implemented in specific classes like
    ///     <see cref="BlobStorageConnectionManager" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StorageConnectionManager<T> : ConnectionManager<T> where T : IConnection
    {
        public override int PartitionCount { get; } = 0;

        protected abstract override Task<T> CreateConnectionAsync(IConnectionMetadata connectionMetadata);

        protected async Task<CloudStorageAccount> GetCloudStorageAccountAsync(IConnectionMetadata connectionMetadata)
        {
            var connectionString = await GetConnectionStringAsync(connectionMetadata);

            if (!CloudStorageAccount.TryParse(connectionString, out var storageAccount))
            {
                throw new InvalidOperationException($"The specified connection string to the table storage account is invalid. Found {connectionString}");
            }

            return storageAccount;
        }

        private async Task<string> GetConnectionStringAsync(IConnectionMetadata connectionMetadata)
        {
            var connectionString = await ServiceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync(connectionMetadata.SecretName);

            return connectionString;
        }

        protected StorageConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}