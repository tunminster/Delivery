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
using Delivery.Shop.Domain.Constants;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement;
using Delivery.Shop.Domain.Handlers.QueryHandlers.ShopOrders;
using Delivery.Shop.Domain.Validators.OrderManagement;
using FluentValidation.Results;
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
        [Route("get-order", Order = 1)]
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
        ///  Get order history by shop user
        /// </summary>
        [Route("get-order-history", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(List<ShopOrderContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetShopOrdersHistoryAsync(ShopOrderQueryContract shopOrderQueryContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var shopOrderHistoryQuery = new ShopOrderHistoryQuery
            {
                Email = userEmail, 
                Status = shopOrderQueryContract.OrderStatus
            };

            var shopOrders = await new ShopOrderHistoryQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(shopOrderHistoryQuery);

            return Ok(shopOrders);
        }

        /// <summary>
        ///  Verify order status
        /// </summary>
        [Route("verify-order-status", Order = 2)]
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
        [Route("get-order-details", Order = 3)]
        [HttpGet]
        [ProducesResponseType(typeof(ShopOrderContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetShopOrdersAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                const string validationMessage = "Order id must be provided.";
                return validationMessage.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var shopOrderDetailsQuery = new ShopOrderDetailsQuery
            {
                Email = userEmail,
                OrderId = orderId
            };

            var shopOrderContract = await new ShopOrderDetailsQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(shopOrderDetailsQuery);
            
            return Ok(shopOrderContract);
        }

        /// <summary>
        ///  Request delivery driver
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Route("request-delivery-driver", Order = 4)]
        [HttpGet]
        [ProducesResponseType(typeof(ShopOrderContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeliveryDriverRequest_Async(string orderId)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            if (string.IsNullOrEmpty(orderId))
            {
                var errorMessage = $"{nameof(orderId)} must be provided.";
                return errorMessage.ConvertToBadRequest();
            }
            
            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            var shopOrderDriverRequestMessageContract = new ShopOrderDriverRequestMessageContract
            {
                PayloadIn = new ShopOrderDriverRequestContract { OrderId = orderId},
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new ShopOrderDriverRequestMessagePublisher(serviceProvider).PublishAsync(shopOrderDriverRequestMessageContract);

            return Ok(statusContract);
        }
    }
}