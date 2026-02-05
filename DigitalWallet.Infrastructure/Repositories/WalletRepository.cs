using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Wallet?> GetByUserIdAndCurrencyAsync(Guid userId, string currencyCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyCode == currencyCode);
        }

        public async Task<IEnumerable<Wallet>> GetAllByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalBalanceAsync(Guid userId)
        {
            return await _dbSet
                .Where(w => w.UserId == userId)
                .SumAsync(w => w.Balance);
        }
    }
}