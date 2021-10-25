using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Domain.FrameWork.Context;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;
using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductImageCreations;
using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement;
using Delivery.Product.Domain.Handlers.CommandHandlers;
using Delivery.Product.Domain.Handlers.CommandHandlers.ProductImageCreation;
using Delivery.Product.Domain.Handlers.QueryHandlers;
using Delivery.Product.Domain.Validators.ProductCreation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Product management
    /// </summary>
    /// <remarks>Product management</remarks>
    [Route("api/v1/product-management", Name = "7 - Management Product")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = "ShopOwner,Administrator")]
    public class ProductManagementController : ControllerBase
    {
        private readonly Delivery.Domain.Configuration.AzureStorageConfig storageConfig = null;

        private readonly IServiceProvider serviceProvider;

        public ProductManagementController(
            IOptions<Delivery.Domain.Configuration.AzureStorageConfig> config,
            IServiceProvider serviceProvider)
        {
            storageConfig = config.Value;
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Get all products
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("get-all-products", Order = 1)]
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var productManagementGetAllQueryHandler = new ProductManagementGetAllQueryHandler(serviceProvider, executingRequestContextAdapter);
            var productContractList = await productManagementGetAllQueryHandler.Handle(new ProductManagementGetAllQuery()); 
            return Ok(productContractList);
        }
        
        /// <summary>
        /// Create product with image upload
        /// </summary>
        /// <returns></returns>
        [Route("create-product", Order = 2)]
        [HttpPost]
        [ProducesResponseType(typeof(ProductManagementCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddProduct_Async([ModelBinder(BinderType = typeof(JsonModelBinder))]  ProductManagementCreationContract productManagementCreationContract, IFormFile? productImage)
        {
            var validationResult = await new ProductCreationValidator().ValidateAsync(productManagementCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var productCreationStatusContract = new ProductManagementCreationStatusContract
            {
                ProductId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                DateCreated = DateTimeOffset.UtcNow
            };

            var productImageCreationStatusContract = new ProductImageCreationStatusContract();
            
            if (productImage != null)
            {
                productImageCreationStatusContract = await
                    UploadProductImageAsync(productCreationStatusContract.ProductId, productManagementCreationContract.ProductName, productImage,
                        executingRequestContextAdapter);
            }

            productManagementCreationContract.ProductImage = productImageCreationStatusContract.FileName;
            productManagementCreationContract.ProductImageUrl = productImageCreationStatusContract.ImageUri;

            var createProductManagementCommand = new CreateProductManagementCommand(productManagementCreationContract, productCreationStatusContract);

            var result =
                await new CreateProductManagementCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(createProductManagementCommand);
            
            return Ok(result);
        }
        
        private async Task<ProductImageCreationStatusContract> UploadProductImageAsync(string productId, string productName, IFormFile productImage, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            var productImageCreationContract = new ProductImageCreationContract
            {
                ProductId = productId,
                ProductImage = productImage,
                ProductName = productName
            };

            var productImageCreationCommand = new ProductImageCreationCommand(productImageCreationContract);

            var productImageCreationStatusContract =
                await new ProductImageCreationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(productImageCreationCommand);

            return productImageCreationStatusContract;
        }
    }
}