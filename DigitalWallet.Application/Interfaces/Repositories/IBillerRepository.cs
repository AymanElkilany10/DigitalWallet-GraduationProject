using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IBillerRepository : IBaseRepository<Biller>
    {
        Task<IEnumerable<Biller>> GetActiveBillersAsync();
        Task<IEnumerable<Biller>> GetByCategoryAsync(BillCategory category);
    }
}