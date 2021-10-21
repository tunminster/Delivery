using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Category.Domain.CommandHandlers;
using Delivery.Category.Domain.QueryHandlers;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Category api
    /// </summary>
    [Route("api/[controller]", Name = "3 - Category")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    [ApiController]
    //[Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public CategoryController(
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
            
        }

        /// <summary>
        ///  Get all categories
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("getAllCategories", Order = 1)]
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var categoryGetAllQueryHandler =
                new CategoryGetAllQueryHandler(serviceProvider, executingRequestContextAdapter);
            var categoryGetAllQuery = new CategoryGetAllQuery();
            var result = await categoryGetAllQueryHandler.Handle(categoryGetAllQuery);
            return Ok(result);
        }

        /// <summary>
        ///  Get categories by parent id
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("getAllCategoriesByParentId/{parentId}", Order = 2)]
        [HttpGet]        
        [ProducesResponseType(typeof(List<Database.Entities.Category>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCategoriesByParentIdAsync(string parentId, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var categoryByParentIdQuery = new CategoryByParentIdQuery(parentId);
            var categoryByParentIdQueryHandler =
                new CategoryByParentIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            
            var result = await categoryByParentIdQueryHandler.Handle(categoryByParentIdQuery);
            return Ok(result);
        }

        /// <summary>
        ///  Get category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("GetCategoryById/{id}", Order = 3)]
        [HttpGet]
        [ProducesResponseType(typeof(CategoryContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult>GetCategoryByIdAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var categoryByIdQuery = new CategoryByIdQuery {CategoryId = id};
            var queryCategoryByIdQuery = new CategoryByIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            var result = await queryCategoryByIdQuery.Handle(categoryByIdQuery);

            return Ok(result);
        }


        /// <summary>
        ///  Create a category
        /// </summary>
        /// <param name="categoryCreationContract"></param>
        /// <returns></returns>
        [Route("create", Order = 4)]
        [HttpPost]
        [ProducesResponseType(typeof(CategoryCreationContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddCategoryAsync(CategoryCreationContract categoryCreationContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var categoryCreationCommandHandler =
                new CategoryCreationCommandHandler(serviceProvider, executingRequestContextAdapter);
            
            var categoryCreationCommand = new CategoryCreationCommand(categoryCreationContract);
            var categoryCreationStatusContract = await categoryCreationCommandHandler.Handle(categoryCreationCommand);
            
            return Ok(categoryCreationStatusContract);
        }

        /// <summary>
        ///  Update a category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="categoryCreationContract"></param>
        /// <returns></returns>
        [Route("update/{id}", Order = 5)]
        [HttpPut]
        [ProducesResponseType(typeof(CategoryCreationContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PutCategoryAsync(string id, CategoryCreationContract categoryCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var categoryUpdateCommand = new CategoryUpdateCommand(categoryCreationContract, id);
            var categoryUpdateCommandHandler =
                new CategoryUpdateCommandHandler(serviceProvider, executingRequestContextAdapter);
            
            var categoryUpdateStatusContract = await categoryUpdateCommandHandler.Handle(categoryUpdateCommand);

            return Ok(categoryUpdateStatusContract);
        }

        /// <summary>
        ///  Delete a category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("delete/{id}", Order = 6)]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategoryAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var categoryDeleteCommand = new CategoryDeleteCommand(id);
            var categoryDeleteCommandHandler =
                new CategoryDeleteCommandHandler(serviceProvider, executingRequestContextAdapter);
            var categoryDeleteStatusContract = await  categoryDeleteCommandHandler.Handle(categoryDeleteCommand);

            return Ok(categoryDeleteStatusContract);
        }

    }
}
