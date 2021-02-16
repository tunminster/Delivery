using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.Models;
using Delivery.User.Domain.CommandHandlers;
using Delivery.User.Domain.Contracts.Facebook;
using Delivery.User.Domain.Contracts.Google;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Delivery.User.Domain.ApplicationServices
{
    public class GoogleService
    {
        private readonly HttpClient httpClient;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public GoogleService(IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            httpClient = new HttpClient{
                BaseAddress = new Uri("https://oauth2.googleapis.com/")
            };

            this.executingRequestContextAdapter = executingRequestContextAdapter;
            
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<GoogleUserContract> GetUserFromGoogleAsync(string idToken)
        {
            var result = await GetAsync<dynamic>(idToken, "tokeninfo");
            if (result == null)
            {
                throw new InvalidOperationException("User from this token does not exist");
            }
            
            var account = new GoogleUserContract
            {
                Iss = result.iss,
                Sub = result.sub,
                Email = result.email ?? throw new InvalidOperationException("Email scope was not provided from the Id Token"),
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
            {
                throw new InvalidOperationException($"Google token api response is not succeeded for the accessToken:{accessToken} and endpoint: {endpoint}. Actual response:{response.Content.ReadAsStringAsync()}")
                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
            }
            
            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
        
        
    }
}