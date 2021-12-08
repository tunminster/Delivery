using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Database.Constants;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.Supports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    [Route("api/v1/management/support-management" , Name = "9 - Support management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = RoleConstant.ShopOwner)]
    public class SupportManagementController : Controller
    {
        
        private readonly IServiceProvider serviceProvider;
        
        public SupportManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Request support
        /// </summary>
        /// <returns></returns>
        [Route("request-support", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post_CreateSupportAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] SupportCreationContract supportCreationContract, IFormFile? documenet)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            await Task.CompletedTask;
            
            return Ok(new StatusContract());
        }
    }
}