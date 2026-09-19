using Microsoft.Extensions.Caching.Memory;
using RequestLifeCycle.Middleware;

namespace services.CashingServices
{
    public class AppCashing(IMemoryCache cache) : ICaching
    {
        public async Task<T> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> fetchFunction)
        {
            if(cache.TryGetValue(cacheKey, out T? cachedValue))
            {
                return cachedValue!;
            }
            T value = await fetchFunction();
            if(value == null)
            {
                throw new BadRequestException("The fetch function returned null.");
            }
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            cache.Set(cacheKey, value, options);
            return value;
        }

        public async Task RemoveAsync(string cacheKey)
        {
            cache.Remove(cacheKey);
        }
    }
}