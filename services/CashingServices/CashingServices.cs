using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using RequestLifeCycle.Middleware;

namespace services.CashingServices
{
    public class AppCashing(IDistributedCache cache) : ICaching
    {
        public async Task<T> GetOrCreateAsync<T>(
            string cacheKey,
            Func<Task<T>> fetchFunction)
        {
            var cachedValue = await cache.GetStringAsync(cacheKey);

            if (cachedValue is not null)
            {
                var value = JsonSerializer.Deserialize<T>(cachedValue);

                if (value is not null)
                {
                    return value;
                }
            }

            T valueFromDatabase = await fetchFunction();

            if (valueFromDatabase is null)
            {
                throw new BadRequestException(
                    "The fetch function returned null.");
            }

            var jsonValue = JsonSerializer.Serialize(valueFromDatabase);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(5),

                SlidingExpiration =
                    TimeSpan.FromMinutes(2)
            };

            await cache.SetStringAsync(
                cacheKey,
                jsonValue,
                options);

            return valueFromDatabase;
        }

        public async Task RemoveAsync(string cacheKey)
        {
            await cache.RemoveAsync(cacheKey);
        }
    }
}