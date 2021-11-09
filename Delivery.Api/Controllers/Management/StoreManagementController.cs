using System;
using System.Collections.Generic;
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
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries;
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
        ///  Get stores list
        /// </summary>
        /// <returns></returns>
        [Route("get-stores", Order = 1)]
        [HttpGet]
        [ProducesResponseType(typeof(List<StoreContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_StoresAsync(string totalPage, string pageNumber)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            int.TryParse(totalPage, out var iNumberOfObjectPerPage);
            int.TryParse(pageNumber, out var iPageNumber);
        
            var storeGetAllQuery =
                new StoreGetAllQuery(iNumberOfObjectPerPage, iPageNumber);
            var storeContractList =
                await new StoreGetAllQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(storeGetAllQuery);
            
            return Ok(storeContractList);
        }
        
        /// <summary>
        ///  Get store
        /// </summary>
        /// <returns></returns>
        [Route("get-store", Order = 2)]
        [HttpGet]
        [ProducesResponseType(typeof(StoreContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_StoresAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var storeContract = await new StoreGetQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new StoreGetQuery{StoreId = id});
            
            return Ok(storeContract);
        }
        
        /// <summary>
        ///  Get store details
        /// </summary>
        /// <returns></returns>
        [Route("get-store-details", Order = 2)]
        [HttpGet]
        [ProducesResponseType(typeof(StoreContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = RoleConstant.ShopOwner)]
        public async Task<IActionResult> Get_StoresAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var storeContract = await new StoreGetQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new StoreGetQuery{StoreId = string.Empty, StoreUserEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!});
            
            return Ok(storeContract);
        }
        
        /// <summary>
        ///  Shop approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-shop", Order = 3)]
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
        [Route("approve-shop-user", Order = 4)]
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