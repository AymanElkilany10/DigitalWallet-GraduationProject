using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Auth;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ServiceResult<bool>> VerifyOtpAsync(VerifyOtpRequestDto request);
        Task<ServiceResult<bool>> SendOtpAsync(Guid userId, string otpType);
    }
}