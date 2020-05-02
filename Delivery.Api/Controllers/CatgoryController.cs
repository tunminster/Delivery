using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Delivery.Api.Helpers;
using Delivery.Api.Data;
using Delivery.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static Delivery.Api.Extensions.HttpResults;
using System.Threading;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _appDbContext;

        public CategoryController(ILogger<UserController> logger, 
        ApplicationDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        [HttpGet("/getAllCategories")]
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

        [HttpGet("/getAllCategoriesByParentId/{parentId}")]        
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoriesByParentId(int parentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _appDbContext.Categories.Where(x => x.ParentCategoryId == parentId)
                            .ToListAsync(cancellationToken);
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
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult>GetCategoryById(int id)
        {
            try
            {
                var result = await _appDbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);

                return Ok(result);
            }
            catch(Exception ex)
            {
                var errorMessage = "Fetching category by id";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }


        [HttpPost("Create")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory(Category category)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _appDbContext.Categories.AddAsync(category);
                return CreatedAtAction(nameof(GetCategoryById), new {id = category.Id}, category);
            }
            catch(Exception ex)
            {
                var errorMessage = "Error occurred in creating category";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }

    }
}
