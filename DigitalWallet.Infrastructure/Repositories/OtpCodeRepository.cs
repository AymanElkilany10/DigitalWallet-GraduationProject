using Microsoft.EntityFrameworkCore;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using DigitalWallet.Infrastructure.Data;

namespace DigitalWallet.Infrastructure.Repositories
{
    public class OtpCodeRepository : BaseRepository<OtpCode>, IOtpCodeRepository
    {
        public OtpCodeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<OtpCode?> GetValidOtpAsync(Guid userId, string code, OtpType type)
        {
            return await _dbSet
                .FirstOrDefaultAsync(o =>
                    o.UserId == userId &&
                    o.Code == code &&
                    o.Type == type &&
                    !o.IsUsed &&
                    o.ExpiresAt > DateTime.UtcNow);
        }

        public async Task MarkAsUsedAsync(Guid otpId)
        {
            var otp = await GetByIdAsync(otpId);
            if (otp != null)
            {
                otp.IsUsed = true;
                await UpdateAsync(otp);
            }
        }

        public async Task DeleteExpiredOtpsAsync()
        {
            var expiredOtps = await _dbSet
                .Where(o => o.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _dbSet.RemoveRange(expiredOtps);
        }
    }
}