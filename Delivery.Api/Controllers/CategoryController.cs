using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static Delivery.Api.Extensions.HttpResults;
using System.Threading;
using Delivery.Api.Models.Dto;
using Delivery.Api.Domain.Query;
using Delivery.Api.QueryHandler;
using Delivery.Database.Context;
using Delivery.Database.Entities;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _appDbContext;
        private readonly IQueryHandler<CategoryByIdQuery, CategoryDto> _queryCategoryByIdQuery;

        public CategoryController(
            ILogger<UserController> logger, 
            ApplicationDbContext appDbContext,
            IQueryHandler<CategoryByIdQuery, CategoryDto> queryCategoryByIdQuery)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _queryCategoryByIdQuery = queryCategoryByIdQuery;
        }

        [HttpGet("getAllCategories")]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _appDbContext.Categories.ToListAsync(cancellationToken);
                return Ok(result);
            }
            catch(Exception ex)
            {
                var errorMessage = "Fetching Category list";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpGet("getAllCategoriesByParentId/{parentId}")]        
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoriesByParentId(int parentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _appDbContext.Categories.Where(x => x.ParentCategoryId == parentId)
                            .ToListAsync(cancellationToken);
                _logger.LogInformation(string.Concat("GetCategoriesByParentId: ",parentId, " called."));
                return Ok(result);
            }
            catch(Exception ex)
            {
                var errorMessage = "Fetching Category list by parentId";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }

        }

        [HttpGet("GetCategoryById/{id}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult>GetCategoryById(int id)
        {
            try
            {
                var categoryByIdQuery = new CategoryByIdQuery();
                categoryByIdQuery.CategoryId = id;

                var result = await _queryCategoryByIdQuery.Handle(categoryByIdQuery);

                return Ok(result);
            }
            catch(Exception ex)
            {
                var errorMessage = "Fetching category by id";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }


        [HttpPost("create")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory(CategoryDto categoryDto)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var category = new Category();
                category.Id = categoryDto.Id;
                category.CategoryName = categoryDto.CategoryName;
                category.Description = categoryDto.Description;
                category.ParentCategoryId = categoryDto.ParentCategoryId;
                category.Order = categoryDto.Order;

                var result = await _appDbContext.Categories.AddAsync(category);
                await _appDbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCategoryById), new {id = category.Id}, category);
            }
            catch(Exception ex)
            {
                var errorMessage = "Error occurred in creating category";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpPut("update/{id}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutCategory(int id, CategoryDto categorydto)
        {
            if (id != categorydto.Id)
            {
                return BadRequest();
            }

            var result = await _appDbContext.Categories.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            result.CategoryName = categorydto.CategoryName;
            result.Description = categorydto.Description;
            result.Order = categorydto.Order;

            _appDbContext.Entry(result).State = EntityState.Modified;
            try
            {
                await _appDbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in updating category";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }

            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _appDbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _appDbContext.Categories.Remove(category);
            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}
