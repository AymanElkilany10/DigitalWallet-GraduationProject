using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IAdminRepository : IBaseRepository<AdminUser>
    {
        Task<AdminUser?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<AdminUser>> GetByRoleAsync(string role);
    }
}