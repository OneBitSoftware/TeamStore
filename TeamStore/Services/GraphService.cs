using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TeamStore.Interfaces;

namespace TeamStore.Services
{
    public class GraphService : IGraphService
    {
        private readonly IMemoryCache _memoryCache;

        private string _aadInstance; 
        private readonly string _appId; 
        private readonly string _appSecret; 
        private readonly string _tenantId; 
        private readonly string _graphResourceId; 
        private readonly string _redirectUri;

        private GraphServiceClient _graphClient = null;

        public GraphService(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _redirectUri = configuration.GetValue<string>("AzureAd:CallbackPath");
            _aadInstance = configuration.GetValue<string>("AzureAd:Instance");
            _appId = configuration.GetValue<string>("AzureAd:ClientId");
            _tenantId = configuration.GetValue<string>("AzureAd:TenantId");
            _graphResourceId = configuration.GetValue<string>("AzureAd:GraphResourceId");

            // This code takes the value from the user-secrets capability in dotnet
            // running "dotnet user-secrets list" will list all secrets
            // we don't want it in the appsettings.json file
            _appSecret = configuration.GetValue<string>("Authentication:AzureAd:ClientSecret");
            if (string.IsNullOrWhiteSpace(_appSecret)) throw new ArgumentException("Client Secret is unknown. Terminating.");
        }

        // Get an authenticated Microsoft Graph Service client.
        public GraphServiceClient GetAuthenticatedClient(string userId)
        {
            _graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    // Passing tenant ID to the sample auth provider to use as a cache key
                    string accessToken = await GetUserAccessTokenAsync(userId);

                    // Append the access token to the request
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }));

            return _graphClient;
        }

        // Gets an access token. First tries to get the access token from the token cache.
        // Using password (secret) to authenticate. Production apps should use a certificate.
        public async Task<string> GetUserAccessTokenAsync(string userId)
        {
            TokenCache userTokenCache = new SessionCacheService(userId, _memoryCache).GetCacheInstance();

            try
            {
                AuthenticationContext authContext = new AuthenticationContext(_aadInstance, userTokenCache);
                ClientCredential credential = new ClientCredential(_appId, _appSecret);
                AuthenticationResult result = await authContext.AcquireTokenSilentAsync(
                    _graphResourceId,
                    credential,
                    new UserIdentifier(userId, UserIdentifierType.UniqueId));

                return result.AccessToken;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Gets a token by Authorization Code.
        // Using password (secret) to authenticate. Production apps should use a certificate.
        public async Task<AuthenticationResult> GetTokenByAuthorizationCodeAsync(string userId, string code, string redirectHost)
        {
            TokenCache userTokenCache = new SessionCacheService(userId, _memoryCache).GetCacheInstance();

            if (!_aadInstance.Last().Equals('/'))
                _aadInstance = _aadInstance + "/";

            _aadInstance = _aadInstance + _tenantId;

            try
            {
                AuthenticationContext authContext = new AuthenticationContext(_aadInstance, userTokenCache);
                ClientCredential credential = new ClientCredential(_appId, _appSecret);
                AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                    code,
                    new Uri(redirectHost + _redirectUri),
                    credential,
                    _graphResourceId);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
