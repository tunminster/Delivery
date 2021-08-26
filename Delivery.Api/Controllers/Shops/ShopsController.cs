using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.Models;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopCreation;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopCreation;
using Delivery.Shop.Domain.Services;
using Delivery.Shop.Domain.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Driver controller
    /// </summary>
    [Route("api/v1/[controller]", Name = "1 - Shop owner")]
    [PlatformSwaggerCategory(ApiCategory.ShopOwner)]
    [ApiController]
    public class ShopsController : ControllerBase
    {
        
        private readonly IServiceProvider serviceProvider;
        private readonly IJwtFactory jwtFactory;
        private readonly JwtIssuerOptions jwtOptions;
        private IOptions<IdentityOptions> optionsAccessor;
        private readonly IPasswordHasher<Database.Models.ApplicationUser> passwordHasher;
        private readonly IEnumerable<IUserValidator<Database.Models.ApplicationUser>> userValidators;
        private readonly IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> passwordValidators;
        private readonly ILookupNormalizer keyNormalizer;
        private readonly IdentityErrorDescriber errors;
        private readonly ILogger<UserManager<Database.Models.ApplicationUser>> logger;
        
        public ShopsController(IServiceProvider serviceProvider, 
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<Database.Models.ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<Database.Models.ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> passwordValidators,
            IJwtFactory jwtFactory,
            IOptions<JwtIssuerOptions> jwtOptions,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<UserManager<Database.Models.ApplicationUser>> logger
        )
        {
            this.serviceProvider = serviceProvider;
            this.optionsAccessor = optionsAccessor;
            this.passwordHasher = passwordHasher;
            this.userValidators = userValidators;
            this.passwordValidators = passwordValidators;
            this.jwtFactory = jwtFactory;
            this.jwtOptions = jwtOptions.Value;
            this.keyNormalizer = keyNormalizer;
            this.errors = errors;
            this.logger = logger;
        }
        
        /// <summary>
        ///  Shop register
        /// </summary>
        /// <param name="shopCreationContract"></param>
        /// <param name="shopImage"></param>
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
        
        private async Task<bool> CreateUserAsync(ShopCreationContract shopCreationContract, IExecutingRequestContextAdapter executingRequestContextAdapter, ValidationResult validationResult)
        {
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            using var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var user = new Database.Models.ApplicationUser { UserName = shopCreationContract.EmailAddress, Email = shopCreationContract.EmailAddress };
            var result = await userManager.CreateAsync(user, shopCreationContract.Password);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "ShopOwner");
                var claim = new Claim(ClaimData.ShopApiAccess.ClaimType, ClaimData.ShopApiAccess.ClaimValue, ClaimValueTypes.String);
                var groupClaim = new Claim("groups", executingRequestContextAdapter.GetShard().Key,
                    ClaimValueTypes.String);
                
                await userManager.AddClaimAsync(user, claim);
                await userManager.AddClaimAsync(user, groupClaim);
                
                return true;
            }

            foreach (var error in result.Errors)
            {
                validationResult.Errors.Add(new ValidationFailure(error.Code, error.Description));
            }
            
            return false;
        }
    }
}