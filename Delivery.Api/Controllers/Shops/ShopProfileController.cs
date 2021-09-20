using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopProfile;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopProfile;
using Delivery.Shop.Domain.Validators.ShopProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Shop profile controller
    /// </summary>
    [Route("api/v1/shop-profile", Name = "3 - Shop profile")]
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
    }
}