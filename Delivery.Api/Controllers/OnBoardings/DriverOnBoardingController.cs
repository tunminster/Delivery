using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.OnBoardings
{
    [Route("api/v1/on-boarding" , Name = "1 - OnBoarding api")]
    [PlatformSwaggerCategory(ApiCategory.WebApp)]
    [ApiController]
    public class DriverOnBoardingController : ControllerBase
    {
        // GET
        public async Task<IActionResult> Post_OnBoardingDriverAsync()
        {
            return Ok();
        }
    }
}