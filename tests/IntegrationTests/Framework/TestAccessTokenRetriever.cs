using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TeamStore.Keeper.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace IntegrationTests.Framework
{
    // THIS IS WORK IN PROGRESS
    public class TestAccessTokenRetriever : IAccessTokenRetriever
    {
        private readonly string grantType = "password";
        private readonly string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";
        private readonly string contentType = "application/x-www-form-urlencoded";

        public async Task<string> GetGraphAccessTokenAsync(
            string userId,
            string _aadInstance,
            string _appId,
            string _appSecret,
            string _tenantId,
            IMemoryCache _memoryCache,
            string _graphResourceId)
        {
            JObject jResult = null;
            String urlParameters = String.Format(
                    "grant_type={0}&resource={1}&client_id={2}&client_secret={3}&username={4}&password={5}",
                    "password",
                    "https%3A%2F%2Fgraph.microsoft.com%2F",
                    _appId,
                    _appSecret,
                    "",
                    ""
            );
            HttpClient client = new HttpClient();
            var createBody = new StringContent(urlParameters, System.Text.Encoding.UTF8, contentType);

            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, createBody);

            if (response.IsSuccessStatusCode)
            {
                Task<string> responseTask = response.Content.ReadAsStringAsync();
                responseTask.Wait();
                string responseContent = responseTask.Result;
                jResult = JObject.Parse(responseContent);
            }

            if (jResult == null) return null;

            var accessToken = (string)jResult["access_token"];

            if (!String.IsNullOrEmpty(accessToken))
            {
                //Set AuthenticationHelper values so that the regular MSAL auth flow won't be triggered.
                var tokenForUser = accessToken;
                var expiration = DateTimeOffset.UtcNow.AddHours(5);
            }

            return accessToken;
        }
    }
}
