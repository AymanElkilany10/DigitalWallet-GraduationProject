using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class AdminRepository : BaseRepository<AdminUser>, IAdminRepository
    {
        public AdminRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<AdminUser?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(a => a.Email == email);
        }

        public async Task<IEnumerable<AdminUser>> GetByRoleAsync(string role)
        {
            return await _dbSet
                .Where(a => a.Role.ToString() == role)
                .ToListAsync();
        }
    }
}