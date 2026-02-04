using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IOtpCodeRepository : IBaseRepository<OtpCode>
    {
        Task<OtpCode?> GetValidOtpAsync(Guid userId, string code, OtpType type);
        Task MarkAsUsedAsync(Guid otpId);
        Task DeleteExpiredOtpsAsync();
    }
}