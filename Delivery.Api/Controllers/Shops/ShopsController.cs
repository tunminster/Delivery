using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.Factories;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.Models;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopCreation;
using Delivery.Shop.Domain.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopLogin;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopResetPasswordVerification;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopEmailVerification;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopResetPasswordVerification;
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
using Newtonsoft.Json;

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Shop controller
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
        
        /// <summary>
        ///  Request email verification
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
                    .Handle(new ShopEmailVerificationCommand(shopEmailVerificationContract));

            return Ok(shopEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Request email verification
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
                    .Handle(new ShopEmailVerificationCheckCommand(shopEmailVerificationCheckContract));

            if (shopEmailVerificationStatusContract.Status == "approved")
            {
                await ConfirmEmailAsync(shopEmailVerificationCheckContract, executingRequestContextAdapter);
            }

            return Ok(shopEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Shop owner login 
        /// </summary>
        /// <param name="shopLoginContract"></param>
        /// <returns></returns>
        [Route("login", Order = 2)]
        [HttpPost]
        public async Task<IActionResult> Post_LoginAsync([FromBody] ShopLoginContract shopLoginContract)
        {
            var validationResult = await new ShopLoginValidator().ValidateAsync(shopLoginContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter(); 
            
            var isApproved =
                await new ShopService(serviceProvider, executingRequestContextAdapter).IsShopOwnerApprovedAsync(
                    shopLoginContract.Username);

            if (isApproved == false)
            {
                validationResult.Errors.Add(new ValidationFailure(nameof(ShopLoginContract), 
                    "Your account application under review. we will send you email when the application is approved."));
                
                return validationResult.ConvertToBadRequest();
            }
            
            var identity = await GetClaimsIdentityAsync(shopLoginContract.Username, shopLoginContract.Password, executingRequestContextAdapter);
            
            if (identity == null)
            {
                validationResult.Errors.Add(new ValidationFailure(nameof(ShopLoginContract), 
                    "Invalid username or password."));
                
                return validationResult.ConvertToBadRequest();
            }
            
            var jwt = await Tokens.GenerateJwtAsync(identity, jwtFactory, shopLoginContract.Username, jwtOptions, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }
        
        /// <summary>
        ///  Request reset password otp
        /// </summary>
        /// <param name="shopResetPasswordRequestContract"></param>
        /// <returns></returns>
        [Route("request-reset-password-otp", Order = 5)]
        [ProducesResponseType(typeof(ShopResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestResetPasswordAsync(
            [FromBody] ShopResetPasswordRequestContract shopResetPasswordRequestContract)
        {
            var validationResult = await new ShopResetPasswordRequestValidator().ValidateAsync(shopResetPasswordRequestContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var shopResetPasswordStatusContract =
                await new ShopResetPasswordVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new ShopResetPasswordVerificationCommand(shopResetPasswordRequestContract));

            return Ok(shopResetPasswordStatusContract);
        }
        
        /// <summary>
        ///  Verify otp and reset password
        /// </summary>
        /// <param name="shopResetPasswordCreationContract"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("verify-reset-password", Order = 7)]
        [ProducesResponseType(typeof(ShopResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyResetPasswordOtpAsync([FromBody] ShopResetPasswordCreationContract shopResetPasswordCreationContract)
        {
            var validationResult =
                await new ShopResetPasswordVerificationCheckValidator().ValidateAsync(
                    shopResetPasswordCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverResetPasswordStatusContract =
                await new ShopResetPasswordVerificationCheckCommandHandler(serviceProvider,
                        executingRequestContextAdapter)
                    .Handle(new ShopResetPasswordVerificationCheckCommand(shopResetPasswordCreationContract));

            if (driverResetPasswordStatusContract.Status == "approved")
            {
                await using var applicationDbContext =
                    await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
                var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

                var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                    passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

                var user = await userManager.FindByEmailAsync(shopResetPasswordCreationContract.Email);

                if (user == null)
                {
                    throw new InvalidOperationException($"Expected to be found {shopResetPasswordCreationContract.Email} user.").WithTelemetry(
                        executingRequestContextAdapter.GetTelemetryProperties());
                }
                
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetResult = await userManager.ResetPasswordAsync(user, token, shopResetPasswordCreationContract.Password);

                if (passwordResetResult.Succeeded)
                {
                    return Ok(driverResetPasswordStatusContract);
                }

                foreach (var error in passwordResetResult.Errors)
                {
                    validationResult.Errors.Add(new ValidationFailure(error.Code, error.Description));
                }
                
                return validationResult.ConvertToBadRequest();
            }
            
            var errorResult = driverResetPasswordStatusContract with
            {
                Status = "error",
                Valid = false
            };

            return Ok(errorResult);
        }
        
        private async Task ConfirmEmailAsync(ShopEmailVerificationCheckContract shopEmailVerificationCheckContract,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            await using var applicationDbContext =
                await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

            var user = await userManager.FindByEmailAsync(shopEmailVerificationCheckContract.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Expected to be found user.").WithTelemetry(
                    executingRequestContextAdapter.GetTelemetryProperties());
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
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
        
        private async Task<ClaimsIdentity?> GetClaimsIdentityAsync(string userName, string password, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity?>(null);
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);

            // get the user to verifty
            var userToVerify = await userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);
            
            var claimList = await userManager.GetClaimsAsync(userToVerify);
            
            var roleList = await userManager.GetRolesAsync(userToVerify);

            // check the credentials
            if (await userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id, claimList, roleList.ToList(),executingRequestContextAdapter));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity?>(null);
        }
    }
}