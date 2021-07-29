using System;
using System.Threading.Tasks;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Driver controller
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DriversController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Driver controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DriversController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Create driver application
        /// </summary>
        /// <param name="driverCreationContract"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Post_RegisterDriverAsync([FromBody] DriverCreationContract driverCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            // upload image service

            var driverCreationStatusContract = new DriverCreationStatusContract
            {
                DateCreated = DateTimeOffset.UtcNow,
                Message = "Driver application submitted successfully.",
                ImageUri = string.Empty,
                DrivingLicenseFrontUri = string.Empty,
                DrivingLicenseBackUri = string.Empty
            };

            var driverCreationMessage = new DriverCreationMessageContract
            {
                PayloadIn = driverCreationContract,
                PayloadOut = driverCreationStatusContract
            };
            
            await new DriverCreationMessagePublisher(serviceProvider).PublishAsync(driverCreationMessage);
            
            return Ok(driverCreationStatusContract);
        }
    }
}