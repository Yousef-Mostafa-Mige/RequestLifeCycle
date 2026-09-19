namespace services.CashingServices
{
    public interface ICaching
    {
        Task<T> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> fetchFunction);
        Task RemoveAsync(string cacheKey);
    }
}