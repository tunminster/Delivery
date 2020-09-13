using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Domain.Query;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.Models.Dto;
using Delivery.Api.QueryHandler;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Product.Domain.CommandHandlers;
using Delivery.Product.Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static Delivery.Api.Extensions.HttpResults;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly AzureStorageConfig storageConfig = null;
        private readonly IQueryHandler<ProductGetAllQuery, ProductDto[]> _queryProductGetAllQuery;
        private readonly ICommandHandler<CreateProductCommand, bool> _createProductCommandHandler;

        public ProductController(ILogger<UserController> logger,
        ApplicationDbContext appDbContext,
        IMapper mapper,
        IOptions<AzureStorageConfig> config,
        IQueryHandler<ProductGetAllQuery, ProductDto[]> queryProductGetAllQuery,
        ICommandHandler<CreateProductCommand, bool> createProductCommandHandler)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _mapper = mapper;
            storageConfig = config.Value;
            _queryProductGetAllQuery = queryProductGetAllQuery;
            _createProductCommandHandler = createProductCommandHandler;
        }

        [HttpGet("getAllProducts")]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _queryProductGetAllQuery.Handle(new ProductGetAllQuery()); 
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorMessage = "Fetching Product list";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpGet("GetProductById/{id}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var result = await _appDbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
                var productDto = _mapper.Map<ProductDto>(result);
                return Ok(productDto);
            }
            catch (Exception ex)
            {
                var errorMessage = "Fetching product by id";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpGet("GetProductByCategoryId/{id}")]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductByCategoryId(int id)
        {
            try
            {
                var result = await _appDbContext.Products.Where(x => x.CategoryId == id).ToListAsync();
                var productDtoList = _mapper.Map<List<ProductDto>>(result);
                return Ok(productDtoList);
            }
            catch (Exception ex)
            {
                var errorMessage = "Fetching product by categoryid.";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpPost("Create")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProduct(ProductDto productDto, IFormFile file)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var product = _mapper.Map<Database.Entities.Product>(productDto);
                await _appDbContext.Products.AddAsync(product);
                await _appDbContext.SaveChangesAsync();

                productDto.Id = product.Id;

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, productDto);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in creating product";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        /// <summary>
        /// Create product with image upload
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("CreateProduct")]
        //[Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProductWithString([FromForm]  ProductContract productContract, IFormFile file)
        {

            //var productDto = JsonConvert.DeserializeObject<ProductDto>(jsonString);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _createProductCommandHandler.Handle(new CreateProductCommand(productContract, file)))
            {
                return Ok();
            }

            return NotFound("Error occurred in creating product");
        }
        
        // GET /api/images/thumbnails
        
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {
            try
            {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);
                return new ObjectResult(thumbnailUrls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDto productDto)
        {
            if(id != productDto.Id)
            {
                return BadRequest();
            }

            var result = await _appDbContext.Products.FindAsync(id);
            if(result == null)
            {
                return NotFound();
            }

            result.ProductName = productDto.ProductName;
            result.Description = productDto.Description;
            result.ProductImage = productDto.ProductImage;
            result.UnitPrice = decimal.Parse(productDto.UnitPrice);
            result.CategoryId = productDto.CategoryId;
            result.ProductImageUrl = productDto.ProductImageUrl;
            
            _appDbContext.Entry(result).State = EntityState.Modified;
            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in updating product";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }

            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _appDbContext.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }

            _appDbContext.Products.Remove(product);
            await _appDbContext.SaveChangesAsync();

            return NoContent();

        }

    }
}