using System;
using System.IO;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Blob.Accessors;
using Delivery.Azure.Library.Storage.Blob.Models;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Constants;
using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductImageCreations;

namespace Delivery.Product.Domain.Handlers.CommandHandlers.ProductImageCreation
{
    public class ProductImageCreationCommandHandler : ICommandHandler<ProductImageCreationCommand, ProductImageCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ProductImageCreationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ProductImageCreationStatusContract> Handle(ProductImageCreationCommand command)
        {
            var storageAccountConnectionStringKey = $"Storage-Account-{executingRequestContextAdapter.GetShard().Key}-Connection-String";
            
            var blobStorageAccessor = await BlobStorageDataAccessor.CreateAsync(serviceProvider, executingRequestContextAdapter, storageAccountConnectionStringKey, StorageContainerPath.Product);
            
            if (command.ProductImageCreationContract.ProductImage.Length == 0)
            {
                throw new InvalidOperationException("Empty image uploaded").WithTelemetry(executingRequestContextAdapter
                    .GetTelemetryProperties());
            }
            
            var productImage = command.ProductImageCreationContract.ProductImage;
            string ext = System.IO.Path.GetExtension(productImage.FileName);
            var formattedImageName = $"{command.ProductImageCreationContract.ProductName.Replace(" ", "-").ToLowerInvariant()}{ext}";

            var blobName = $"{executingRequestContextAdapter.GetShard().Key.ToLowerInvariant()}/{formattedImageName}";
            
            await using var readStream = productImage.OpenReadStream();
            
            // this stream will be disposed in UploadBlobsAsync; do not add a using statement
            var memoryStream = new MemoryStream();
            await readStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            var blobParametersModel = new BlobParametersRequestModel($"{executingRequestContextAdapter.GetShard().Key.ToLowerInvariant()}", blobName, productImage.ContentType);
            
            var blobParametersUploadModel = new BlobParametersUploadModel(blobParametersModel, memoryStream);
            
            var imageUri = await blobStorageAccessor.UploadBlobAsync(blobParametersUploadModel);

            var productImageCreationStatusContract = new ProductImageCreationStatusContract
            {
                ImageUri = imageUri,
                FileName =  formattedImageName,
                DateCreated = DateTimeOffset.UtcNow
            };

            return productImageCreationStatusContract;
        }
    }
}