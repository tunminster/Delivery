using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Models;
using Delivery.Domain.Factories;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.Models;
using Delivery.User.Domain.CommandHandlers;
using Delivery.User.Domain.Contracts.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Delivery.User.Domain.ApplicationServices
{
    public class AccountService
    {
        private readonly FacebookService facebookService;
        private readonly JwtCommandHandler jwtCommandHandler;
        private readonly IServiceProvider serviceProvider;
        private readonly UserManager<Database.Models.ApplicationUser> userManager;
        private readonly IJwtFactory jwtFactory;
        private readonly JwtIssuerOptions jwtOptions;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public AccountService(IServiceProvider serviceProvider, UserManager<Database.Models.ApplicationUser> userManager, 
            IJwtFactory jwtFactory, 
            JwtIssuerOptions jwtOptions,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            facebookService = new FacebookService();
            jwtCommandHandler = new JwtCommandHandler(serviceProvider);
            this.serviceProvider = serviceProvider;
            this.userManager = userManager;
            this.jwtFactory = jwtFactory;
            this.jwtOptions = jwtOptions;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        
        
        public async Task<FacebookAuthorizationTokenContract> FacebookLoginAsync(FacebookLoginContract facebookLoginContract)
        {
            if (string.IsNullOrEmpty(facebookLoginContract.FacebookToken))
            {
                throw new Exception("Token is null or empty");
            }

            var facebookUser = await facebookService.GetUserFromFacebookAsync(facebookLoginContract.FacebookToken);
            var facebookEmail = $"{facebookUser.Email}_{UniqueIdFactory.UniqueFacebookId()}@facebooklogin.com";
            var user = new Database.Models.ApplicationUser { UserName = facebookUser.Email, Email = facebookEmail };
            var domainUser = await userManager.FindByNameAsync(facebookUser.Email);
            if (domainUser == null)
            {
                
                user = new Database.Models.ApplicationUser { UserName = $"{facebookUser.FirstName}_{facebookUser.LastName}", Email = facebookEmail };
                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    var userLoginInfo = new UserLoginInfo
                        ("Facebook", "Facebook", user.UserName);
                        
                    result = await userManager.AddLoginAsync(user, userLoginInfo);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddToRoleAsync(user, "Customer");
                        var claim = new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue, ClaimValueTypes.String);
                        await userManager.AddClaimAsync(user, claim);
                    }
                }
            }
            
            return await CreateAccessTokensAsync(user, "Customer");
        }
        
        public async Task<string> FacebookLoginTokenAsync(FacebookLoginContract facebookLoginContract)
        {
            if (string.IsNullOrEmpty(facebookLoginContract.FacebookToken))
            {
                throw new Exception("Token is null or empty");
            }

            var facebookUser = await facebookService.GetUserFromFacebookAsync(facebookLoginContract.FacebookToken);
            var facebookEmail = $"{facebookUser.Email}_{UniqueIdFactory.UniqueFacebookId()}@facebooklogin.com";
            var user = new Database.Models.ApplicationUser { UserName = facebookUser.Email, Email = facebookEmail };
            var domainUser = await userManager.FindByNameAsync(facebookUser.Email);
            if (domainUser == null)
            {
                
                user = new Database.Models.ApplicationUser { UserName = $"{facebookUser.FirstName}_{facebookUser.LastName}", Email = facebookEmail };
                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    var userLoginInfo = new UserLoginInfo
                        ("Facebook", "Facebook", user.UserName);
                        
                    result = await userManager.AddLoginAsync(user, userLoginInfo);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                        var claim = new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue, ClaimValueTypes.String);
                        
                        var groupClaim = new Claim("groups", executingRequestContextAdapter.GetShard().Key,
                            ClaimValueTypes.String);
                        await userManager.AddClaimAsync(user, claim);
                        await userManager.AddClaimAsync(user, groupClaim);
                    }
                }
            }
            
            return await CreateAccessTokenStringAsync(user, "Customer");
        }

        private async Task<FacebookAuthorizationTokenContract> CreateAccessTokensAsync(ApplicationUser user, string roleName)
        {
            var accessToken = jwtCommandHandler.CreateAccessToken(Guid.Parse(user.Id), user.Email, roleName);
            var refreshToken = jwtCommandHandler.CreateRefreshToken(Guid.Parse(user.Id));
            
            return new FacebookAuthorizationTokenContract { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<string> CreateAccessTokenStringAsync(ApplicationUser user, string roleName)
        {
            var identity = await GetClaimsIdentityAsync(user.UserName);
            
            var jwtToken = await Tokens.GenerateJwt(identity, jwtFactory, user.UserName, jwtOptions, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Formatting.Indented });
            return jwtToken;
        }
        
        private async Task<ClaimsIdentity> GetClaimsIdentityAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return await Task.FromResult<ClaimsIdentity>(null);
            }
                

            // get the user to verifty
            var userToVerify = await userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            return await Task.FromResult(jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id, executingRequestContextAdapter));

        }
    }
}