using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopActive;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopProfile;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopActive;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopProfile;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopActive;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopProfile;
using Delivery.Shop.Domain.Handlers.QueryHandlers.ShopProfile;
using Delivery.Shop.Domain.Services;
using Delivery.Shop.Domain.Validators.ShopActive;
using Delivery.Shop.Domain.Validators.ShopProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Shop profile controller
    /// </summary>
    [Route("api/v1/shop-owner/shop-profile", Name = "3 - Shop profile")]
    [PlatformSwaggerCategory(ApiCategory.ShopOwner)]
    [ApiController]
    [Authorize(Policy = "ShopApiUser")]
    public class ShopProfileController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        public ShopProfileController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Shop profile update
        /// </summary>
        [Route("update", Order = 1)]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPut]
        public async Task<IActionResult> Post_UpdateStoreAsync(
            ShopProfileCreationContract shopProfileCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new ShopProfileCreationValidator().ValidateAsync(shopProfileCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            var shopProfileMessage = new ShopProfileMessageContract
            {
                PayloadIn = shopProfileCreationContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new ShopProfileUpdateMessagePublisher(serviceProvider).PublishAsync(shopProfileMessage);

            return Ok(statusContract);
        }

        /// <summary>
        ///  Shop profile update
        /// </summary>
        [Route("update-image", Order = 2)]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPut]
        public async Task<IActionResult> Post_UpdateStoreImageAsync(
            IFormFile? shopImage)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var shopProfileContract =
                await new ShopProfileQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    new ShopProfileQuery{Email = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!});
            if (shopImage != null)
            {
                var shopImageCreationContract = new ShopImageCreationContract
                {
                    StoreId = shopProfileContract.StoreId,
                    StoreName = shopProfileContract.StoreName,
                    ShopImage = shopImage
                };
                
                var storeImageCreationStatusContract =
                    await new ShopService(serviceProvider, executingRequestContextAdapter)
                        .UploadShopImageAsync(shopImageCreationContract);

                var shopProfileImageCreationContract = new ShopProfileImageCreationContract
                {
                    ImageUri = storeImageCreationStatusContract.ShopImageUri,
                    StoreId = shopProfileContract.StoreId
                };

                var statusContract =
                    await new ShopProfileImageUpdateCommandHandler(serviceProvider, executingRequestContextAdapter)
                        .HandleAsync(new ShopProfileImageUpdateCommand(shopProfileImageCreationContract));

                return Ok(statusContract);
            }
            return Ok(new StatusContract{Status = false, DateCreated = DateTimeOffset.UtcNow});
        }
        
        /// <summary>
        ///  Shop profile update
        /// </summary>
        [Route("get-profile", Order = 3)]
        [ProducesResponseType(typeof(ShopProfileContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpGet]
        public async Task<IActionResult> Get_StoreProfileAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var shopProfileContract =
                await new ShopProfileQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    new ShopProfileQuery{Email = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!});
            
            return Ok(shopProfileContract);
        }
        
        /// <summary>
        ///  4. Shop active
        /// </summary>
        [Route("set-shop-active", Order = 4)]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Set_StoreActiveAsync(ShopActiveCreationContract shopActiveCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            if (string.IsNullOrEmpty(shopActiveCreationContract.ShopUserName))
            {
                shopActiveCreationContract = shopActiveCreationContract
                    with
                    {
                        ShopUserName = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!
                    };
            }
            
            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            var shopActiveMessage = new ShopActiveMessageContract
            {
                PayloadIn = shopActiveCreationContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new ShopActiveMessagePublisher(serviceProvider).PublishAsync(shopActiveMessage);

            return Ok(statusContract);
        }
    }
}