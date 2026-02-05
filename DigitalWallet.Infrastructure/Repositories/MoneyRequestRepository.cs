using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class MoneyRequestRepository : BaseRepository<MoneyRequest>, IMoneyRequestRepository
    {
        public MoneyRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MoneyRequest>> GetByFromUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .Where(m => m.FromUserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<MoneyRequest>> GetByToUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .Where(m => m.ToUserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<MoneyRequest>> GetPendingRequestsAsync(Guid userId)
        {
            return await _dbSet
                .Include(m => m.FromUser)
                .Include(m => m.ToUser)
                .Where(m => m.ToUserId == userId && m.Status == MoneyRequestStatus.Pending)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}