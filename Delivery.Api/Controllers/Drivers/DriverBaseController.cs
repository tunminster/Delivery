using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Models;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.Models;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers.Drivers
{
    public class DriverBaseController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IJwtFactory jwtFactory;
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
        /// <param name="jwtFactory"></param>
        /// <param name="keyNormalizer"></param>
        /// <param name="errors"></param>
        /// <param name="logger"></param>
        public DriverBaseController(IServiceProvider serviceProvider, 
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<Database.Models.ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<Database.Models.ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<Database.Models.ApplicationUser>> passwordValidators,
            IJwtFactory jwtFactory,
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
            this.keyNormalizer = keyNormalizer;
            this.errors = errors;
            this.logger = logger;
        }
        
        protected async Task<bool> CreateUserAsync(DriverCreationContract driverCreationContract, IExecutingRequestContextAdapter executingRequestContextAdapter, ValidationResult validationResult)
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
        
        protected async Task<ClaimsIdentity?> GetClaimsIdentityAsync(string userName, string password, IExecutingRequestContextAdapter executingRequestContextAdapter)
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