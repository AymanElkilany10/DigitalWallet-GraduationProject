using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IFakeBankAccountRepository : IBaseRepository<FakeBankAccount>
    {
        Task<FakeBankAccount?> GetByUserIdAsync(Guid userId);
        Task<FakeBankAccount?> GetByAccountNumberAsync(string accountNumber);
    }
}