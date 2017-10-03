namespace TeamStore.Keeper.Services
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Graph;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Factories;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Models;


    // NOTE: This is copied from somewhere and is quite shit.
    // Need to move userId/currentUser around.
    public class GraphService : IGraphService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAccessTokenRetriever _tokenRetriever;

        private string _aadInstance;
        private readonly string _appId;
        private readonly string _appSecret;
        private readonly string _tenantId;
        private readonly string _graphResourceId;
        private readonly string _redirectUri;

        private GraphServiceClient _graphClient = null;

        public GraphService(IMemoryCache memoryCache, IConfiguration configuration, IAccessTokenRetriever tokenRetriever)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _tokenRetriever = tokenRetriever ?? throw new ArgumentNullException(nameof(tokenRetriever));
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
            //RA: here we could add a if (_graphClient != null) return _graphClient;

            _graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    // Passing tenant ID to the sample auth provider to use as a cache key
                    string accessToken = await _tokenRetriever.GetGraphAccessTokenAsync(
                           userId,
                           _aadInstance,
                           _appId,
                           _appSecret,
                           _tenantId,
                           _memoryCache,
                           _graphResourceId);

                    // Append the access token to the request
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }));

            return _graphClient;
        }

        // Gets a token by Authorization Code.
        // Using password (secret) to authenticate. Production apps should use a certificate.
        public async Task<AuthenticationResult> GetTokenByAuthorizationCodeAsync(string userId, string code, string redirectHost)
        {
            TokenCache userTokenCache = new SessionCacheService(userId, _memoryCache).GetCacheInstance();

            if (!_aadInstance.Last().Equals('/')) // TODO: extract and DRY
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
                // TODO: log ex
                return null;
            }
        }

        public async Task<List<ApplicationGroup>> GetGroups(string prefix, string userObjectId)
        {
            var graphClient = GetAuthenticatedClient(userObjectId);

            List<ApplicationGroup> items = new List<ApplicationGroup>();

            // Get groups.
            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().GetAsync();

            if (groups?.Count > 0)
            {
                foreach (Group group in groups)
                {
                    items.Add(new ApplicationGroup
                    {
                        DisplayName = group.DisplayName,
                        AzureAdObjectIdentifier = group.Id
                    });
                }
            }

            return items;
        }

        public async Task<ApplicationGroup> GetGroup(string azureAdObjectId, string userObjectId)
        {
            var graphClient = GetAuthenticatedClient(userObjectId);

            List<ApplicationGroup> items = new List<ApplicationGroup>();

            // Get groups by ID - TODO
            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().GetAsync();

            if (groups?.Count > 0)
            {
                foreach (Group group in groups)
                {
                    items.Add(new ApplicationGroup
                    {
                        DisplayName = group.DisplayName,
                        AzureAdObjectIdentifier = group.Id
                    });
                }
            }

            return items.FirstOrDefault();
        }

        /// <summary>
        /// Gets the groups for which a given user is a member of
        /// </summary>
        /// <param name="userId">The Id of the User to check</param>
        /// <returns>A Task of ApplicationGroup items</returns>
        public async Task<List<ApplicationGroup>> GetGroupMembershipForUser(string userId)
        {
            List<ApplicationGroup> items = new List<ApplicationGroup>();
            var graphClient = GetAuthenticatedClient(userId);

            // Get groups the current user is a direct member of.
            IUserMemberOfCollectionWithReferencesPage memberOfGroups = await graphClient.Me.MemberOf.Request().GetAsync();

            if (memberOfGroups?.Count > 0)
            {
                foreach (var directoryObject in memberOfGroups)
                {

                    // We only want groups, so ignore DirectoryRole objects.
                    if (directoryObject is Group)
                    {
                        Group group = directoryObject as Group;
                        items.Add(new ApplicationGroup
                        {
                            DisplayName = group.DisplayName,
                            AzureAdObjectIdentifier = group.Id
                        });
                    }

                }
            }
            return items;
        }
        
        // TODO: Implement with Func
        public async Task<ApplicationUser> ResolveUserByObjectIdAsync(string azureAdObjectIdentifier, string currentUserId)
        {
            var graphClient = GetAuthenticatedClient(currentUserId);
            try
            {
                var user = await graphClient.Users[azureAdObjectIdentifier].Request().GetAsync();
                var mappedUser = UserIdentityFactory.MapApplicationUser(user);
                mappedUser.TenantId = _tenantId;
                return mappedUser;
            }
            catch (ServiceException graphException)
            {
                // TODO LOG and return null;

                return null;
            }
        }

        // TODO: Implement with Func
        public async Task<ApplicationUser> ResolveUserByUpnAsync(string upn, string currentUserId)
        {
            var graphClient = GetAuthenticatedClient(currentUserId);

            try
            {
                var user = await graphClient.Users[upn].Request().GetAsync();
                var mappedUser = UserIdentityFactory.MapApplicationUser(user);
                mappedUser.TenantId = _tenantId;
                return mappedUser;
            }
            catch (ServiceException graphException)
            {
                // TODO LOG
                // user UPN not found log
                return null;
            }
        }
    }
}
