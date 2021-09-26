using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopMenu;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopMenu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Shop menu controller
    /// </summary>
    [Route("api/v1/shop-menu", Name = "4 - Shop Menu")]
    [PlatformSwaggerCategory(ApiCategory.ShopOwner)]
    [ApiController]
    [Authorize(Policy = "ShopApiUser")]
    public class ShopMenuController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Shop menu controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ShopMenuController(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Set menu turn on or off
        ///  If menu id is not provided, the whole store's menu will be turn on or off.
        /// </summary>
        [Route("set-menu-status", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post_ShopMenu_Async(ShopMenuStatusCreationContract shopMenuStatusCreationContract,
            CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var statusContract = new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };

            var shopMenuStatusMessageContract = new ShopMenuStatusMessageContract
            {
                PayloadIn = shopMenuStatusCreationContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };

            await new ShopMenuStatusMessagePublisher(serviceProvider).PublishAsync(shopMenuStatusMessageContract);

            return Ok(statusContract);
        }
    }
}