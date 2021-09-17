using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverElasticSearch;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver controller
    /// </summary>
    [Route("api/v1/driver-profile", Name = "1 - Driver Profile")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [ApiController]
    [Authorize(Policy = "DriverApiUser")]
    public class DriverProfileController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public DriverProfileController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Get store type
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("get-profile")]
        [HttpGet]
        [ProducesResponseType(typeof(DriverProfileContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDriverProfileAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverProfileQuery = new DriverProfileQuery
            {
                EmailAddress = userEmail
            };

            var driverProfileContract = await new DriverProfileQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverProfileQuery);

            return Ok(driverProfileContract);
        }

        /// <summary>
        ///  Get driver status
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("get-driver-status")]
        [HttpGet]
        [ProducesResponseType(typeof(DriverActiveStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDriverStatusAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverStatusQuery = new DriverStatusQuery(userEmail);
            var driverActiveStatusContract = await new DriverStatusQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverStatusQuery);

            return Ok(driverActiveStatusContract);
        }
        
        /// <summary>
        ///  Index driver
        /// </summary>
        /// <returns></returns>
        [Route("index-driver")]
        [HttpPost]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> IndexDriverAsync(DriverIndexCreationContract driverIndexCreationContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverIndexCommand = new DriverIndexCommand(driverIndexCreationContract.DriverId);
            var driverIndexStatusContract = await new DriverIndexCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverIndexCommand);
            var statusContract = new StatusContract
            {
                Status = driverIndexStatusContract.Status,
                DateCreated = driverIndexStatusContract.DateCreated
            };
            
            return Ok(statusContract);
        }
    }
}