using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.User.Domain.Contracts.Facebook;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;


namespace Delivery.User.Domain.CommandHandlers
{
    public class JwtCommandHandler
    {
        private IServiceProvider serviceProvider;
        public JwtCommandHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public TokenResource CreateAccessToken(Guid userId, string email, string role)
        {
            var now = DateTime.UtcNow;
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.Ticks.ToString(), ClaimValueTypes.Integer64),
            };

            var configurationProvider =  serviceProvider.GetRequiredService<IConfigurationProvider>();

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationProvider.GetSetting("Tokens:Key"))),
                SecurityAlgorithms.HmacSha256);
            //var expiry = now.AddMinutes(double.Parse(configuration["Tokens:AccessExpireMinutes"]));
            var expiry = DateTimeOffset.UtcNow.AddMonths(3);
            var jwt = CreateSecurityToken(claims, now, expiry.DateTime, signingCredentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return CreateTokenResource(token, expiry.ToUnixTimeSeconds());
        }
        
        public TokenResource CreateRefreshToken(Guid userId)
        {
            var now = DateTimeOffset.UtcNow;
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };
            var configurationProvider =  serviceProvider.GetRequiredService<IConfigurationProvider>();

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationProvider.GetSetting("Tokens:Key"))),
                SecurityAlgorithms.HmacSha256);
            //var expiry = now.AddMinutes(double.Parse(configuration["Tokens:RefreshExpireMinutes"]));
            var expiry = DateTimeOffset.UtcNow.AddMonths(3);
            var jwt = CreateSecurityToken(claims, now.DateTime, expiry.DateTime, signingCredentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return CreateTokenResource(token, expiry.ToUnixTimeSeconds());
        }

        private JwtSecurityToken CreateSecurityToken(IEnumerable<Claim> claims, DateTime now, DateTime expiry, SigningCredentials credentials)
            => new JwtSecurityToken(claims: claims, notBefore: now, expires: expiry, signingCredentials: credentials);

        private static TokenResource CreateTokenResource(string token, long expiry)
            => new TokenResource { Token = token, Expiry = expiry };
    }
}