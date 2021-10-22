using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Category.Domain.CommandHandlers;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;
using Delivery.Category.Domain.Contracts.V1.RestContracts;
using Delivery.Category.Domain.QueryHandlers;
using Delivery.Category.Domain.Validators.CategoryCreation;
using Delivery.Domain.FrameWork.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Management user controller
    /// </summary>
    [Route("api/v1/category-management", Name = "6 - Management Category")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = "ShopOwner,Administrator")]
    public class CategoryManagementController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public CategoryManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Get all categories
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("get-all-categories", Order = 1)]
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var result = await new CategoryGetAllByUserQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new CategoryGetAllByUserQuery { Email = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected user email.")});
            return Ok(result);
        }
        
        /// <summary>
        ///  Create a category
        /// </summary>
        /// <remark>Create a category</remark>
        [Route("create-category", Order = 2)]
        [HttpPost]
        [ProducesResponseType(typeof(CategoryCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddCategoryAsync(CategoryCreationContract categoryCreationContract)
        {
            var validationResult = await new CategoryCreationValidator().ValidateAsync(categoryCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
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
        /// <remark>Update category</remark>
        [Route("update-category/{id}", Order = 5)]
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
        /// <remark>Delete category</remark>
        [Route("delete-category/{id}/delete", Order = 6)]
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