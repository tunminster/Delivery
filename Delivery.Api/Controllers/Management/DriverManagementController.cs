using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverApproval;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile;
using Delivery.Driver.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Driver management
    /// </summary>
    [Route("api/v1/management/driver-management" , Name = "2 - Driver management")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class DriverManagementController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        
        public DriverManagementController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Driver list
        /// </summary>
        /// <returns></returns>
        [Route("drivers", Order = 1)]
        [ProducesResponseType(typeof(DriversPageContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpGet]
        public async Task<IActionResult> Get_DriversAsync(string pageNumber, string pageSize)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            if (string.IsNullOrEmpty(pageNumber) || string.IsNullOrEmpty(pageSize))
            {
                var errorMessage = $"{nameof(pageNumber)} and {nameof(pageSize)} are required";

                return errorMessage.ConvertToBadRequest();
            }

            int.TryParse(pageNumber, out var iPageNumber);
            int.TryParse(pageSize, out var iPageSize);


            var driverQuery = new DriversQuery(iPageNumber, iPageSize);

            var driversPageContract = await new DriversQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverQuery);

            return Ok(driversPageContract);
        }
        
        /// <summary>
        ///  Driver approval
        /// </summary>
        /// <returns></returns>
        [Route("approve-driver", Order = 2)]
        [ProducesResponseType(typeof(DriverApprovalStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_ApproveAsync(DriverApprovalContract driverApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var driverApprovalCommand = new DriverApprovalCommand(driverApprovalContract, true);

            var driverApprovalStatusContract = await new DriverApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                driverApprovalCommand);
            
            return Ok(driverApprovalStatusContract);
        }
        
        /// <summary>
        ///  Driver un-approve
        /// </summary>
        /// <returns></returns>
        [Route("un-approve-driver", Order = 2)]
        [ProducesResponseType(typeof(DriverApprovalStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_UnApproveAsync(DriverApprovalContract driverApprovalContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var driverApprovalCommand = new DriverApprovalCommand(driverApprovalContract, true);

            var driverApprovalStatusContract = await new DriverApprovalCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                driverApprovalCommand);
            
            return Ok(driverApprovalStatusContract);
        }
    }
}