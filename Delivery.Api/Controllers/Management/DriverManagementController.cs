using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverApproval;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    [Route("api/driver-management" , Name = "2 - Driver management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Policy = "BackOfficeUser")]
    public class DriverManagementController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        
        public DriverManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Driver approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-driver", Order = 1)]
        [ProducesResponseType(typeof(DriverApprovalStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_ApproveAsync(DriverApprovalContract driverApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var driverApprovalCommand = new DriverApprovalCommand(driverApprovalContract);

            var driverApprovalStatusContract = await new DriverApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                driverApprovalCommand);
            
            return Ok(driverApprovalStatusContract);
        }
    }
}