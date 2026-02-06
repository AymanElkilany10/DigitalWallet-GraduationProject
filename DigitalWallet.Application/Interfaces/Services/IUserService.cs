using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.DTOs.Auth;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserManagementDto>> GetUserByIdAsync(Guid userId);
        Task<ServiceResult<UserManagementDto>> GetUserByEmailAsync(string email);
    }
}