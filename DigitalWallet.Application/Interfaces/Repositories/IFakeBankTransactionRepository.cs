using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IFakeBankTransactionRepository : IBaseRepository<FakeBankTransaction>
    {
        Task<IEnumerable<FakeBankTransaction>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<FakeBankTransaction>> GetPendingTransactionsAsync();
    }
}