using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IMoneyRequestRepository : IBaseRepository<MoneyRequest>
    {
        Task<IEnumerable<MoneyRequest>> GetByFromUserIdAsync(Guid userId);
        Task<IEnumerable<MoneyRequest>> GetByToUserIdAsync(Guid userId);
        Task<IEnumerable<MoneyRequest>> GetPendingRequestsAsync(Guid userId);
    }
}