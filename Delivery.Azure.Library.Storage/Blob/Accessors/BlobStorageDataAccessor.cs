using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Core.Monads;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Blob.Configurations;
using Delivery.Azure.Library.Storage.Blob.Connections;
using Delivery.Azure.Library.Storage.Blob.Interfaces;
using Delivery.Azure.Library.Storage.Blob.Models;
using Delivery.Azure.Library.Storage.HostedServices.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Delivery.Azure.Library.Storage.Blob.Accessors
{
    /// <summary>
	///     Manages reading and writing of data to a blob storage account
	///     Dependencies:
	///     <see cref="IBlobStorageConnectionManager" />
	///     Settings:
	///     <see cref="BlobStorageConnectionConfigurationDefinition" />
	/// </summary>
	public class BlobStorageDataAccessor : BlobStorageAccessor
	{
		private readonly IServiceProvider serviceProvider;

		protected BlobStorageDataAccessor(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, BlobStorageConnection blobStorageConnection, BlobStorageConnectionConfigurationDefinition configurationDefinition)
			: base(executingRequestContextAdapter, blobStorageConnection, configurationDefinition)
		{
			this.serviceProvider = serviceProvider;
		}

		/// <summary>
		///     Creates a new instance of the BlobStorageDataAccessor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="executingRequestContextAdapter">The request details</param>
		/// <param name="connectionStringName">Name of the connection string secret</param>
		/// <param name="containerName">Container name</param>
		/// <param name="publicAccessType">Define the public access type</param>
		public static async Task<BlobStorageDataAccessor> CreateAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, string connectionStringName, string containerName, BlobContainerPublicAccessType publicAccessType = BlobContainerPublicAccessType.Off)
		{
			var configuration = new BlobStorageConnectionConfigurationDefinition(serviceProvider, containerName, publicAccessType, connectionStringName);
			return await CreateAsync(serviceProvider, executingRequestContextAdapter, configuration);
		}

		/// <summary>
		///     Creates a new instance of the BlobStorageDataAccessor
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <param name="executingRequestContextAdapter">The request details</param>
		/// <param name="configurationDefinition">Configuration definition to blob</param>
		public new static async Task<BlobStorageDataAccessor> CreateAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, BlobStorageConnectionConfigurationDefinition configurationDefinition)
		{
			var connection = await GetConnectionTaskAsync(serviceProvider, configurationDefinition);

			return new BlobStorageDataAccessor(serviceProvider, executingRequestContextAdapter, connection, configurationDefinition);
		}

		/// <summary>
		///     Gets an Azure Block Blob by its name
		/// </summary>
		/// <param name="blobName">Name of the blob. Note: if the name contains '/' it will be part of folder path</param>
		public virtual CloudBlockBlob GetBlockBlob(string blobName)
		{
			var container = BlobStorageConnection.CloudBlobClient.GetContainerReference(ConfigurationDefinition.ContainerName);
			var blockBlob = container.GetBlockBlobReference(blobName);
			return blockBlob;
		}

		/// <summary>
		///     Get a blob parameters model by it's name.
		///     Container name check only works with no virtual folder.
		/// </summary>
		/// <param name="blobName">Name of the blob. Note: if the name contains '/' it will be part of folder path</param>
		public virtual async Task<Maybe<BlobParametersModel>> GetBlobParametersModelAsync(string blobName)
		{
			var container = BlobStorageConnection.CloudBlobClient.GetContainerReference(ConfigurationDefinition.ContainerName);

			var blockBlob = container.GetBlockBlobReference(blobName);
			var blockBlobExists = await blockBlob.ExistsAsync();
			if (!blockBlobExists)
			{
				return Maybe<BlobParametersModel>.NotPresent;
			}

			var blobParametersModel = new BlobParametersRequestModel(blockBlob.Name, blockBlob.Name, blockBlob.Properties.ContentType);
			var parametersModel = new BlobParametersModel(blobParametersModel, blockBlob);
			return new Maybe<BlobParametersModel>(parametersModel);
		}

		/// <summary>
		///     Uploads a blob into a storage account container
		/// </summary>
		/// <returns>The blob uri</returns>
		public virtual async Task<string> UploadBlobAsync(BlobParametersUploadModel blobParametersUploadModel)
		{
			var uploadedBlobs = await UploadBlobsAsync(new List<BlobParametersUploadModel> {blobParametersUploadModel});
			return uploadedBlobs.Single();
		}

		/// <summary>
		///     Uploads multiple blobs in parallel using a performance-optimized solutions
		/// </summary>
		/// <returns>A list of uploaded blob urls</returns>
		public virtual async Task<List<string>> UploadBlobsAsync(List<BlobParametersUploadModel> blobParametersModels)
		{
			var container = BlobStorageConnection.CloudBlobClient.GetContainerReference(ConfigurationDefinition.ContainerName);
			var queueBlobUploadWorkBackgroundService = serviceProvider.GetRequiredHostedService<IQueueBlobUploadWorkBackgroundService>();

			var uriList = new List<string>();
			foreach (var blobParametersModel in blobParametersModels)
			{
				var blob = container.GetBlockBlobReference(blobParametersModel.BlobParametersRequestModel.FileName);
				var uri = blob.StorageUri.PrimaryUri.ToString();
				uriList.Add(uri);

				blob.Properties.ContentType = blobParametersModel.BlobParametersRequestModel.ContentType;

				serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Preparing to upload blob '{blob.Name}' to {uri}", SeverityLevel.Information, ExecutingRequestContextAdapter.GetTelemetryProperties());
				await queueBlobUploadWorkBackgroundService.EnqueueBackgroundWorkAsync(ExecutingRequestContextAdapter, blob, blobParametersModel.Stream);
			}

			return uriList;
		}
	}
}