using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class BillerRepository : BaseRepository<Biller>, IBillerRepository
    {
        public BillerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Biller>> GetActiveBillersAsync()
        {
            return await _dbSet
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Biller>> GetByCategoryAsync(BillCategory category)
        {
            return await _dbSet
                .Where(b => b.Category == category && b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }
    }
}