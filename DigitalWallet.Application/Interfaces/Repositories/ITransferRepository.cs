using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface ITransferRepository : IBaseRepository<Transfer>
    {
        Task<IEnumerable<Transfer>> GetBySenderWalletIdAsync(Guid walletId, int pageNumber, int pageSize);
        Task<IEnumerable<Transfer>> GetByReceiverWalletIdAsync(Guid walletId, int pageNumber, int pageSize);
    }
}