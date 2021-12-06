using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Database.Constants;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.Factories;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Domain.Models;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.ResetPassword;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.UpdateProfile;
using Delivery.Managements.Domain.Handlers.CommandHandlers;
using Delivery.Managements.Domain.Handlers.CommandHandlers.ResetPassword;
using Delivery.Managements.Domain.Validators.EmailVerification;
using Delivery.Managements.Domain.Validators.ManagementUserCreation;
using Delivery.Managements.Domain.Validators.ResetPassword;
using Delivery.Managements.Domain.Validators.UpdateProfile;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopUsers;
using Delivery.Shop.Domain.Handlers.QueryHandlers.ShopUsers;
using Delivery.User.Domain.Contracts.V1.RestContracts.Managements;
using Delivery.User.Domain.Handlers.QueryHandlers;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Management user controller
    /// </summary>
    [Route("api/v1/management/management-user", Name = "5 - Management user")]
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
        ///  Management user controller
        /// </summary>
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
        ///  Add admin role
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

            using var userManager = new UserManager<ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);


            var user = await userManager.FindByNameAsync(managementUserContract.Email);
            if (!user.EmailConfirmed)
            {
                throw new InvalidOperationException($"{managementUserContract.Email} has not been confirmed.");
            }
            
            await userManager.AddToRoleAsync(user, RoleConstant.Administrator);
            var claim = new Claim(ClaimData.AdminUserAccess.ClaimType, ClaimData.AdminUserAccess.ClaimValue, ClaimValueTypes.String);
            var groupClaim = new Claim("groups", executingRequestContextAdapter.GetShard().Key,
                ClaimValueTypes.String);
                
            await userManager.AddClaimAsync(user, claim);
            await userManager.AddClaimAsync(user, groupClaim);
            
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
        
        /// <summary>
        ///  Request reset password otp
        /// </summary>
        /// <returns></returns>
        [Route("request-reset-password-otp", Order = 5)]
        [ProducesResponseType(typeof(UserManagementResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestResetPasswordAsync(
            [FromBody] UserManagementResetPasswordRequestContract userManagementResetPasswordRequestContract)
        {
            var validationResult = await new UserManagementResetPasswordRequestValidator().ValidateAsync(userManagementResetPasswordRequestContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var userManagementResetPasswordStatusContract =
                await new UserManagementResetPasswordVerificationRequestCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new UserManagementResetPasswordVerificationRequestCommand(userManagementResetPasswordRequestContract));

            return Ok(userManagementResetPasswordStatusContract);
        }
        
        /// <summary>
        ///  Verify otp and reset password
        /// </summary>
        [Route("verify-reset-password", Order = 7)]
        [ProducesResponseType(typeof(UserManagementResetPasswordStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyResetPasswordOtpAsync([FromBody] UserManagementResetPasswordCreationContract userManagementResetPasswordCreationContract)
        {
            var validationResult =
                await new UserManagementResetPasswordVerificationValidator().ValidateAsync(
                    userManagementResetPasswordCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverResetPasswordStatusContract =
                await new UserManagementResetPasswordVerificationCommandHandler(serviceProvider,
                        executingRequestContextAdapter)
                    .Handle(new UserManagementResetPasswordVerificationCommand(userManagementResetPasswordCreationContract));

            if (driverResetPasswordStatusContract.Status == "approved")
            {
                await using var applicationDbContext =
                    await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
                
                
                var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

                var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                    passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

                var user = await userManager.FindByEmailAsync(userManagementResetPasswordCreationContract.Email);

                if (user == null)
                {
                    throw new InvalidOperationException($"Expected to be found {userManagementResetPasswordCreationContract.Email} user.").WithTelemetry(
                        executingRequestContextAdapter.GetTelemetryProperties());
                }
                
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetResult = await userManager.ResetPasswordAsync(user, token, userManagementResetPasswordCreationContract.Password);

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
        ///  User login 
        /// </summary>
        [Route("login", Order = 8)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_LoginAsync([FromBody] ManagementUserLoginContract driverLoginContract)
        {
            var validationResult = await new ManagementUserLoginValidator().ValidateAsync(driverLoginContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter(); 
            
            
            var identity = await GetClaimsIdentityAsync(driverLoginContract.Username, driverLoginContract.Password, executingRequestContextAdapter);
            
            
            var jwt = await Tokens.GenerateJwtAsync(identity, jwtFactory, driverLoginContract.Username, jwtOptions, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }

        /// <summary>
        ///  Get role
        /// </summary>
        ///<remarks>The endpoint allows user to get role name.</remarks>
        [Route("get-role", Order = 6)]
        [ProducesResponseType(typeof(ManagementUserRoleContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpGet]
        [Authorize(Roles = "ShopOwner,Administrator")]
        public async Task<IActionResult> Get_RoleAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var role = executingRequestContextAdapter.GetAuthenticatedUser().Role;

            if (string.Equals(role, "administrator", StringComparison.InvariantCultureIgnoreCase))
            {
                return Ok(await Task.FromResult(new ManagementUserRoleContract { Role = "SuperAdmin" }));
            }

            return Ok(await Task.FromResult(new ManagementUserRoleContract() { Role = "Admin" }));
        }
        
        /// <summary>
        ///  Get user profile
        /// </summary>
        ///<remarks>The endpoint allows user to get user profile.</remarks>
        [Route("get-user-profile", Order = 7)]
        [ProducesResponseType(typeof(UserProfileContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpGet]
        [Authorize(Roles = "ShopOwner,Administrator")]
        public async Task<IActionResult> Get_UserProfileAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;

            var userProfileContract = await new UserGetQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(new UserGetQuery(userEmail!));

            return Ok(userProfileContract);
        }

        /// <summary>
        ///  Update user profile
        /// </summary>
        [Route("update-user-profile", Order = 8)]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpPut]
        [Authorize(Roles = "ShopOwner,Administrator")]
        public async Task<IActionResult> Update_ProfileAsync(
            UpdateProfileCreationContract updateProfileCreationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var validationResult =
                await new UpdateProfileCreationValidator().ValidateAsync(updateProfileCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            await using var applicationDbContext =
                await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
                
                
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider, logger);

            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;

            var user = await userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                throw new InvalidOperationException($"Expected to be found {userEmail} user.").WithTelemetry(
                    executingRequestContextAdapter.GetTelemetryProperties());
            }
                
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResetResult = await userManager.ResetPasswordAsync(user, token, updateProfileCreationContract.Password);

            if (passwordResetResult.Succeeded)
            {
                return Ok(new StatusContract{ Status = true, DateCreated = DateTimeOffset.UtcNow});
            }
            
            foreach (var error in passwordResetResult.Errors)
            {
                validationResult.Errors.Add(new ValidationFailure(error.Code, error.Description));
            }
                
            return validationResult.ConvertToBadRequest();
        }
        

        /// <summary>
        ///  Get user list
        /// </summary>
        /// <remarks>The endpoint allows user to get shop user list</remarks>
        [Route("get-users", Order = 9)]
        [ProducesResponseType(typeof(ShopUsersPageContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Get_UsersAsync(string pageNumber, string pageSize)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            if (string.IsNullOrEmpty(pageNumber) || string.IsNullOrEmpty(pageSize))
            {
                var errorMessage = $"{nameof(pageNumber)} and {nameof(pageSize)} are required";

                return errorMessage.ConvertToBadRequest();
            }

            int.TryParse(pageNumber, out var iPageNumber);
            int.TryParse(pageSize, out var iPageSize);
            
            var shopUserAllQuery = new ShopUserAllQuery(iPageNumber, iPageSize);
            
            var shopUsersPageContract = await new ShopUserAllQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(shopUserAllQuery);
            
            return Ok(shopUsersPageContract);
        }

        /// <summary>
        ///  Get user by id
        /// </summary>
        [Route("get-user", Order = 10)]
        [ProducesResponseType(typeof(ShopUserContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Get_UserAsync(string userId)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var shopUserGetQuery = new ShopUserGetQuery(userId);

            var shopUserContract = await new ShopUserGetQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(shopUserGetQuery);

            return Ok(shopUserContract);
        }
        
        private async Task<ClaimsIdentity?> GetClaimsIdentityAsync(string userName, string password, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity?>(null);
            
            await using var applicationDbContext = await ApplicationDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var store = new UserStore<Database.Models.ApplicationUser>(applicationDbContext);

            var userManager = new UserManager<Database.Models.ApplicationUser>(store, optionsAccessor,
                passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, serviceProvider,logger);

            // get the user to verify
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