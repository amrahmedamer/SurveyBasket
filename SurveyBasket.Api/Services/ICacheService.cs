namespace SurveyBasket.Api.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken) where T : class;
    Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken) where T : class;
    Task RemoveAsync(string cacheKey, CancellationToken cancellationToken);
}
