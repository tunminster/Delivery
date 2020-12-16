using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Api.Models.Dto;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Factories;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;
using Delivery.Order.Domain.QueryHandlers;
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

        // POST api/values
        [HttpPost("Create")]
        [ProducesResponseType(typeof(OrderCreationContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddOrderAsync(OrderCreationContract orderCreationContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var orderItemCommands = new List<OrderItemCommand>();

            foreach(var item in orderCreationContract.OrderItems)
            {
                orderItemCommands.Add(new OrderItemCommand() { Count = item.Count, ProductId = item.ProductId });
            }

            var command = new CreateOrderCommand();
            command.Description = string.Empty;
            command.TotalAmount = Convert.ToDecimal(orderCreationContract.TotalAmount);
            command.CurrencyCode = "GBP";
            command.PaymentType = "Card";
            command.CardHolderName = orderCreationContract.CardHolderName;
            command.PaymentCard = orderCreationContract.CardNumber;
            command.PaymentStatus = string.Empty;
            command.PaymentExpiryMonth = orderCreationContract.ExpiryMonth;
            command.PaymentExpiryYear = orderCreationContract.ExpiryYear;
            command.PaymentCVC = orderCreationContract.Cvc;
            command.PaymentIssueNumber = "1";
            command.OrderStatus = string.Empty;
            command.CustomerId = orderCreationContract.CustomerId;
            command.DateCreated = DateTime.UtcNow;
            command.OrderItems = orderItemCommands;
            command.ShippingAddressId = orderCreationContract.ShippingAddressId;
            command.SaveCard = orderCreationContract.SaveCard;

            var createOrderCommandHandler = new OrderCommandHandler(httpClientFactory, configuration, serviceProvider,
                executingRequestContextAdapter);

            await createOrderCommandHandler.Handle(command);
            var orderCreationStatusContract = new OrderCreationStatusContract
            {
                OrderId = UniqueIdFactory.UniqueExternalId(executingRequestContextAdapter.GetShard().Key), 
                Status = "Order has been created successfully."
            };

            return Ok(orderCreationStatusContract);
        }

        [HttpGet("GetByUserId/{userId}")]
        [ProducesResponseType(typeof(List<OrderContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductByCategoryIdAsync(int userId)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var query = new OrderByCustomerIdQuery {CustomerId = userId};

            var orderByCustomerIdQueryHandler =
                new OrderByCustomerIdQueryHandler(serviceProvider, executingRequestContextAdapter);
            var result = await orderByCustomerIdQueryHandler.Handle(query);

            return Ok(result);
        }
    }
}
