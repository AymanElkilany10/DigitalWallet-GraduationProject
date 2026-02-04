using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IFraudLogRepository : IBaseRepository<FraudLog>
    {
        Task<IEnumerable<FraudLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<FraudLog>> GetRecentLogsAsync(int hours);
    }
}