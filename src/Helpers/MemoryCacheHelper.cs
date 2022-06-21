using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace ProjectTemplate.Helpers
{
    public static class MemoryCacheHelper
    {
        private static MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public static T Get<T>(string key, Func<T> init, TimeSpan timeSpan = default)
        {
            var cache = _memoryCache.Get(key);
            if (cache == null)
            {
                cache = init();
                if (cache == default)
                {
                    return (T)cache;
                }

                if (timeSpan == default)
                {
                    _memoryCache.Set(key, cache);
                }
                else
                {
                    _memoryCache.Set(key, cache, timeSpan);
                }
                return (T)cache;
            }

            return (T)cache;
        }

        public static async Task<T> GetAsync<T>(string key, Func<Task<T>> init, TimeSpan timeSpan = default)
        {
            var cache = _memoryCache.Get(key);
            if (cache == null)
            {
                cache = await init();
                if (cache == default)
                {
                    return (T)cache;
                }

                if (timeSpan == default)
                {
                    _memoryCache.Set(key, cache);
                }
                else
                {
                    _memoryCache.Set(key, cache, timeSpan);
                }
                return (T)cache;
            }

            return (T)cache;
        }

        public static void Remove(object key) => _memoryCache.Remove(key);
    }
}
