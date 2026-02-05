using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class BillPaymentRepository : BaseRepository<BillPayment>, IBillPaymentRepository
    {
        public BillPaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BillPayment>> GetByUserIdAsync(
            Guid userId,
            int pageNumber,
            int pageSize)
        {
            return await _dbSet
                .Include(b => b.Biller)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillPayment>> GetByWalletIdAsync(Guid walletId)
        {
            return await _dbSet
                .Include(b => b.Biller)
                .Where(b => b.WalletId == walletId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}