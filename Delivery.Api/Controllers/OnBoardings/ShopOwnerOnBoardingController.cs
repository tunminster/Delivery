using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.ShopOwner;
using Delivery.User.Domain.Validators.OnBoardings;
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
        public async Task<IActionResult> Post_OnBoardingShopOwnerAsync(ShopOwnerOnBoardingCreationContract shopOwnerOnBoardingCreationContract, string id)
        {
            var validationResult = await new ShopOwnerOnBoardingCreationValidator().ValidateAsync(shopOwnerOnBoardingCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            return Ok(new ShopOwnerOnBoardingStatusContract { AccountNumber = "acct_1JlVEQRTb0JcIyR1"});
        }
    }
}