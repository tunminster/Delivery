using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Api.Models.Dto;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;
using Delivery.Order.Domain.QueryHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ICommandHandler<CreateOrderCommand, bool> _createOrderCommand;
        private readonly IQueryHandler<OrderByCustomerIdQuery, List<OrderContract>> queryOrderByCustomerIdQuery;

        public OrderController(
            ICommandHandler<CreateOrderCommand, bool> createOrderCommand,
            IQueryHandler<OrderByCustomerIdQuery, List<OrderContract>> queryOrderByCustomerIdQuery
        )
        {
            _createOrderCommand = createOrderCommand;
            this.queryOrderByCustomerIdQuery = queryOrderByCustomerIdQuery;
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

        [HttpGet("GetByUserId/{userId}")]
        [ProducesResponseType(typeof(List<OrderContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductByCategoryId(int userId)
        {
            var query = new OrderByCustomerIdQuery {CustomerId = userId};
            var result = await queryOrderByCustomerIdQuery.Handle(query);

            return Ok(result);
        }
    }
}
