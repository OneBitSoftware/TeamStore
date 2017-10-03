﻿using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class GraphTestBase : IntegrationTestBase
    {
        private readonly string clientId;
        private readonly string userName;
        private readonly string password;
        private readonly string contentType = "application/x-www-form-urlencoded";
        // Don't use password grant in your apps. Only use for legacy solutions and automated testing.
        private readonly string grantType = "password";
        private readonly string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";
        private readonly string resourceId = "https%3A%2F%2Fgraph.microsoft.com%2F";

        private static string accessToken = null;
        private static string tokenForUser = null;
        private static System.DateTimeOffset expiration;

        protected static GraphServiceClient _graphClient = null;

        public GraphTestBase()
        {
            // Setup for Integration tests and CI
            clientId = System.Environment.GetEnvironmentVariable("test_client_id");
            userName = System.Environment.GetEnvironmentVariable("test_user_name");
            password = System.Environment.GetEnvironmentVariable("test_password");

            GetAuthenticatedClient();
        }

        // Get an access token and provide a GraphServiceClient.
        private void GetAuthenticatedClient()
        {
            if (_graphClient == null)
            {
                // Create Microsoft Graph client.
                try
                {
                    _graphClient = new GraphServiceClient(
                        "https://graph.microsoft.com/v1.0",
                        new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                var token = await GetAccessTokenUsingPasswordGrant();
                                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

                            }));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Could not create a graph client: " + ex.Message);
                }
            }
        }

        private async Task<string> GetAccessTokenUsingPasswordGrant()
        {
            JObject jResult = null;
            String urlParameters = String.Format(
                    "grant_type={0}&resource={1}&client_id={2}&username={3}&password={4}",
                    grantType,
                    resourceId,
                    clientId,
                    userName,
                    password
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

            accessToken = (string)jResult["access_token"];

            if (!String.IsNullOrEmpty(accessToken))
            {
                //Set AuthenticationHelper values so that the regular MSAL auth flow won't be triggered.
                tokenForUser = accessToken;
                expiration = DateTimeOffset.UtcNow.AddHours(5);
            }

            return accessToken;
        }
    }
}
