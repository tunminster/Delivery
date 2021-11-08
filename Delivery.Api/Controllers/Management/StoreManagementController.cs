using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Database.Constants;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopApproval;
using Delivery.Shop.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    [Route("api/v1/store-management" , Name = "2 - Store management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = RoleConstant.Administrator)]
    public class StoreManagementController : Controller
    {
        private readonly IServiceProvider serviceProvider;
        
        public StoreManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Shop approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-shop", Order = 1)]
        [ProducesResponseType(typeof(ShopApprovalStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_ApproveAsync(ShopApprovalContract shopApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new ShopApprovalValidator().ValidateAsync(shopApprovalContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var shopApprovalCommand = new ShopApprovalCommand(shopApprovalContract);
            
            var shopApprovalStatusContract = await new ShopApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                shopApprovalCommand);

            return Ok(shopApprovalStatusContract);
        }
        
        /// <summary>
        ///  Shop user approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-shop-user", Order = 1)]
        [ProducesResponseType(typeof(ShopApprovalStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_ApproveUserAsync(ShopUserApprovalContract shopUserApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new ShopUserApprovalValidator().ValidateAsync(shopUserApprovalContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var shopUserApprovalCommand = new ShopUserApprovalCommand(shopUserApprovalContract);
            
            var shopUserApprovalStatusContract = await new ShopUserApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                shopUserApprovalCommand);

            return Ok(shopUserApprovalStatusContract);
        }
    }
}