namespace TeamStore.Keeper.Interfaces
{
    using Microsoft.Extensions.Caching.Memory;
    using System.Threading.Tasks;

    public interface IAccessTokenRetriever
    {
        Task<string> GetGraphAccessTokenAsync(string userId, string _aadInstance, string _appId, string _appSecret, string _tenantId, IMemoryCache _memoryCache, string _graphResourceId);
    }
}
