using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Database.Constants;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.Models;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using Delivery.Managements.Domain.Handlers.CommandHandlers;
using Delivery.Managements.Domain.Validators.EmailVerification;
using Delivery.Managements.Domain.Validators.ManagementUserCreation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Management user controller
    /// </summary>
    [Route("api/v1/management-user", Name = "5 - Management user")]
    [PlatformSwaggerCategory(ApiCategory.Management)]
    [ApiController]
    public class ManagementUserController : ControllerBase
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
        public ManagementUserController(IServiceProvider serviceProvider, 
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
        ///  Create a new admin user
        /// </summary>
        /// <remarks>Register a new user with user role.</remarks>
        [Route("register", Order = 1)]
        [ProducesResponseType(typeof(StatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RegisterUserAsync(ManagementUserCreationContract managementUserCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var validationResult = await new ManagementUserCreationValidator().ValidateAsync(managementUserCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            

            if(!await CreateUserAsync(managementUserCreationContract, executingRequestContextAdapter, validationResult))
            {
                return validationResult.ConvertToBadRequest();
            }

            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            return Ok(statusContract);
        }
        
        /// <summary>
        ///  Create a new admin user
        /// </summary>
        /// <remarks>Register a new user with user role.</remarks>
        [Route("add-admin-role", Order = 1)]
        [ProducesResponseType(typeof(StatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        [Authorize(Roles = RoleConstant.Administrator)]
        public async Task<IActionResult> Post_AddAdminRoleAsync(ManagementUserContract managementUserContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var validationResult = await new ManagementUserValidator().ValidateAsync(managementUserContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            using var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);


            var user = await userManager.FindByNameAsync(managementUserContract.Email);
            if (user.EmailConfirmed)
            {
                await userManager.AddToRoleAsync(user, RoleConstant.Administrator);
            }
            
            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            return Ok(statusContract);
        }
        
        /// <summary>
        ///  Request email verification
        /// </summary>
        [Route("request-email-otp", Order = 3)]
        [ProducesResponseType(typeof(ManagementUserEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestEmailOtpAsync(
            [FromBody] ManagementUserEmailVerificationRequestContract managementUserEmailVerificationRequestContract)
        {
            var validationResult = await new ManagementUserEmailVerificationRequestValidator().ValidateAsync(managementUserEmailVerificationRequestContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var managementUserEmailVerificationStatusContract =
                await new ManagementUserEmailVerificationRequestCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new ManagementUserEmailVerificationRequestCommand(managementUserEmailVerificationRequestContract));

            return Ok(managementUserEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Verify email verification 
        /// </summary>
        /// <returns></returns>
        [Route("verify-email-otp", Order = 4)]
        [ProducesResponseType(typeof(ManagementUserEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyEmailOtpAsync(
            [FromBody] ManagementUserEmailVerificationContract managementUserEmailVerificationContract)
        {
            var validationResult = await new ManagementUserEmailVerificationValidator().ValidateAsync(managementUserEmailVerificationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var managementUserEmailVerificationStatusContract =
                await new ManagementUserEmailVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new ManagementUserEmailVerificationCommand(managementUserEmailVerificationContract));

            if (managementUserEmailVerificationStatusContract.Status == "approved")
            {
                await ConfirmEmailAsync(managementUserEmailVerificationContract, executingRequestContextAdapter);
            }

            return Ok(managementUserEmailVerificationStatusContract);
        }
        
        private async Task ConfirmEmailAsync(ManagementUserEmailVerificationContract managementUserEmailVerificationContract,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            await using var applicationDbContext =
                await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

            var user = await userManager.FindByEmailAsync(managementUserEmailVerificationContract.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Expected to be found user.").WithTelemetry(
                    executingRequestContextAdapter.GetTelemetryProperties());
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
        }
        
        
        
        private async Task<bool> CreateUserAsync(ManagementUserCreationContract managementUserCreationContract, IExecutingRequestContextAdapter executingRequestContextAdapter, ValidationResult validationResult)
        {
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            using var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);
            
            var user = new Database.Models.ApplicationUser { UserName = managementUserCreationContract.EmailAddress, Email = managementUserCreationContract.EmailAddress };
            var result = await userManager.CreateAsync(user, managementUserCreationContract.Password);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleConstant.User);
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