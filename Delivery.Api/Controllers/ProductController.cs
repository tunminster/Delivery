using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.Models.Dto;
using Delivery.Domain.CommandHandlers;
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
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly AzureStorageConfig storageConfig = null;
        private readonly IQueryHandler<ProductGetAllQuery, List<ProductContract>> queryProductGetAllQuery;
        private readonly ICommandHandler<CreateProductCommand, bool> createProductCommandHandler;
        private readonly IQueryHandler<ProductByIdQuery, ProductContract> queryProductByIdQuery;
        private readonly IQueryHandler<ProductByCategoryIdQuery, List<ProductContract>> queryProductByCategoryIdQuery;
        private readonly ICommandHandler<ProductUpdateCommand, bool> productUpdateCommandHandler;
        private readonly ICommandHandler<ProductDeleteCommand, bool> productDeleteCommandHandler;

        public ProductController(
        IOptions<AzureStorageConfig> config,
        IQueryHandler<ProductGetAllQuery, List<ProductContract>> queryProductGetAllQuery,
        ICommandHandler<CreateProductCommand, bool> createProductCommandHandler,
        IQueryHandler<ProductByIdQuery, ProductContract> queryProductByIdQuery,
        IQueryHandler<ProductByCategoryIdQuery, List<ProductContract>> queryProductByCategoryIdQuery,
        ICommandHandler<ProductUpdateCommand, bool> productUpdateCommandHandler,
        ICommandHandler<ProductDeleteCommand, bool> productDeleteCommandHandler)
        {
            storageConfig = config.Value;
            this.queryProductGetAllQuery = queryProductGetAllQuery;
            this.createProductCommandHandler = createProductCommandHandler;
            this.queryProductByIdQuery = queryProductByIdQuery;
            this.queryProductByCategoryIdQuery = queryProductByCategoryIdQuery;
            this.productUpdateCommandHandler = productUpdateCommandHandler;
            this.productDeleteCommandHandler = productDeleteCommandHandler;
        }

        [HttpGet("getAllProducts")]
        [ProducesResponseType(typeof(List<ProductContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
        {
            var productContractList = await queryProductGetAllQuery.Handle(new ProductGetAllQuery()); 
            return Ok(productContractList);
        }

        [HttpGet("GetProductById/{id}")]
        [ProducesResponseType(typeof(ProductContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            var productByIdQuery = new ProductByIdQuery(id);
            var productContract = await queryProductByIdQuery.Handle(productByIdQuery);
            
            return Ok(productContract);
        }

        [HttpGet("GetProductByCategoryId/{id}")]
        [ProducesResponseType(typeof(List<ProductContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductByCategoryIdAsync(int id)
        {
            var productByCategoryIdQuery = new ProductByCategoryIdQuery(id);
            var productContractList = await queryProductByCategoryIdQuery.Handle(productByCategoryIdQuery);
            return Ok(productContractList);
        }

        [HttpPost("Create")]
        [ProducesResponseType(typeof(ProductContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProductAsync(ProductContract productContract, IFormFile file)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
        [ProducesResponseType(typeof(ProductContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProductWithStringAsync([FromForm]  ProductContract productContract, IFormFile file)
        {

            //var productDto = JsonConvert.DeserializeObject<ProductDto>(jsonString);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            var productByIdQuery = new ProductByIdQuery(productContract.Id);
            var product = await queryProductByIdQuery.Handle(productByIdQuery);
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
            
            var productUpdateCommand = new ProductUpdateCommand(product, file);
            var isProductUpdated = await productUpdateCommandHandler.Handle(productUpdateCommand);
            
            return Ok(isProductUpdated);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            var productDeleteCommand = new ProductDeleteCommand(id);
            var isProductDeleted = await productDeleteCommandHandler.Handle(productDeleteCommand);
            
            return Ok(isProductDeleted);
        }

    }
}