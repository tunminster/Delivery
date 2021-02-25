using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Api.Models.Dto;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Factories;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.ModelContracts.Stripe;
using Delivery.Order.Domain.Contracts.RestContracts;
using Delivery.Order.Domain.Contracts.RestContracts.OrderDetails;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.Order.Domain.Handlers.QueryHandlers;
using Delivery.Order.Domain.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        public OrderController(IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }
        
        [HttpPost("Payment/CreatePaymentIntent")]
        [ProducesResponseType(typeof(PaymentIntentCreationStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreatePaymentIntentAsync(StripeOrderCreationContract stripeOrderCreationContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var paymentOrderServiceRequest = new PaymentOrderServiceRequest(stripeOrderCreationContract, "gbp");
            var paymentIntentCreationStatusContract =
                await new PaymentOrderService(serviceProvider, executingRequestContextAdapter)
                    .ExecuteStripePaymentIntentWorkflow(paymentOrderServiceRequest);

            return Ok(paymentIntentCreationStatusContract);
        }

        /// <summary>
        ///  Get orders by User id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("GetByUserId/{userId}")]
        [ProducesResponseType(typeof(List<OrderContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetProductByCategoryIdAsync(int userId, int page = 1, int pageSize = 20)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var query = new OrderByCustomerIdQuery(userId, page, pageSize);

            var orderByCustomerIdQueryHandler =
                new OrderByCustomerIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            var result = await orderByCustomerIdQueryHandler.Handle(query);

            return Ok(result);
        }
        
        /// <summary>
        ///  Get Order details
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        [HttpGet("GetOrderDetails")]
        [ProducesResponseType(typeof(List<OrderDetailsContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetProductByCategoryIdAsync(string orderId, string timeZone)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var query = new OrderDetailsQuery(orderId, int.Parse(timeZone));

            var orderDetailsContract =
                await new OrderDetailsQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(query);

            return Ok(orderDetailsContract);
        }
    }
}
