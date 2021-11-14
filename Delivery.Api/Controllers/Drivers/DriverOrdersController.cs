using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrder;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrderHistory;
using Delivery.Driver.Domain.Validators;
using Delivery.Driver.Domain.Validators.DriverAssignment;
using Delivery.Driver.Domain.Validators.DriverOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver orders controller
    /// </summary>
    [Route("api/v1/delivery-partners/driver-orders", Name = "5 - Driver Orders")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [ApiController]
    [Authorize(Policy = "DriverApiUser")]
    public class DriverOrdersController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        
        public DriverOrdersController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Driver assignment
        /// </summary>
        /// <returns></returns>
        [Route("update-delivery-order-status", Order = 1)]
        [ProducesResponseType(typeof(StatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_AssignAsync(DriverOrderActionContract driverOrderActionContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverOrderActionValidator().ValidateAsync(driverOrderActionContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            var driverOrderActionMessage = new DriverOrderActionMessageContract
            {
                PayloadIn = driverOrderActionContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverOrderActionMessagePublisher(serviceProvider).PublishAsync(driverOrderActionMessage);
            
            return Ok(statusContract);
        }

        [Route("get-driver-orders-status", Order = 2)]
        [ProducesResponseType(typeof(List<DriverOrderDetailsContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Get_DriverOrdersStatus_Async(DriverOrderStatusRequestContract driverOrderStatusRequestContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverOrderStatusRequestContractValidator().ValidateAsync(driverOrderStatusRequestContract, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var driverOrderStatusQuery = new DriverOrderStatusQuery
            {
                DriverOrderStatusRequestContract = driverOrderStatusRequestContract
            };

            var driverOrderDetails =
                await new DriverOrderStatusQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(driverOrderStatusQuery);

            return Ok(driverOrderDetails);

        }

        [Route("set-driver-orders-index", Order = 3)]
        [ProducesResponseType(typeof(StatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Set_DriverOrdersIndex_Async(
            DriverOrderIndexCreationContract driverOrderIndexCreationContract,
            CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverOrderIndexCreationContractValidator()
                .ValidateAsync(driverOrderIndexCreationContract, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            var driverOrderIndexMessageContract = new DriverOrderIndexMessageContract
            {
                PayloadIn = driverOrderIndexCreationContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverOrderIndexMessagePublisher(serviceProvider).PublishAsync(driverOrderIndexMessageContract);

            return Ok(statusContract);
        }
        
        [Route("set-all-driver-orders-index", Order = 4)]
        [ProducesResponseType(typeof(HttpStatusCode), (int) HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Set_AllDriverOrdersIndex_Async(
            DriverOrderIndexAllCreationContract driverOrderIndexAllCreationContract,
            CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverOrderIndexAllCreationContractValidator()
                .ValidateAsync(driverOrderIndexAllCreationContract, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            await new DriverOrderIndexAllCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new DriverOrderIndexAllCommand(driverOrderIndexAllCreationContract.CreateDate));

            return Accepted();
        }
        
        [Route("remove-all-driver-orders-index", Order = 5)]
        [ProducesResponseType(typeof(HttpStatusCode), (int) HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpDelete]
        public async Task<IActionResult> Remove_AllDriverOrdersIndex_Async(DriverOrderIndexAllCreationContract driverOrderIndexAllCreationContract,
            CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverOrderIndexAllCreationContractValidator()
                .ValidateAsync(driverOrderIndexAllCreationContract, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            await new DriverOrderIndexDeleteAllCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new DriverOrderIndexDeleteAllCommand(driverOrderIndexAllCreationContract.CreateDate));

            return Accepted();
        }

        [Route("get-driver-order-history", Order = 6)]
        [ProducesResponseType(typeof(List<DriverOrderHistoryContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Get_OrderHistoryAsync(
            DriverOrderHistoryRequestContract driverOrderHistoryRequestContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var orderHistoryContracts = await new DriverOrderHistoryQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new DriverOrderHistoryQuery(driverOrderHistoryRequestContract.OrderDateFrom,
                    driverOrderHistoryRequestContract.DriverOrderStatus));

            return Ok(orderHistoryContracts);
        }
        
        [Route("get-driver-order-history-details", Order = 7)]
        [ProducesResponseType(typeof(List<DriverOrderHistoryContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpGet]
        public async Task<IActionResult> Get_OrderHistoryAsync(
            string orderId)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            if (string.IsNullOrEmpty(orderId))
            {
                const string message = "Order id is required";
                return message.ConvertToBadRequest();
            }
            
            var orderHistoryDetailsContract = await new DriverOrderHistoryDetailsQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new DriverOrderHistoryDetailsQuery(orderId));

            return Ok(orderHistoryDetailsContract);
        }
    }
}