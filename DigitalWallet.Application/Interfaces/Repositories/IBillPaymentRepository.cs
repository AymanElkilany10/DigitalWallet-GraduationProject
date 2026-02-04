using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IBillPaymentRepository : IBaseRepository<BillPayment>
    {
        Task<IEnumerable<BillPayment>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<IEnumerable<BillPayment>> GetByWalletIdAsync(Guid walletId);
    }
}