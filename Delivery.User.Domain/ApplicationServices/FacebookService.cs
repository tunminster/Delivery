using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Factories;
using Delivery.User.Domain.Contracts.Facebook;
using Newtonsoft.Json;

namespace Delivery.User.Domain.ApplicationServices
{
    public class FacebookService
    {
        private readonly HttpClient httpClient;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public FacebookService(IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            httpClient = new HttpClient{
                BaseAddress = new Uri("https://graph.facebook.com/v2.8/")
            };

            this.executingRequestContextAdapter = executingRequestContextAdapter;
            
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<FacebookUserContract> GetUserFromFacebookAsync(string facebookToken)
        {
            var result = await GetAsync<dynamic>(facebookToken, "me", "fields=id,first_name,last_name,email,picture.width(100).height(100)");
            if (result == null)
            {
                throw new InvalidOperationException("The provided facebook token returned null.User from this token does not exist")
                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
            }

            var account = new FacebookUserContract()
            {
                Email = result.email,
                FirstName = result.first_name,
                LastName = result.last_name,
                Picture = result.picture.data.url
            };

            return account;
        }
        
        private async Task<T> GetAsync<T>(string accessToken, string endpoint, string args = null)
        {
            var response = await httpClient.GetAsync($"{endpoint}?access_token={accessToken}&{args}");
            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}