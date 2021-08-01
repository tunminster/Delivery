using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
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
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Driver.Domain.Services;
using Delivery.Driver.Domain.Validators;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;

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
            JwtIssuerOptions jwtOptions,
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
            this.jwtOptions = jwtOptions;
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
        [HttpPost("register")]
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

            if(!await CreateUserAsync(driverCreationContract, executingRequestContextAdapter))
            {
                throw new InvalidOperationException(
                    $"{nameof(DriverCreationContract)} should be able to create a user account. Instead: throw errors with {driverCreationContract.ConvertToJson()}");
            }

            var driverCreationStatusContract = new DriverCreationStatusContract
            {
                DateCreated = DateTimeOffset.UtcNow,
                Message = "Driver application submitted successfully.",
                ImageUri = driverImageCreationStatusContract.DriverImageUri,
                DrivingLicenseFrontUri = driverImageCreationStatusContract.DrivingLicenseFrontImageUri,
                DrivingLicenseBackUri = driverImageCreationStatusContract.DrivingLicenseBackImageUri
            };

            var driverCreationMessage = new DriverCreationMessageContract
            {
                PayloadIn = driverCreationContract,
                PayloadOut = driverCreationStatusContract
            };
            
            await new DriverCreationMessagePublisher(serviceProvider).PublishAsync(driverCreationMessage);
            
            return Ok(driverCreationStatusContract);
        }

        /// <summary>
        ///  Driver login 
        /// </summary>
        /// <param name="driverLoginContract"></param>
        /// <returns></returns>
        [HttpPost("login")]
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

        private async Task<bool> CreateUserAsync(DriverCreationContract driverCreationContract, IExecutingRequestContextAdapter executingRequestContextAdapter)
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
                var claim = new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue, ClaimValueTypes.String);
                var groupClaim = new Claim("groups", executingRequestContextAdapter.GetShard().Key,
                    ClaimValueTypes.String);
                await userManager.AddClaimAsync(user, claim);


                await userManager.AddClaimAsync(user, groupClaim);
                var customerCreationContract = new CustomerCreationContract
                {
                    IdentityId = user.Id, 
                    Username = user.Email,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    ContactNumber = string.Empty
                };

                var createCustomerCommand = new CreateCustomerCommand(customerCreationContract);
                var createCustomerCommandHandler =
                    new CreateCustomerCommandHandler(serviceProvider, executingRequestContextAdapter); 
                await createCustomerCommandHandler.Handle(createCustomerCommand);

                return true;
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

            // check the credentials
            if (await userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id, claimList, executingRequestContextAdapter));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity?>(null);
        }
        
    }
}