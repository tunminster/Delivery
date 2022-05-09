using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Database.Constants;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopApproval;
using Delivery.Shop.Domain.Validators;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreCreation;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreDelete;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreImageCreation;
using Delivery.Store.Domain.Handlers.CommandHandlers.StoreUpdate;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries;
using Delivery.Store.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    [Route("api/v1/management/store-management" , Name = "2 - Store management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
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
        [ProducesResponseType(typeof(StoreManagementPagedContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = RoleConstant.Administrator)]
        public async Task<IActionResult> Get_StoresAsync(string pageSize, string pageNumber)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            int.TryParse(pageSize, out var iPageSize);
            int.TryParse(pageNumber, out var iPageNumber);
        
            var storeManagementGetAllQuery =
                new StoreManagementGetAllQuery(iPageSize, iPageNumber);
            var storeManagementPagedContract =
                await new StoreManagementGetAllQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(storeManagementGetAllQuery);
            
            return Ok(storeManagementPagedContract);
        }
        
        /// <summary>
        ///  Get store
        /// </summary>
        /// <returns></returns>
        [Route("get-store", Order = 2)]
        [HttpGet]
        [ProducesResponseType(typeof(StoreContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = RoleConstant.Administrator)]
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
        [ProducesResponseType(typeof(StoreManagementContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = RoleConstant.ShopOwner)]
        public async Task<IActionResult> Get_StoresAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var storeManagementContract = await new StoreGetQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new StoreGetQuery{StoreId = string.Empty, StoreUserEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!});
            
            return Ok(storeManagementContract);
        }

        /// <summary>
        ///  Create store
        /// </summary>
        /// <remarks>
        ///     This endpoint allows user to create a new store with store user.
        /// </remarks>
        [Route("create-store", Order = 3)]
        [HttpPost]
        [ProducesResponseType(typeof(StoreCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = RoleConstant.Administrator)]
        public async Task<IActionResult> Post_CreateStoreAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] StoreCreationContract storeCreationContract, IFormFile? storeImage)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new StoreCreationValidator().ValidateAsync(storeCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var storeCreationStatusContract = new StoreCreationStatusContract
            {
                StoreId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                InsertionDateTime = DateTimeOffset.UtcNow
            };
            
            //todo: create account first here
            
            // upload image
            if (storeImage != null)
            {
                var storeImageCreationStatusContract = await
                    UploadStoreImageAsync(storeCreationStatusContract.StoreId, storeCreationContract.StoreName, storeImage,
                        executingRequestContextAdapter);

                storeCreationContract.ImageUri = storeImageCreationStatusContract.ImageUri;
            }

            await new StoreCreationCommandHandler(serviceProvider, executingRequestContextAdapter)
                .HandleAsync(new StoreCreationCommand(storeCreationContract, storeCreationStatusContract));
            
            return Ok(storeCreationStatusContract);
        }
        
        /// <summary>
        ///  Update store
        /// </summary>
        /// <remarks>
        ///     This endpoint allows user to update the store with store user.
        /// </remarks>
        [Route("update-store", Order = 4)]
        [HttpPut]
        [ProducesResponseType(typeof(StoreUpdateStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "Administrator,ShopOwner")]
        public async Task<IActionResult> Put_UpdateStoreAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] StoreUpdateContract storeUpdateContract, IFormFile? storeImage)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new StoreUpdateValidator().ValidateAsync(storeUpdateContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var storeUpdateStatusContract = new StoreUpdateStatusContract
            {
                StoreId = storeUpdateContract.StoreId,
                InsertionDateTime = DateTimeOffset.UtcNow
            };
            
            // upload image
            if (storeImage != null)
            {
                var storeImageCreationStatusContract = await
                    UploadStoreImageAsync(storeUpdateStatusContract.StoreId, storeUpdateContract.StoreName, storeImage,
                        executingRequestContextAdapter);

                storeUpdateContract.ImageUri = storeImageCreationStatusContract.ImageUri;
            }

            await new StoreUpdateCommandHandler(serviceProvider, executingRequestContextAdapter)
                .HandleAsync(new StoreUpdateCommand(storeUpdateContract, storeUpdateStatusContract));
            
            return Ok(storeUpdateStatusContract);
        }
        
        /// <summary>
        ///  Shop approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-shop", Order = 4)]
        [ProducesResponseType(typeof(ShopApprovalStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPost]
        [Authorize(Roles = RoleConstant.Administrator)]
        public async Task<IActionResult> Post_ApproveAsync(ShopApprovalContract shopApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new ShopApprovalValidator().ValidateAsync(shopApprovalContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var shopApprovalCommand = new ShopApprovalCommand(shopApprovalContract);
            
            var shopApprovalStatusContract = await new ShopApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                shopApprovalCommand);

            return Ok(shopApprovalStatusContract);
        }
        
        /// <summary>
        ///  Shop user approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-shop-user", Order = 5)]
        [ProducesResponseType(typeof(ShopApprovalStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPost]
        [Authorize(Roles = RoleConstant.Administrator)]
        public async Task<IActionResult> Post_ApproveUserAsync(ShopUserApprovalContract shopUserApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new ShopUserApprovalValidator().ValidateAsync(shopUserApprovalContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var shopUserApprovalCommand = new ShopUserApprovalCommand(shopUserApprovalContract);
            
            var shopUserApprovalStatusContract = await new ShopUserApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                shopUserApprovalCommand);

            return Ok(shopUserApprovalStatusContract);
        }
        
        /// <summary>
        ///  Delete store
        /// </summary>
        /// <remarks>
        ///     This endpoint allows user to delete the store with store user.
        /// </remarks>
        [Route("delete-store", Order = 6)]
        [HttpDelete]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = RoleConstant.Administrator)]
        public async Task<IActionResult> Delete_StoreAsync(string storeId)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            if (string.IsNullOrEmpty(storeId))
            {
                const string message = "store id must be provided";
                return message.ConvertToBadRequest();
            }

            await new StoreDeleteCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new StoreDeleteCommand(storeId));
            
            return Accepted();
        }
        
        private async Task<StoreImageCreationStatusContract> UploadStoreImageAsync(string storeId, string storeName, IFormFile storeImage, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            var storeImageCreationContract = new StoreImageCreationContract
            {
                StoreId = storeId,
                StoreImage = storeImage,
                StoreName = storeName
            };

            var storeImageCreationCommand = new StoreImageCreationCommand(storeImageCreationContract);

            var storeImageCreationStatusContract =
                await new StoreImageCreationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleCoreAsync(storeImageCreationCommand);

            return storeImageCreationStatusContract;
        }
    }
}