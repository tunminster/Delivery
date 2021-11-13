using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverSearch;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverAssignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver controller
    /// </summary>
    [Route("api/v1/delivery-partners/driver-search", Name = "6 - Driver Search")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [ApiController]
    [Authorize(Policy = "DriverApiUser")]
    public class DriverSearchController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        public DriverSearchController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Search driver
        /// </summary>
        /// <returns></returns>
        [Route("search-driver")]
        [HttpPost]
        [ProducesResponseType(typeof(List<DriverContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_Nearest_Drivers_Async(DriverSearchCreationContract driverSearchCreationContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverByNearestLocationQuery = new DriverByNearestLocationQuery
            {
                Latitude = driverSearchCreationContract.Latitude,
                Longitude = driverSearchCreationContract.Longitude,
                Distance = driverSearchCreationContract.Distance,
                Page = driverSearchCreationContract.Page,
                PageSize = driverSearchCreationContract.PageSize
            };

            var driverContracts =
                await new DriverByNearestLocationQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    driverByNearestLocationQuery);

            return Ok(driverContracts);
        }
    }
}