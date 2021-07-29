using System;
using System.Threading.Tasks;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
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
            
            return Ok();
        }
    }
}