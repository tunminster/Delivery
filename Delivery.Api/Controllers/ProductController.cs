using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.Helpers;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Domain.FrameWork.Context;
using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;
using Delivery.Product.Domain.Contracts.V1.RestContracts;
using Delivery.Product.Domain.Contracts.V1.RestContracts.ProductImageCreations;
using Delivery.Product.Domain.Handlers.CommandHandlers;
using Delivery.Product.Domain.Handlers.CommandHandlers.ProductImageCreation;
using Delivery.Product.Domain.Handlers.QueryHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiUser")]
    //[Authorize]
    public class ProductController : ControllerBase
    {
        private readonly Delivery.Domain.Configuration.AzureStorageConfig storageConfig = null;

        private readonly IServiceProvider serviceProvider;

        public ProductController(
        IOptions<Delivery.Domain.Configuration.AzureStorageConfig> config,
        IServiceProvider serviceProvider)
        {
            storageConfig = config.Value;
            this.serviceProvider = serviceProvider;
        }

        [HttpGet("getAllProducts")]
        [ProducesResponseType(typeof(List<ProductContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var productGetAllQueryHandler = new ProductGetAllQueryHandler(serviceProvider, executingRequestContextAdapter);
            var productContractList = await productGetAllQueryHandler.Handle(new ProductGetAllQuery()); 
            return Ok(productContractList);
        }

        [HttpGet("GetProductById/{id}")]
        [ProducesResponseType(typeof(ProductContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetProductByIdAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var productByIdQuery = new ProductByIdQuery(id);
            var queryProductByIdQuery = new ProductByIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            var productContract = await queryProductByIdQuery.Handle(productByIdQuery);
            
            return Ok(productContract);
        }

        [HttpGet("GetProductByCategoryId/{id}")]
        [ProducesResponseType(typeof(List<ProductContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetProductByCategoryIdAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var productByCategoryIdQueryHandler =
                new ProductByCategoryIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            var productByCategoryIdQuery = new ProductByCategoryIdQuery(id);
            var productContractList = await productByCategoryIdQueryHandler.Handle(productByCategoryIdQuery);
            return Ok(productContractList);
        }

        // [HttpPost("Create")]
        // [ProducesResponseType(typeof(ProductCreationContract), (int)HttpStatusCode.OK)]
        // [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        // public async Task<IActionResult> AddProductAsync(ProductCreationContract productCreationContract, IFormFile file)
        // {
        //     var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
        //     
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //
        //     var createProductCommandHandler =
        //         new CreateProductCommandHandler(storageConfig, serviceProvider, executingRequestContextAdapter);
        //
        //     var createProductCommand = new CreateProductCommand(productCreationContract, file);
        //     var isCreatedProduct = await createProductCommandHandler.Handle(createProductCommand);
        //     return Ok(isCreatedProduct);
        // }

        /// <summary>
        /// Create product with image upload
        /// </summary>
        /// <param name="productCreationContract"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("CreateProduct")]
        [ProducesResponseType(typeof(ProductCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddProductAsync([ModelBinder(BinderType = typeof(JsonModelBinder))]  ProductCreationContract productCreationContract, IFormFile productImage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var productCreationStatusContract = new ProductCreationStatusContract
            {
                ProductId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            var productImageCreationStatusContract = new ProductImageCreationStatusContract();
            
            if (productImage != null)
            {
                productImageCreationStatusContract = await
                    UploadProductImageAsync(productCreationStatusContract.ProductId, productCreationContract.ProductName, productImage,
                        executingRequestContextAdapter);
            }

            productCreationContract.ProductImage = productImageCreationStatusContract.FileName;
            productCreationContract.ProductImageUrl = productImageCreationStatusContract.ImageUri;

            var createProductCommand = new CreateProductCommand(productCreationContract, productCreationStatusContract.ProductId);

            var result =
                await new CreateProductCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(createProductCommand);
            
            return Ok(result);
        }
        
        // GET /api/images/thumbnails
        
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNailsAsync()
        {
            if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

            if (storageConfig.ImageContainer == string.Empty)
                return BadRequest("Please provide a name for your image container in Azure blob storage.");

            List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);
            return new ObjectResult(thumbnailUrls);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutProductAsync(ProductUpdateContract productUpdateContract, IFormFile productImage)
        {
            if(string.IsNullOrEmpty(productUpdateContract.ProductId))
            {
                return BadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var productByIdQueryHandler = new ProductByIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            
            var productByIdQuery = new ProductByIdQuery(productUpdateContract.ProductId);
            var productContract = await productByIdQueryHandler.Handle(productByIdQuery);
            if(productContract == null)
            {
                return NotFound();
            }
            
            var productUpdateStatusContract = new ProductUpdateStatusContract
            {
                ProductId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            var productImageCreationStatusContract = new ProductImageCreationStatusContract();
            
            if (productImage != null)
            {
                productImageCreationStatusContract = await
                    UploadProductImageAsync(productUpdateContract.ProductId, productUpdateContract.ProductName, productImage,
                        executingRequestContextAdapter);
            }

            productUpdateContract.ProductImage = productImageCreationStatusContract.FileName;
            productUpdateContract.ProductImageUrl = productImageCreationStatusContract.ImageUri;
            
            var productUpdateCommandHandler =
                new ProductUpdateCommandHandler(serviceProvider, executingRequestContextAdapter);
            
            var productUpdateCommand = new ProductUpdateCommand(productUpdateContract);
            var isProductUpdated = await productUpdateCommandHandler.Handle(productUpdateCommand);
            
            return Ok(isProductUpdated);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var productDeleteCommandHandler =
                new ProductDeleteCommandHandler(serviceProvider, executingRequestContextAdapter);
            var productDeleteCommand = new ProductDeleteCommand(id);
            var isProductDeleted = await productDeleteCommandHandler.Handle(productDeleteCommand);
            
            return Ok(isProductDeleted);
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