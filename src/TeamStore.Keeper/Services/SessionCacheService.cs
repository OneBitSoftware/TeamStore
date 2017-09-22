﻿namespace TeamStore.Keeper.Services
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Text;

    /// <summary>
    /// Responsible for caching tokens in memory
    /// </summary>
    public class SessionCacheService
    {
        private static readonly object _fileLock = new object();
        string _userId = string.Empty;
        string _cacheId = string.Empty;
        IMemoryCache _memoryCache;
        TokenCache _cache = new TokenCache();

        public SessionCacheService(string userId, IMemoryCache memoryCache)
        {
            // not object, we want the SUB
            _userId = userId;
            _cacheId = userId + "_TokenCache";
            _memoryCache = memoryCache;

            Load();
        }

        public TokenCache GetCacheInstance()
        {
            _cache.BeforeAccess = BeforeAccessNotification;
            _cache.AfterAccess = AfterAccessNotification;
            Load();

            return _cache;
        }

        public void SaveUserStateValue(string state)
        {
            lock (_fileLock)
            {
                _memoryCache.Set(_cacheId + "_state", Encoding.ASCII.GetBytes(state));
            }
        }

        public string ReadUserStateValue()
        {
            string state = string.Empty;
            lock (_fileLock)
            {
                state = Encoding.ASCII.GetString(_memoryCache.Get(_cacheId + "_state") as byte[]);
            }

            return state;
        }

        public void Load()
        {
            lock (_fileLock)
            {
                _cache.Deserialize(_memoryCache.Get(_cacheId) as byte[]);
            }
        }

        public void Persist()
        {
            lock (_fileLock)
            {
                // reflect changes in the persistent store
                _memoryCache.Set(_cacheId, _cache.Serialize());
                // once the write operation took place, restore the HasStateChanged bit to false
                _cache.HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public void Clear()
        {
            _cache = null;
            _memoryCache.Remove(_cacheId);
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (_cache.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
