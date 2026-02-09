using DigitalWallet.Domain.Entities;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class ExchangeRateRepository : BaseRepository<ExchangeRate>, IExchangeRateRepository
    {
        public ExchangeRateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ExchangeRate?> GetRateAsync(string fromCurrency, string toCurrency)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r =>
                    r.FromCurrency == fromCurrency &&
                    r.ToCurrency == toCurrency &&
                    r.IsActive);
        }

        public async Task<List<ExchangeRate>> GetAllActiveRatesAsync()
        {
            return await _dbSet
                .Where(r => r.IsActive)
                .OrderBy(r => r.FromCurrency)
                .ThenBy(r => r.ToCurrency)
                .ToListAsync();
        }

        public async Task<bool> UpdateRateAsync(string fromCurrency, string toCurrency, decimal rate)
        {
            var existingRate = await GetRateAsync(fromCurrency, toCurrency);

            if (existingRate != null)
            {
                existingRate.Rate = rate;
                existingRate.LastUpdated = DateTime.UtcNow;
                await UpdateAsync(existingRate);
                return true;
            }

            return false;
        }
    }

    public class CurrencyExchangeRepository : BaseRepository<CurrencyExchange>, ICurrencyExchangeRepository
    {
        public CurrencyExchangeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CurrencyExchange>> GetUserExchangesAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<CurrencyExchange?> GetExchangeByIdAsync(Guid exchangeId)
        {
            return await _dbSet
                .Include(e => e.FromWallet)
                .Include(e => e.ToWallet)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == exchangeId);
        }
    }
}