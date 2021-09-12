using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement;
using Delivery.Shop.Domain.Handlers.QueryHandlers.ShopOrders;
using Delivery.Shop.Domain.Validators.OrderManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Shop order controller
    /// </summary>
    [Route("api/v1/shop-orders", Name = "2 - Shop order")]
    [PlatformSwaggerCategory(ApiCategory.ShopOwner)]
    [ApiController]
    [Authorize(Policy = "ShopApiUser")]
    public class ShopOrdersController : Controller
    {
        
        private readonly IServiceProvider serviceProvider;
        public ShopOrdersController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Get order by shop user
        /// </summary>
        [Route("get-order")]
        [HttpPost]
        [ProducesResponseType(typeof(List<ShopOrderContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetShopOrdersAsync(ShopOrderQueryContract shopOrderQueryContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var shopOrderQuery = new ShopOrderQuery
            {
                Email = userEmail, 
                Status = shopOrderQueryContract.OrderStatus
            };

            var shopOrders = await new ShopOrdersQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(shopOrderQuery);

            return Ok(shopOrders);
        }

        /// <summary>
        ///  Verify order status
        /// </summary>
        [Route("verify-order-status")]
        [HttpPost]
        [ProducesResponseType(typeof(List<StatusContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> VerifyOrderStatusAsync(ShopOrderStatusCreationContract shopOrderStatusCreationContract,
            CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult =
                await new ShopOrderStatusCreationValidator().ValidateAsync(
                    shopOrderStatusCreationContract, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            var shopOrderStatusCreationMessage = new ShopOrderStatusMessageContract
            {
                PayloadIn = shopOrderStatusCreationContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new ShopOrderStatusMessagePublisher(serviceProvider).PublishAsync(shopOrderStatusCreationMessage);

            return Ok(statusContract);
        }

        /// <summary>
        ///  Get order by shop user
        /// </summary>
        [Route("get-order-details")]
        [HttpGet]
        [ProducesResponseType(typeof(List<ShopOrderDetailsContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetShopOrdersAsync(string orderId)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            return Ok();
        }
    }
}