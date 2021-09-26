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
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrder;
using Delivery.Driver.Domain.Validators;
using Delivery.Driver.Domain.Validators.DriverOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver orders controller
    /// </summary>
    [Route("api/v1/driver-orders", Name = "5 - Driver Orders")]
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
        [Route("order-assign", Order = 1)]
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
    }
}