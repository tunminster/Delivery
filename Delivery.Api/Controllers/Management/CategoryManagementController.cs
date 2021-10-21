using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;
using Delivery.Category.Domain.QueryHandlers;
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
    }
}