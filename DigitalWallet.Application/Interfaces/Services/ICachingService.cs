namespace DigitalWallet.Application.Interfaces.Services
{
    public interface ICachingService
    {
        /// <summary>
        /// Gets value from cache
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Sets value in cache with expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Removes value from cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes all keys matching pattern
        /// </summary>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Checks if key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Gets or sets value using factory method
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    }
}