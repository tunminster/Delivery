using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Blob.Accessors;
using Delivery.Azure.Library.Storage.Blob.Models;
using Delivery.Database.Context;
using Delivery.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Driver.Domain.Services
{ 
    public class DriverService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<DriverImageCreationStatusContract> UploadDriverImagesAsync(
            DriverImageCreationContract driverImageCreationContract)
        {
            var storageAccountConnectionStringKey = $"Storage-Account-{executingRequestContextAdapter.GetShard().Key}-Connection-String";
            
            var blobStorageAccessor = await BlobStorageDataAccessor.CreateAsync(serviceProvider, executingRequestContextAdapter, storageAccountConnectionStringKey, StorageContainerPath.Driver);
            string driverImageUri = string.Empty;
            string drivingLicenseFrontImageUri = string.Empty;
            string drivingLicenseBackImageUri = string.Empty;
            
            var driverImage = driverImageCreationContract.DriverImage;
            var drivingLicenseFrontImage = driverImageCreationContract.DrivingLicenseFrontImage;
            var drivingLicenseBackImage = driverImageCreationContract.DrivingLicenseBackImage;

            if (driverImage != null)
            {
                driverImageUri = await UploadBlobAsync(driverImage, 
                    $"{driverImageCreationContract.DriverName.Replace(" ", "-").ToLowerInvariant()}_profile.{Path.GetExtension(driverImage.FileName)}", 
                    $"{driverImageCreationContract.DriverName.Replace(" ", "-").ToLowerInvariant()}",
                    blobStorageAccessor);
            }

            if (drivingLicenseFrontImage != null)
            {
                drivingLicenseFrontImageUri = await UploadBlobAsync(drivingLicenseFrontImage, 
                    $"{driverImageCreationContract.DriverName.Replace(" ", "-").ToLowerInvariant()}_front_license.{Path.GetExtension(drivingLicenseFrontImage.FileName)}",
                    $"{driverImageCreationContract.DriverName.Replace(" ", "-").ToLowerInvariant()}",
                    blobStorageAccessor);
            }

            if (drivingLicenseBackImage != null)
            {
                drivingLicenseBackImageUri = await UploadBlobAsync(drivingLicenseBackImage,
                    $"{driverImageCreationContract.DriverName.Replace(" ", "-").ToLowerInvariant()}_front_license.{Path.GetExtension(drivingLicenseBackImage.FileName)}",
                            $"{driverImageCreationContract.DriverName.Replace(" ", "-").ToLowerInvariant()}",
                    blobStorageAccessor);
            }

            var driverImageCreationStatusContract = new DriverImageCreationStatusContract
            {
                DriverImageUri = driverImageUri,
                DrivingLicenseFrontImageUri = drivingLicenseFrontImageUri,
                DrivingLicenseBackImageUri = drivingLicenseBackImageUri
            };

            return driverImageCreationStatusContract;
        }

        public async Task<bool> IsDriverApprovedAsync(string emailAddress)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driver = databaseContext.Drivers.FirstOrDefault(x => x.EmailAddress == emailAddress && x.Approved);

            if (driver is null)
            {
                return false;
            }

            return true;
        }

        private async Task<string> UploadBlobAsync(IFormFile image, string blobName, string driverName, BlobStorageDataAccessor blobStorageAccessor)
        {
            await using var readStream = image.OpenReadStream();
            
            // this stream will be disposed in UploadBlobsAsync; do not add a using statement
            var memoryStream = new MemoryStream();
            await readStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var blobParametersModel = new BlobParametersRequestModel($"{StorageContainerPath.Driver}/{driverName}", blobName, image.ContentType);
            var blobParametersUploadModel = new BlobParametersUploadModel(blobParametersModel, memoryStream);
            
            var imageUri = await blobStorageAccessor.UploadBlobAsync(blobParametersUploadModel);

            return imageUri;
        }
    }
}