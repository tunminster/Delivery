using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.CommandHandler;
using Delivery.Api.Domain.Command;
using Delivery.Api.Domain.Query;
using Delivery.Api.Models.Dto;
using Delivery.Api.QueryHandler;
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
        private readonly ICommandHandler<CreateOrderCommand> _createOrderCommand;
        private readonly IQueryHandler<GetOrderByCustomerIdQuery, OrderViewDto[]> _queryOrderByCustomerIdQuery;

        public OrderController(
            ILogger<OrderController> logger,
            ICommandHandler<CreateOrderCommand> createOrderCommand,
            IQueryHandler<GetOrderByCustomerIdQuery, OrderViewDto[]> queryOrderByCustomerIdQuery
        )
        {
            _logger = logger;
            _createOrderCommand = createOrderCommand;
            _queryOrderByCustomerIdQuery = queryOrderByCustomerIdQuery;
        }

        // POST api/values
        [HttpPost("Create")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddOrder(OrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var orderItemCommands = new List<OrderItemCommand>();

                foreach(var item in orderDto.OrderItems)
                {
                    orderItemCommands.Add(new OrderItemCommand() { Count = item.Count, ProductId = item.ProductId });
                }

                var command = new CreateOrderCommand();
                command.Description = string.Empty;
                command.TotalAmount = Convert.ToDecimal(orderDto.TotalAmount);
                command.CurrencyCode = "GBP";
                command.PaymentType = "Card";
                command.CardHolderName = orderDto.CardHolderName;
                command.PaymentCard = orderDto.CardNumber;
                command.PaymentStatus = string.Empty;
                command.PaymentExpiryMonth = orderDto.ExpiryMonth;
                command.PaymentExpiryYear = orderDto.ExpiryYear;
                command.PaymentCVC = orderDto.Cvc;
                command.PaymentIssueNumber = "1";
                command.OrderStatus = string.Empty;
                command.CustomerId = orderDto.CustomerId;
                command.DateCreated = DateTime.UtcNow;
                command.OrderItems = orderItemCommands;
                command.ShippingAddressId = orderDto.ShippingAddressId;
                command.SaveCard = orderDto.SaveCard;

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
