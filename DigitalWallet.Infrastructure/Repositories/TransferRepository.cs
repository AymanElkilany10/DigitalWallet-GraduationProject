using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class TransferRepository : BaseRepository<Transfer>, ITransferRepository
    {
        public TransferRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transfer>> GetBySenderWalletIdAsync(
            Guid walletId,
            int pageNumber,
            int pageSize)
        {
            return await _dbSet
                .Where(t => t.SenderWalletId == walletId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transfer>> GetByReceiverWalletIdAsync(
            Guid walletId,
            int pageNumber,
            int pageSize)
        {
            return await _dbSet
                .Where(t => t.ReceiverWalletId == walletId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}