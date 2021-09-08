using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Models;
using Delivery.Domain.Models;
using ServiceStack;

namespace Delivery.Domain.Factories.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;

        public JwtFactory(Microsoft.Extensions.Options.IOptions<JwtIssuerOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }
        
        public async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity)
        {
            var claimList = new List<Claim>();
            // Todo: refactor
            // var claims = new[]
            // {
            //      new Claim(JwtRegisteredClaimNames.Sub, userName),
            //      new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
            //      new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
            //      
            //      // default api_access claim
            //      new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue),
            //      
            //      
            //      identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Role),
            //      identity.FindFirst("groups"),
            //      identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
            //  };

            claimList.Add(new Claim(JwtRegisteredClaimNames.Sub, userName));
            claimList.Add(new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()));
            claimList.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64));
            
            claimList.AddRange(identity.Claims.Select(item => new Claim(item.Type.ToLower(), item.Value)));
            
            

            // if (claimList.Any(x => string.Equals(x.Type.ToLower(), ClaimData.JwtClaimIdentifyClaim.ClaimType.ToLower(),
            //     StringComparison.Ordinal)))
            // {
            //     claimList.Add(new Claim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue));
            // }
            // else if (claimList.Any(x => string.Equals(x.Type.ToLower(),
            //     ClaimData.DriverApiAccess.ClaimType.ToLower(),
            //     StringComparison.Ordinal)))
            // {
            //     claimList.Add(new Claim(ClaimData.DriverApiAccess.ClaimType, ClaimData.DriverApiAccess.ClaimValue));
            // }
            

            //claimList.Add(identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Role));
            
            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claimList,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public ClaimsIdentity GenerateClaimsIdentity(string userName, string id, IList<Claim> claimList, List<string> roles, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            claimList.Add(new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Id, id));

            foreach (var item in roles)
            {
                claimList.Add(new Claim("role", item));
            }

            // var hasApiAccess = claimList.FirstOrDefault(x =>
            //     x.Type.ToLower() == Helpers.Constants.Strings.JwtClaimIdentifiers.Role);

            // if (hasApiAccess?.Value == "api_access")
            // {
            //     return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            //     {
            //         new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Id, id),
            //         new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Role, Helpers.Constants.Strings.JwtClaims.ApiAccess),
            //         new Claim("groups", executingRequestContextAdapter.GetShard().Key)
            //     });
            // }
            // Todo: needs to return claim list 
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), claimList);
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }
    }
}