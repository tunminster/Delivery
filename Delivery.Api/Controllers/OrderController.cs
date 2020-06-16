using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.CommandHandler;
using Delivery.Api.Domain.Command;
using Delivery.Api.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Delivery.Api.Extensions.HttpResults;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Delivery.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {

        private readonly ILogger<OrderController> _logger;
        private readonly ICommandHandler<CreateOrderCommand> _createOrderCommand;
        private readonly IMapper _mapper;

        public OrderController(ILogger<OrderController> logger,
        ICommandHandler<CreateOrderCommand> createOrderCommand,
        IMapper mapper)
        {
            _logger = logger;
            _createOrderCommand = createOrderCommand;
            _mapper = mapper;
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
                //var command = _mapper.Map<CreateOrderCommand>(orderDto);

                var orderItemCommands = new List<OrderItemCommand>();

                foreach(var item in orderDto.OrderItems)
                {
                    orderItemCommands.Add(new OrderItemCommand() { Count = item.Count, ProductId = item.ProductId });
                }
                //orderItemCommands.Add(new OrderItemCommand { Product})

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
                _logger.LogError(ex, string.Concat(errorMessage," -" , ex.Message));
                return InternalServerErrorResult(errorMessage);
            }
        }

        
    }
}
