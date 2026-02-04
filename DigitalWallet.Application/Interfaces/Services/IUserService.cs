using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Auth;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid userId);
        Task<ServiceResult<UserDto>> GetUserByEmailAsync(string email);
        Task<ServiceResult<UserDto>> GetUserByPhoneAsync(string phone);
    }
}