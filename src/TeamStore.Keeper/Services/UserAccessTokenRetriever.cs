using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStore.Keeper.Interfaces;

namespace TeamStore.Keeper.Services
{
    public class UserAccessTokenRetriever : IAccessTokenRetriever
    {
        public async Task<string> GetGraphAccessTokenAsync(string userId, string _aadInstance, string _appId, string _appSecret, string _tenantId, IMemoryCache _memoryCache, string _graphResourceId)
        {
            TokenCache userTokenCache = new SessionCacheService(userId, _memoryCache).GetCacheInstance();

            // TODO: extract and DRY
            if (!_aadInstance.Last().Equals('/'))
                _aadInstance = _aadInstance + "/";

            _aadInstance = _aadInstance + _tenantId;
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
            catch (Exception ex)
            {
                // TODO: log ex
                return null;
            }
        }
    }
}
