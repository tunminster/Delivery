using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopPaymentHistory;
using Delivery.Shop.Domain.Handlers.QueryHandlers.ShopPaymentHistory;
using Delivery.Shop.Domain.Validators.ShopPaymentHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Shops
{
    [Route("api/v1/shop-payments", Name = "6 - Shop payments")]
    [PlatformSwaggerCategory(ApiCategory.ShopOwner)]
    [ApiController]
    [Authorize(Policy = "ShopApiUser")]
    public class ShopPaymentController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        public ShopPaymentController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Get weekly payment history
        /// </summary>
        /// <remarks>Get list of payments by month and year.</remarks>
        [Route("get-weekly-payment-history", Order = 1)]
        [ProducesResponseType(typeof(List<ShopPaymentHistoryContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Get_Driver_Earnings_Async(ShopPaymentHistoryQueryContract shopPaymentHistoryQueryContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new ShopPaymentHistoryValidator().ValidateAsync(shopPaymentHistoryQueryContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var shopPaymentHistoryQuery = new ShopPaymentHistoryQuery(shopPaymentHistoryQueryContract);
            var paymentHistoryList = await new ShopPaymentHistoryQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(shopPaymentHistoryQuery);
            
            return Ok(paymentHistoryList);
        }
    }
}