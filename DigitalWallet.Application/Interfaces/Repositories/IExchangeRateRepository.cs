using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IExchangeRateRepository : IBaseRepository<ExchangeRate>
    {
        Task<ExchangeRate?> GetRateAsync(string fromCurrency, string toCurrency);
        Task<List<ExchangeRate>> GetAllActiveRatesAsync();
        Task<bool> UpdateRateAsync(string fromCurrency, string toCurrency, decimal rate);
    }

    public interface ICurrencyExchangeRepository : IBaseRepository<CurrencyExchange>
    {
        Task<List<CurrencyExchange>> GetUserExchangesAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
        Task<CurrencyExchange?> GetExchangeByIdAsync(Guid exchangeId);
    }
}