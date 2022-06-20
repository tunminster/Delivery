using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.Controllers.Shops;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Database.Models;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopCreation;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopEmailVerification;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopCreation;
using Delivery.Shop.Domain.Services;
using Delivery.Shop.Domain.Validators;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Handlers.QueryHandlers.StoreTypeGetQueries;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.ShopOwner;
using Delivery.User.Domain.Validators.OnBoardings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers.OnBoardings
{
    [Route("api/v1/on-boarding/shop-owner" , Name = "2 - Shop owner OnBoarding api")]
    [PlatformSwaggerCategory(ApiCategory.WebApp)]
    [ApiController]
    public class ShopOwnerOnBoardingController : ShopBaseController
    {
        private readonly IServiceProvider serviceProvider;
        public ShopOwnerOnBoardingController(IServiceProvider serviceProvider, IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, 
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, IJwtFactory jwtFactory, ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, ILogger<UserManager<ApplicationUser>> logger) 
            : base(serviceProvider, optionsAccessor, passwordHasher, userValidators, passwordValidators, jwtFactory, keyNormalizer, errors, logger)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Show owner on-boarding
        /// </summary>
        /// <returns></returns>
        [Route("on-boarding", Order = 1)]
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
        
        /// <summary>
        ///  Shop owner on-boarding registration
        /// </summary>
        /// <returns></returns>
        [Route("register", Order = 1)]
        [ProducesResponseType(typeof(ShopCreationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RegisterShopAsync(
            [ModelBinder(BinderType = typeof(JsonModelBinder))] ShopCreationContract shopCreationContract,
            IFormFile? shopImage)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new ShopCreationValidator().ValidateAsync(shopCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            if(!await CreateUserAsync(shopCreationContract, executingRequestContextAdapter, validationResult))
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var shopCreationStatusContract = new ShopCreationStatusContract
            {
                StoreId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                DateCreated = DateTimeOffset.UtcNow
            };
            
            // upload shop image
            if (shopImage != null)
            {
                var shopImageCreationContract = new ShopImageCreationContract
                {
                    StoreId = shopCreationStatusContract.StoreId,
                    StoreName = shopCreationContract.BusinessName,
                    ShopImage = shopImage
                };
                
                var storeImageCreationStatusContract =
                    await new ShopService(serviceProvider, executingRequestContextAdapter)
                        .UploadShopImageAsync(shopImageCreationContract);

                shopCreationStatusContract = shopCreationStatusContract with
                {
                    ImageUri = storeImageCreationStatusContract.ShopImageUri
                };
            }

            var shopCreationMessage = new ShopCreationMessageContract
            {
                PayloadIn = shopCreationContract,
                PayloadOut = shopCreationStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new ShopCreationMessagePublisher(serviceProvider).PublishAsync(shopCreationMessage);

            return Ok(shopCreationStatusContract);
        }
        
        /// <summary>
        ///  Shop owner Request email verification
        /// </summary>
        /// <param name="shopEmailVerificationContract"></param>
        /// <returns></returns>
        [Route("request-email-otp", Order = 3)]
        [ProducesResponseType(typeof(ShopEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestEmailOtpAsync(
            [FromBody] ShopEmailVerificationContract shopEmailVerificationContract)
        {
            var validationResult = await new ShopEmailVerificationValidator().ValidateAsync(shopEmailVerificationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var shopEmailVerificationStatusContract =
                await new ShopEmailVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new ShopEmailVerificationCommand(shopEmailVerificationContract));

            return Ok(shopEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Shop owner Request email verification
        /// </summary>
        /// <param name="shopEmailVerificationCheckContract"></param>
        /// <returns></returns>
        [Route("verify-email-otp", Order = 4)]
        [ProducesResponseType(typeof(ShopEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyEmailOtpAsync(
            [FromBody] ShopEmailVerificationCheckContract shopEmailVerificationCheckContract)
        {
            var validationResult = await new ShopEmailVerificationCheckValidator().ValidateAsync(shopEmailVerificationCheckContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var shopEmailVerificationStatusContract =
                await new ShopEmailVerificationCheckCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new ShopEmailVerificationCheckCommand(shopEmailVerificationCheckContract));

            if (shopEmailVerificationStatusContract.Status == "approved")
            {
                await ConfirmEmailAsync(shopEmailVerificationCheckContract, executingRequestContextAdapter);
            }

            return Ok(shopEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Get store type
        /// </summary>
        /// <returns></returns>
        [Route("GetAllStoreTypes")]
        [HttpGet]
        [ProducesResponseType(typeof(List<StoreTypeContract>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetStoreTypesAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var storeTypeGetAllQuery =
                new StoreTypeGetAllQuery($"Database-{executingRequestContextAdapter.GetShard().Key}-default-store-types");
            var storeTypeContractList =
                await new StoreTypeGetAllQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(storeTypeGetAllQuery);
          
            return Ok(storeTypeContractList);
        }
        
    }
}