using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.ShopOwner;
using Delivery.User.Domain.Validators.OnBoardings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.OnBoardings
{
    [Route("api/v1/on-boarding" , Name = "2 - Shop owner OnBoarding api")]
    [PlatformSwaggerCategory(ApiCategory.WebApp)]
    [ApiController]
    public class ShopOwnerOnBoardingController : Controller
    {
        [Route("shop-owner", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(ShopOwnerOnBoardingStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post_OnBoardingShopOwnerAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] ShopOwnerOnBoardingCreationContract shopOwnerOnBoardingCreationContract, string id, IFormFile identityFile)
        {
            var validationResult = await new ShopOwnerOnBoardingCreationValidator().ValidateAsync(shopOwnerOnBoardingCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            if (identityFile.Length < 1)
            {
                return "Identity file should be attached".ConvertToBadRequest();
            }
            
            return Ok(new ShopOwnerOnBoardingStatusContract { Message = "Thank you for joining the Shop Owner partner. We will contact you soon."});
        }
    }
}