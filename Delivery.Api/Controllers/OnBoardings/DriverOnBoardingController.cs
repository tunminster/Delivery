using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.Driver;
using Delivery.User.Domain.Validators.OnBoardings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.OnBoardings
{
    [Route("api/v1/on-boarding" , Name = "1 - Driver OnBoarding api")]
    [PlatformSwaggerCategory(ApiCategory.WebApp)]
    [ApiController]
    public class DriverOnBoardingController : ControllerBase
    {
        [Route("driver", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(DriverOnBoardingStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post_OnBoardingDriverAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] DriverOnBoardingCreationContract driverOnBoardingCreationContract, string id, IFormFile identityFile)
        {
            var validationResult = await new DriverOnBoardingCreationValidator().ValidateAsync(driverOnBoardingCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            if (identityFile.Length < 1)
            {
                return "Identity file should be attached".ConvertToBadRequest();
            }
            
            return Ok(new DriverOnBoardingStatusContract { Message = "Thank you for joining to Delivery partner. We will contact you soon."});
        }
    }
}