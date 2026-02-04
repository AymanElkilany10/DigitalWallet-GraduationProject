using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId, int pageNumber, int pageSize);
        Task<int> GetCountByWalletIdAsync(Guid walletId);
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(Guid walletId, int count);
    }
}