using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IWalletRepository : IBaseRepository<Wallet>
    {
        Task<Wallet?> GetByUserIdAndCurrencyAsync(Guid userId, string currencyCode);
        Task<IEnumerable<Wallet>> GetAllByUserIdAsync(Guid userId);
        Task<decimal> GetTotalBalanceAsync(Guid userId);
    }
}