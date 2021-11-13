using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrder;
using Delivery.Driver.Domain.Validators;
using Delivery.Driver.Domain.Validators.DriverAssignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver assignment controller
    /// </summary>
    [Route("api/v1/delivery-partners/driver-assignment", Name = "3 - Driver Assignment")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [Authorize(Policy = "DriverApiUser")]
    [ApiController]
    public class DriverAssignmentController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        
        public DriverAssignmentController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Driver assignment
        /// </summary>
        /// <returns></returns>
        [Route("assign", Order = 1)]
        [ProducesResponseType(typeof(DriverAssignmentStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_AssignAsync(DriverAssignmentCreationContract driverAssignmentCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverAssignmentValidator().ValidateAsync(driverAssignmentCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var driverAssignmentStatusContract = new DriverAssignmentStatusContract
            {
                DateCreated = DateTimeOffset.UtcNow
            };
            
            var driverAssignmentCreationMessage = new DriverAssignmentMessageContract
            {
                PayloadIn = driverAssignmentCreationContract,
                PayloadOut = driverAssignmentStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverAssignmentMessagePublisher(serviceProvider).PublishAsync(driverAssignmentCreationMessage);
            
            return Ok(driverAssignmentStatusContract);
        }

        /// <summary>
        ///  Get order details for driver
        /// </summary>
        /// <returns></returns>
        [Route("get-order-details", Order = 2)]
        [ProducesResponseType(typeof(DriverOrderDetailsContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpGet]
        public async Task<IActionResult> Get_Order_DetailsAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverOrderQuery = new DriverOrderQuery();
            var driverOrderDetailsContract =
                await new DriverOrderQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(driverOrderQuery);

            return Ok(driverOrderDetailsContract);
        }
    }
}