using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Admin;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IAdminService
    {
        Task<ServiceResult<IEnumerable<UserManagementDto>>> GetAllUsersAsync();
        Task<ServiceResult<IEnumerable<FraudLogDto>>> GetFraudLogsAsync();
    }
}