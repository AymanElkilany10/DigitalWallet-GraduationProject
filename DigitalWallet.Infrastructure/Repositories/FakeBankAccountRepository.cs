using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class FakeBankAccountRepository : BaseRepository<FakeBankAccount>, IFakeBankAccountRepository
    {
        public FakeBankAccountRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<FakeBankAccount?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(f => f.UserId == userId);
        }

        public async Task<FakeBankAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(f => f.AccountNumber == accountNumber);
        }
    }
}