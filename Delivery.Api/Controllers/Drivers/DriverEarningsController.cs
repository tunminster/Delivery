using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverEarnings;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrder;
using Delivery.Driver.Domain.Validators.DriverEarnings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver earning controller
    /// </summary>
    [Route("api/v1/driver-earning", Name = "7 - Driver Earnings")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [Authorize(Policy = "DriverApiUser")]
    [ApiController]
    public class DriverEarningsController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        
        public DriverEarningsController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Get driver earnings
        /// </summary>
        /// <returns></returns>
        [Route("get-driver-earnings", Order = 1)]
        [ProducesResponseType(typeof(List<DriverEarningContract>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Get_Driver_Earnings_Async(DriverEarningQueryContract driverEarningQueryContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverEarningQueryValidator().ValidateAsync(driverEarningQueryContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var driverEarningsQuery = new DriverEarningsQuery(driverEarningQueryContract, 1, 100);
            var earnings = await new DriverEarningsQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverEarningsQuery);
            
            return Ok(earnings);
        }
    }
}