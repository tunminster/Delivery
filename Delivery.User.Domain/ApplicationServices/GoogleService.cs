using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.Models;
using Delivery.User.Domain.CommandHandlers;
using Delivery.User.Domain.Contracts.Facebook;
using Delivery.User.Domain.Contracts.Google;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Delivery.User.Domain.ApplicationServices
{
    public class GoogleService
    {
        private readonly HttpClient httpClient;

        public GoogleService()
        {
            httpClient = new HttpClient{
                BaseAddress = new Uri("https://oauth2.googleapis.com/")
            };
            
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<GoogleUserContract> GetUserFromGoogleAsync(string idToken)
        {
            var result = await GetAsync<dynamic>(idToken, "tokeninfo");
            if (result == null)
            {
                throw new Exception("User from this token not exist");
            }

            var account = new GoogleUserContract
            {
                Iss = result.iss,
                Sub = result.sub,
                Email = result.email ?? result.id,
                EmailVerified = result.email_verified == "true",
                Name = result.name,
                Picture = result.picture,
                GivenName = result.given_name,
                FamilyName = result.family_name,
                Locale = result.locale
            };

            return account;
        }
        

        private async Task<T> GetAsync<T>(string accessToken, string endpoint)
        {
            var response = await httpClient.GetAsync($"{endpoint}?id_token={accessToken}");
            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
        
    }
}