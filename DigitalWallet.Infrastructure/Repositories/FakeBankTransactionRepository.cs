using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class FakeBankTransactionRepository : BaseRepository<FakeBankTransaction>, IFakeBankTransactionRepository
    {
        public FakeBankTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FakeBankTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FakeBankTransaction>> GetPendingTransactionsAsync()
        {
            return await _dbSet
                .Where(f => f.Status == TransactionStatus.Pending)
                .ToListAsync();
        }
    }
}