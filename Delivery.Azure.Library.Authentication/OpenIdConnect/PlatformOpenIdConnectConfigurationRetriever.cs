using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Delivery.Azure.Library.Authentication.OpenIdConnect
{
    public class PlatformOpenIdConnectConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            var doc = await retriever.GetDocumentAsync(address, cancel);

            var openIdConnectConfiguration = doc.ConvertFromJson<OpenIdConnectConfiguration>();
            if (!string.IsNullOrEmpty(openIdConnectConfiguration.JwksUri))
            {
                var keys = await retriever.GetDocumentAsync(openIdConnectConfiguration.JwksUri, cancel);

                openIdConnectConfiguration.JsonWebKeySet = keys.ConvertFromJson<JsonWebKeySet>();
                foreach (var key in openIdConnectConfiguration.JsonWebKeySet.GetSigningKeys())
                {
                    openIdConnectConfiguration.SigningKeys.Add(key);
                }
            }

            return openIdConnectConfiguration;
        }
    }
}