using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Api.Domain.Query;
using Delivery.Api.Models.Dto;
using Delivery.Api.QueryHandler;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.RestContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Delivery.Api.Extensions.HttpResults;


namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {

        private readonly ILogger<OrderController> _logger;
        private readonly ICommandHandler<CreateOrderCommand, bool> _createOrderCommand;
        private readonly IQueryHandler<OrderByCustomerIdQuery, OrderViewDto[]> _queryOrderByCustomerIdQuery;

        public OrderController(
            ILogger<OrderController> logger,
            ICommandHandler<CreateOrderCommand, bool> createOrderCommand,
            IQueryHandler<GetOrderByCustomerIdQuery, OrderViewDto[]> queryOrderByCustomerIdQuery
        )
        {
            _logger = logger;
            _createOrderCommand = createOrderCommand;
            _queryOrderByCustomerIdQuery = queryOrderByCustomerIdQuery;
        }

        // POST api/values
        [HttpPost("Create")]
        [ProducesResponseType(typeof(OrderCreationContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddOrder(OrderCreationContract orderCreationContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
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

                await _createOrderCommand.Handle(command);

                return Ok();
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred in creating order";
                _logger.LogError(ex, string.Concat(errorMessage," - " , ex.Message));
                return InternalServerErrorResult(errorMessage);
            }
        }

        [HttpGet("GetByUserId/{userId}")]
        [ProducesResponseType(typeof(List<OrderViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductByCategoryId(int userId)
        {
            try
            { 
                var query = new GetOrderByCustomerIdQuery();
                query.CustomerId = userId;
                var result = await _queryOrderByCustomerIdQuery.Handle(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorMessage = "Fetching orders by userId.";
                _logger.LogError(ex, errorMessage);
                return InternalServerErrorResult(errorMessage);
            }
        }


    }
}
