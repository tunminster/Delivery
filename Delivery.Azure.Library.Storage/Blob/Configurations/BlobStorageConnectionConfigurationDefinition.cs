using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Delivery.Azure.Library.Storage.Blob.Configurations
{
    public class BlobStorageConnectionConfigurationDefinition : ConfigurationDefinition
    {
        public BlobStorageConnectionConfigurationDefinition(IServiceProvider serviceProvider,string containerName, BlobContainerPublicAccessType accessType, string secretName) : base(serviceProvider)
        {
            ContainerName = containerName;
            SecretName = secretName;
            AccessType = accessType;
        }
        
        /// <summary>
        ///     The name of the blob container.
        /// </summary>
        public string ContainerName { get; }

        /// <summary>
        ///     The secret that contains the full connection string, retrieved with <see cref="ISecretProvider" />
        /// </summary>
        public string SecretName { get; }

        /// <summary>
        ///     Access type of the container.
        /// </summary>
        public BlobContainerPublicAccessType AccessType { get; }
    }
}