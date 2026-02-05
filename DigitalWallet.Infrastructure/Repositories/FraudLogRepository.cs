using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class FraudLogRepository : BaseRepository<FraudLog>, IFraudLogRepository
    {
        public FraudLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FraudLog>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FraudLog>> GetRecentLogsAsync(int hours)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);
            return await _dbSet
                .Include(f => f.User)
                .Where(f => f.CreatedAt >= cutoffTime)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}