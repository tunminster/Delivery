using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
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
        ///  Get store type
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
    }
}