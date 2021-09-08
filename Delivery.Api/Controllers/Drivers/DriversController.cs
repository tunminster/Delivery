using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Authentication.OpenIdConnect.Extensions;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Customer.Domain.CommandHandlers;
using Delivery.Customer.Domain.Contracts.RestContracts;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.Factories;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.Models;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverCheckEmailVerification;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverResetPasswordVerification;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverActive;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverCheckEmailVerification;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverCheckResetPasswordVerification;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverEmailVerification;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverResetPasswordVerification;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverActive;
using Delivery.Driver.Domain.Services;
using Delivery.Driver.Domain.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver controller
    /// </summary>
    [Route("api/v1/[controller]", Name = "1 - Driver")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [ApiController]
    public class DriversController : ControllerBase
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

        /// <summary>
        ///  Driver controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="optionsAccessor"></param>
        /// <param name="passwordHasher"></param>
        /// <param name="userValidators"></param>
        /// <param name="passwordValidators"></param>
        public DriversController(IServiceProvider serviceProvider, 
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
        ///  Create driver application
        /// </summary>
        /// <param name="driverCreationContract"></param>
        /// <param name="driverImage"></param>
        /// <param name="drivingLicenseFrontImage"></param>
        /// <param name="drivingLicenseBackImage"></param>
        /// <returns></returns>
        [Route("register", Order = 1)]
        [ProducesResponseType(typeof(DriverCreationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RegisterDriverAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] DriverCreationContract driverCreationContract, 
            IFormFile? driverImage, IFormFile? drivingLicenseFrontImage, IFormFile? drivingLicenseBackImage)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var validationResult = await new DriverCreationValidator().ValidateAsync(driverCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            // upload image service
            var driverImageCreationContract = new DriverImageCreationContract
            {
                DriverName = driverCreationContract.FullName,
                DriverEmailAddress = driverCreationContract.EmailAddress,
                DriverImage = driverImage,
                DrivingLicenseFrontImage = drivingLicenseFrontImage,
                DrivingLicenseBackImage = drivingLicenseBackImage
            };

            var driverImageCreationStatusContract = await new DriverService(serviceProvider, executingRequestContextAdapter)
                .UploadDriverImagesAsync(driverImageCreationContract);

            if(!await CreateUserAsync(driverCreationContract, executingRequestContextAdapter, validationResult))
            {
                return validationResult.ConvertToBadRequest();
            }

            var driverCreationStatusContract = new DriverCreationStatusContract
            {
                DriverId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                DateCreated = DateTimeOffset.UtcNow,
                Message = "Driver application submitted successfully.",
                ImageUri = driverImageCreationStatusContract.DriverImageUri,
                DrivingLicenseFrontUri = driverImageCreationStatusContract.DrivingLicenseFrontImageUri,
                DrivingLicenseBackUri = driverImageCreationStatusContract.DrivingLicenseBackImageUri
            };

            var driverCreationMessage = new DriverCreationMessageContract
            {
                PayloadIn = driverCreationContract,
                PayloadOut = driverCreationStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverCreationMessagePublisher(serviceProvider).PublishAsync(driverCreationMessage);
            
            return Ok(driverCreationStatusContract);
        }

        /// <summary>
        ///  Driver login 
        /// </summary>
        /// <param name="driverLoginContract"></param>
        /// <returns></returns>
        [Route("login", Order = 2)]
        [HttpPost]
        public async Task<IActionResult> Post_LoginAsync([FromBody] DriverLoginContract driverLoginContract)
        {
            var validationResult = await new DriverLoginValidator().ValidateAsync(driverLoginContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter(); 
            
            var isApproved =
                await new DriverService(serviceProvider, executingRequestContextAdapter).IsDriverApprovedAsync(
                    driverLoginContract.Username);

            if (isApproved == false)
            {
                validationResult.Errors.Add(new ValidationFailure(nameof(DriverLoginContract), 
                    "Your account application under review. we will send you email when the application is approved."));
                
                return validationResult.ConvertToBadRequest();
            }
            
            var identity = await GetClaimsIdentityAsync(driverLoginContract.Username, driverLoginContract.Password, executingRequestContextAdapter);
            
            if (identity == null)
            {
                validationResult.Errors.Add(new ValidationFailure(nameof(DriverLoginContract), 
                    "Invalid username or password."));
                
                return validationResult.ConvertToBadRequest();
            }
            
            var jwt = await Tokens.GenerateJwtAsync(identity, jwtFactory, driverLoginContract.Username, jwtOptions, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }

        /// <summary>
        ///  Request email verification
        /// </summary>
        /// <param name="driverStartEmailVerificationContract"></param>
        /// <returns></returns>
        [Route("request-email-otp", Order = 3)]
        [ProducesResponseType(typeof(DriverEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestEmailOtpAsync(
            [FromBody] DriverStartEmailVerificationContract driverStartEmailVerificationContract)
        {
            var validationResult = await new DriverStartEmailVerificationValidator().ValidateAsync(driverStartEmailVerificationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverStartEmailVerificationStatusContract =
                await new DriverStartEmailVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new DriverStartEmailVerificationCommand(driverStartEmailVerificationContract));

            return Ok(driverStartEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Request email verification
        /// </summary>
        /// <param name="driverCheckEmailVerificationContract"></param>
        /// <returns></returns>
        [Route("verify-email-otp", Order = 4)]
        [ProducesResponseType(typeof(DriverEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyEmailOtpAsync(
            [FromBody] DriverCheckEmailVerificationContract driverCheckEmailVerificationContract)
        {
            var validationResult = await new DriverCheckEmailVerificationValidator().ValidateAsync(driverCheckEmailVerificationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverEmailVerificationStatusContract =
                await new DriverCheckEmailVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new DriverCheckEmailVerificationCommand(driverCheckEmailVerificationContract));

            if (driverEmailVerificationStatusContract.Status == "approved")
            {
                await ConfirmEmailAsync(driverCheckEmailVerificationContract, executingRequestContextAdapter);
            }

            return Ok(driverEmailVerificationStatusContract);
        }

        /// <summary>
        ///  Request reset password otp
        /// </summary>
        /// <param name="driverResetPasswordRequestContract"></param>
        /// <returns></returns>
        [Route("request-reset-password-otp", Order = 5)]
        [ProducesResponseType(typeof(DriverResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestResetPasswordAsync(
            [FromBody] DriverResetPasswordRequestContract driverResetPasswordRequestContract)
        {
            var validationResult = await new DriverResetPasswordRequestValidator().ValidateAsync(driverResetPasswordRequestContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var driverResetPasswordStatusContract =
                await new DriverResetPasswordVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new DriverResetPasswordVerificationCommand(driverResetPasswordRequestContract));

            return Ok(driverResetPasswordStatusContract);
        }

        /// <summary>
        ///  Verify otp and reset password
        /// </summary>
        /// <param name="driverResetPasswordCreationContract"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Route("verify-reset-password", Order = 7)]
        [ProducesResponseType(typeof(DriverResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyResetPasswordOtpAsync([FromBody] DriverResetPasswordCreationContract driverResetPasswordCreationContract)
        {
            var validationResult =
                await new DriverCheckResetPasswordVerificationValidator().ValidateAsync(
                    driverResetPasswordCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverResetPasswordStatusContract =
                await new DriverCheckResetPasswordVerificationCommandHandler(serviceProvider,
                        executingRequestContextAdapter)
                    .Handle(new DriverCheckResetPasswordVerificationCommand(driverResetPasswordCreationContract));

            if (driverResetPasswordStatusContract.Status == "approved")
            {
                await using var applicationDbContext =
                    await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
                var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

                var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                    passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

                var user = await userManager.FindByEmailAsync(driverResetPasswordCreationContract.Email);

                if (user == null)
                {
                    throw new InvalidOperationException($"Expected to be found {driverResetPasswordCreationContract.Email} user.").WithTelemetry(
                        executingRequestContextAdapter.GetTelemetryProperties());
                }
                
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetResult = await userManager.ResetPasswordAsync(user, token, driverResetPasswordCreationContract.Password);

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

        /// <summary>
        ///  Set driver online or offline
        /// </summary>
        /// <param name="driverActiveCreationContract"></param>
        /// <returns></returns>
        [Route("active", Order = 8)]
        [ProducesResponseType(typeof(DriverActiveStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        [Authorize(Policy = "DriverApiUser")]
        public async Task<IActionResult> Post_ActiveDriverAsync(
            [FromBody] DriverActiveCreationContract driverActiveCreationContract)
        {
            var authenticatedUser = Request.GetAuthenticatedUser("driver");
            
            var validationResult =
                await new DriverActiveValidator(authenticatedUser.UserEmail!).ValidateAsync(
                    driverActiveCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverActiveStatusContract = new DriverActiveStatusContract
            {
                IsActive = true,
                DateCreated = DateTimeOffset.UtcNow
            };

            var driverActiveMessageContract = new DriverActiveMessageContract
            {
                PayloadIn = driverActiveCreationContract,
                PayloadOut = driverActiveStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverActiveMessagePublisher(serviceProvider).PublishAsync(
                driverActiveMessageContract);

            return Ok(driverActiveStatusContract);
        }

        private async Task ConfirmEmailAsync(DriverCheckEmailVerificationContract driverCheckEmailVerificationContract,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            await using var applicationDbContext =
                await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

            var user = await userManager.FindByEmailAsync(driverCheckEmailVerificationContract.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Expected to be found user.").WithTelemetry(
                    executingRequestContextAdapter.GetTelemetryProperties());
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
        }

        private async Task<bool> CreateUserAsync(DriverCreationContract driverCreationContract, IExecutingRequestContextAdapter executingRequestContextAdapter, ValidationResult validationResult)
        {
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            using var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var user = new Database.Models.ApplicationUser { UserName = driverCreationContract.EmailAddress, Email = driverCreationContract.EmailAddress };
            var result = await userManager.CreateAsync(user, driverCreationContract.Password);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Driver");
                var claim = new Claim(ClaimData.DriverApiAccess.ClaimType, ClaimData.DriverApiAccess.ClaimValue, ClaimValueTypes.String);
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
                return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id, claimList, roleList.ToList(), executingRequestContextAdapter));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity?>(null);
        }
        
    }
}