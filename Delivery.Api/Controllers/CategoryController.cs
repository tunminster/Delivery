using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Web.Http.Results;
using Delivery.Api.Models.Dto;
using Delivery.Category.Domain.CommandHandlers;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.QueryHandlers;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IQueryHandler<CategoryByIdQuery, CategoryContract> _queryCategoryByIdQuery;
        private readonly IQueryHandler<CategoryGetAllQuery, List<CategoryContract>> _categoryGetAllQuery;
        private readonly IQueryHandler<CategoryByParentIdQuery, List<CategoryContract>> _categoryByParentIdQuery;

        public CategoryController(
            ILogger<UserController> logger, 
            IQueryHandler<CategoryByIdQuery, CategoryContract> queryCategoryByIdQuery,
            IQueryHandler<CategoryGetAllQuery, List<CategoryContract>> categoryGetAllQuery,
            IQueryHandler<CategoryByParentIdQuery, List<CategoryContract>> categoryByParentIdQuery)
        {
            _logger = logger;
            _queryCategoryByIdQuery = queryCategoryByIdQuery;
            _categoryGetAllQuery = categoryGetAllQuery;
            _categoryByParentIdQuery = categoryByParentIdQuery;
        }

        [HttpGet("getAllCategories")]
        [ProducesResponseType(typeof(List<CategoryContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var categoryGetAllQuery = new CategoryGetAllQuery();
            var result = await _categoryGetAllQuery.Handle(categoryGetAllQuery);
            return Ok(result);
        }

        [HttpGet("getAllCategoriesByParentId/{parentId}")]        
        [ProducesResponseType(typeof(List<Database.Entities.Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoriesByParentId(int parentId, CancellationToken cancellationToken = default)
        {
            var categoryByParentIdQuery = new CategoryByParentIdQuery();
            var result = await _categoryByParentIdQuery.Handle(categoryByParentIdQuery);
            return Ok(result);
        }

        [HttpGet("GetCategoryById/{id}")]
        [ProducesResponseType(typeof(CategoryContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult>GetCategoryById(int id)
        {

            var categoryByIdQuery = new CategoryByIdQuery {CategoryId = id};

            var result = await _queryCategoryByIdQuery.Handle(categoryByIdQuery);

            return Ok(result);
        }


        [HttpPost("create")]
        [ProducesResponseType(typeof(CategoryContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory(CategoryContract categoryContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var categoryCreationCommand = new CategoryCreationCommand(categoryContract);
            var categoryCreationCommandHandler = new CategoryCreationCommandHandler();
            var categoryCreationStatusContract = await categoryCreationCommandHandler.Handle(categoryCreationCommand);
            
            return Ok(categoryCreationStatusContract);
        }

        [HttpPut("update/{id}")]
        [ProducesResponseType(typeof(CategoryContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutCategory(int id, CategoryContract categoryContract)
        {
            if (id != categoryContract.Id)
            {
                return BadRequest();
            }
            
            var categoryUpdateCommand = new CategoryUpdateCommand(categoryContract);
            var categoryUpdateCommandHandler = new CategoryUpdateCommandHandler();
            var categoryUpdateStatusContract = await categoryUpdateCommandHandler.Handle(categoryUpdateCommand);

            return Ok(categoryUpdateStatusContract);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {

            if (id < 1)
            {
                return BadRequest();
            }
            
            var categoryDeleteCommand = new CategoryDeleteCommand(id);
            var categoryDeleteCommandHandler = new CategoryDeleteCommandHandler();
            var categoryDeleteStatusContract = await  categoryDeleteCommandHandler.Handle(categoryDeleteCommand);

            return Ok(categoryDeleteStatusContract);
        }

    }
}
