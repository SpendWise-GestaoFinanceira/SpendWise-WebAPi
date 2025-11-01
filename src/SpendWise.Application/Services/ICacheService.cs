namespace SpendWise.Application.Services;

public interface ICacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T?> GetAsync<T>(string key) where T : class;
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}
