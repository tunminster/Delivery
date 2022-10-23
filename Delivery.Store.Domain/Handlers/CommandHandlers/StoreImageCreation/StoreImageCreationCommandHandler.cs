using System;
using System.IO;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Blob.Accessors;
using Delivery.Azure.Library.Storage.Blob.Models;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreImageCreation
{
    //public class StoreImageCreationCommandHandler : ICommandHandler<StoreImageCreationCommand, StoreImageCreationStatusContract>
    public class StoreImageCreationCommandHandler : CommandHandler<StoreImageCreationCommand, StoreImageCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreImageCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter): base(serviceProvider, executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        protected override async Task<StoreImageCreationStatusContract> HandleAsync(StoreImageCreationCommand command)
        {
            var storageAccountConnectionStringKey = $"Storage-Account-{executingRequestContextAdapter.GetShard().Key}-Connection-String";
            
            var blobStorageAccessor = await BlobStorageDataAccessor.CreateAsync(serviceProvider, executingRequestContextAdapter, storageAccountConnectionStringKey, StorageContainerPath.Store);

            if (command.StoreImageCreationContract.StoreImage.Length == 0)
            {
                throw new InvalidOperationException("Empty image uploaded").WithTelemetry(executingRequestContextAdapter
                    .GetTelemetryProperties());
            }

            var storeImage = command.StoreImageCreationContract.StoreImage;
            string ext = System.IO.Path.GetExtension(storeImage.FileName);
            var blobName = $"{command.StoreImageCreationContract.StoreName.Replace(" ", "-").ToLowerInvariant()}.{ext}";
            
            await using var readStream = storeImage.OpenReadStream();
            
            // this stream will be disposed in UploadBlobsAsync; do not add a using statement
            var memoryStream = new MemoryStream();
            await readStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var blobParametersModel = new BlobParametersRequestModel(StorageContainerPath.Store, blobName, storeImage.ContentType);
            var blobParametersUploadModel = new BlobParametersUploadModel(blobParametersModel, memoryStream);
            
            var imageUri = await blobStorageAccessor.UploadBlobAsync(blobParametersUploadModel);

            var storeImageCreationStatusContract = new StoreImageCreationStatusContract
            {
                ImageUri = imageUri,
                DateCreated = DateTimeOffset.UtcNow
            };

            return storeImageCreationStatusContract;
        }
    }
}