
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Database.Models;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Helpers;
using Delivery.Product.Domain.Configurations;
using Delivery.Product.Domain.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Product.Domain.CommandHandlers
{
    public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, bool>
    {
        private readonly Delivery.Domain.Configuration.AzureStorageConfig _storageConfig;
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public CreateProductCommandHandler(
            Delivery.Domain.Configuration.AzureStorageConfig config,
            IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter
            )
        {
            _storageConfig = config;
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<bool> Handle(CreateProductCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var product = new Database.Entities.Product
            {
                ProductName = command.ProductContract.ProductName,
                Description = command.ProductContract.Description,
                UnitPrice = command.ProductContract.UnitPrice,
                CategoryId = command.ProductContract.CategoryId,
                Currency = Currency.BritishPound.ToString(),
                CurrencySign = CurrencySign.BritishPound.Code,
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDateTime = DateTimeOffset.UtcNow
            };
            
            await databaseContext.Products.AddAsync(product);
            await databaseContext.SaveChangesAsync();

            // upload image
            return await UploadImages(new List<IFormFile>() {command.File}, product.Id, command.ProductContract, databaseContext);
        }
        
        private async Task<bool> UploadImages(ICollection<IFormFile> files, int productId, ProductContract productContract, PlatformDbContext databaseContext)
        {
            bool isUploaded = false;

            try
            {

                if (_storageConfig.AccountKey == string.Empty || _storageConfig.AccountName == string.Empty)
                    throw new ArgumentException("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (_storageConfig.ImageContainer == string.Empty)
                    throw new ArgumentException("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            await using var stream = formFile.OpenReadStream();
                            isUploaded = await StorageHelper.UploadFileToStorage(stream, $"{productId}-{formFile.FileName.ToLower().Replace(" ", "-")}", _storageConfig);

                            var product = databaseContext.Products.FirstOrDefault(x => x.Id == productId);
                            product!.ProductImage = $"{productId}-{formFile.FileName.ToLower().Replace(" ", "-")}";
                            product.ProductImageUrl = $"{"ulr"}{product.ProductImage}";
                            await databaseContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        throw new UnsupportedContentTypeException("Only image file is allowed to upload.");
                    }
                }
                return isUploaded;
            }
            catch(Exception ex)
            {
                var errorMessage = "Error occurred in uploading image";
                throw new Exception(errorMessage + ex.Message);
            }
        }
    }
}