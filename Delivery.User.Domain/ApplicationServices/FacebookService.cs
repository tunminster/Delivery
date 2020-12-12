using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Delivery.User.Domain.Contracts.Facebook;
using Newtonsoft.Json;

namespace Delivery.User.Domain.ApplicationServices
{
    public class FacebookService
    {
        private readonly HttpClient httpClient;

        public FacebookService()
        {
            httpClient = new HttpClient{
                BaseAddress = new Uri("https://graph.facebook.com/v2.8/")
            };
            
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<FacebookUserContract> GetUserFromFacebookAsync(string facebookToken)
        {
            var result = await GetAsync<dynamic>(facebookToken, "me", "fields=first_name,last_name,email,picture.width(100).height(100)");
            if (result == null)
            {
                throw new Exception("User from this token not exist");
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