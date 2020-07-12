using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Query;
using Delivery.Api.Entities;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.Models.Dto;
using Delivery.Api.QueryHandler;
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

        public ProductController(ILogger<UserController> logger,
        ApplicationDbContext appDbContext,
        IMapper mapper,
        IOptions<AzureStorageConfig> config,
        IQueryHandler<ProductGetAllQuery, ProductDto[]> queryProductGetAllQuery)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _mapper = mapper;
            storageConfig = config.Value;
            _queryProductGetAllQuery = queryProductGetAllQuery;
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
                var product = _mapper.Map<Product>(productDto);
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
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProductWithString([FromForm] string jsonString, IFormFile file)
        {

            var productDto = JsonConvert.DeserializeObject<ProductDto>(jsonString);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var product = _mapper.Map<Product>(productDto);
                await _appDbContext.Products.AddAsync(product);
                await _appDbContext.SaveChangesAsync();

                productDto.Id = product.Id;
                
                // upload image
                await UploadImages(new List<IFormFile>() {file}, productDto.Id, productDto);
                

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, productDto);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in creating product";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadImage(ICollection<IFormFile> files)
        {
            var productdto = new ProductDto();
            if (files.Count > 0)
            {
                return await UploadImages(files);
            }

            return InternalServerErrorResult("error");
        }

        private async Task<IActionResult> UploadImages(ICollection<IFormFile> files, int productId, ProductDto productDto)
        {
            bool isUploaded = false;

            try
            {

                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                //isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    if (storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();

                }
                else
                {
                    return BadRequest("Look like the image couldnt upload to the storage");
                }
            }
            catch(Exception ex)
            {
                var errorMessage = "Error occurred in uploading image";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        //Todo
        private async Task<IActionResult> UploadImage(IFormFile file, int productId)
        {
            bool isUploaded = false;
            
            try
            {

                if (file != null)
                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                
                    if (StorageHelper.IsImage(file))
                    {
                        if (file.Length > 0)
                        {
                            using (Stream stream = file.OpenReadStream())
                            {
                                //isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, file.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                

                if (isUploaded)
                {
                    if (storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();

                }
                else
                {
                    return BadRequest("Look like the image couldnt upload to the storage");
                }
            }
            catch(Exception ex)
            {
                var errorMessage = "Error occurred in uploading image";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
            
        }

        private async Task<IActionResult> UploadImages(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {

                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    if (storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();

                }
                else
                {
                    return BadRequest("Look like the image couldnt upload to the storage");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in uploading image";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
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