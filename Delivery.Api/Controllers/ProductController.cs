using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.Models.Dto;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.CommandHandlers;
using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.QueryHandlers;
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
        public async Task<IActionResult> GetProductByIdAsync(int id)
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
        public async Task<IActionResult> GetProductByCategoryIdAsync(int id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var productByCategoryIdQueryHandler =
                new ProductByCategoryIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            var productByCategoryIdQuery = new ProductByCategoryIdQuery(id);
            var productContractList = await productByCategoryIdQueryHandler.Handle(productByCategoryIdQuery);
            return Ok(productContractList);
        }

        [HttpPost("Create")]
        [ProducesResponseType(typeof(ProductContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddProductAsync(ProductContract productContract, IFormFile file)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createProductCommandHandler =
                new CreateProductCommandHandler(storageConfig, serviceProvider, executingRequestContextAdapter);

            var createProductCommand = new CreateProductCommand(productContract, file);
            var isCreatedProduct = await createProductCommandHandler.Handle(createProductCommand);
            return Ok(isCreatedProduct);
        }

        /// <summary>
        /// Create product with image upload
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("CreateProduct")]
        //[Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddProductWithStringAsync([FromForm]  ProductContract productContract, IFormFile file)
        {

            //var productDto = JsonConvert.DeserializeObject<ProductDto>(jsonString);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var createProductCommandHandler =
                new CreateProductCommandHandler(storageConfig, serviceProvider, executingRequestContextAdapter); 

            if (await createProductCommandHandler.Handle(new CreateProductCommand(productContract, file)))
            {
                return Ok();
            }

            return NotFound("Error occurred in creating product");
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
        public async Task<IActionResult> PutProductAsync(int id, ProductContract productContract, IFormFile file)
        {
            if(id != productContract.Id)
            {
                return BadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var productByIdQueryHandler = new ProductByIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            
            var productByIdQuery = new ProductByIdQuery(productContract.Id);
            var product = await productByIdQueryHandler.Handle(productByIdQuery);
            if(product == null)
            {
                return NotFound();
            }

            product.ProductName = productContract.ProductName;
            product.Description = productContract.Description;
            product.ProductImage = productContract.ProductImage;
            product.UnitPrice = productContract.UnitPrice;
            product.CategoryId = productContract.CategoryId;
            product.ProductImageUrl = productContract.ProductImageUrl;

            var productUpdateCommandHandler =
                new ProductUpdateCommandHandler(storageConfig, serviceProvider, executingRequestContextAdapter);
            
            var productUpdateCommand = new ProductUpdateCommand(product, file);
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

    }
}