using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Command;
using Delivery.Api.Domain.Contract;
using Delivery.Api.Entities;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Database.Enums;
using Delivery.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Api.CommandHandler
{
    public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, bool>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly AzureStorageConfig _storageConfig;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        
        public CreateProductCommandHandler(ApplicationDbContext appDbContext,
            IMapper mapper,
            IOptions<AzureStorageConfig> config,
            ILogger<CreateProductCommandHandler> logger)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _storageConfig = config.Value;
            _logger = logger;
        }

        public async Task<bool> Handle(CreateProductCommand command)
        {

            var product = new Product
            {
                ProductName = command.ProductContract.ProductName,
                Description = command.ProductContract.Description,
                UnitPrice = command.ProductContract.UnitPrice,
                CategoryId = command.ProductContract.CategoryId,
                Currency = Currency.BritishPound.ToString(),
                CurrencySign = CurrencySign.BritishPound.Code
            };
            
            await _appDbContext.Products.AddAsync(product);
            await _appDbContext.SaveChangesAsync();

            // upload image
            return await UploadImages(new List<IFormFile>() {command.File}, product.Id, command.ProductContract);
        }
        
        private async Task<bool> UploadImages(ICollection<IFormFile> files, int productId, ProductContract productContract)
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
                            using Stream stream = formFile.OpenReadStream();
                            isUploaded = await StorageHelper.UploadFileToStorage(stream, $"{productId}-{formFile.FileName.ToLower().Replace(" ", "-")}", _storageConfig);

                            var product = _appDbContext.Products.FirstOrDefault(x => x.Id == productId);
                            product.ProductImage = $"{productId}-{formFile.FileName.ToLower().Replace(" ", "-")}";
                            product.ProductImageUrl = $"{"ulr"}{product.ProductImage}";
                            await _appDbContext.SaveChangesAsync();
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
                throw new Exception(errorMessage);
            }
        }
    }
}
