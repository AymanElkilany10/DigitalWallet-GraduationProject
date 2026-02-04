using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Wallet;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<ServiceResult<WalletDto>> CreateWalletAsync(CreateWalletRequestDto request);
        Task<ServiceResult<WalletDto>> GetWalletByIdAsync(Guid walletId);
        Task<ServiceResult<IEnumerable<WalletDto>>> GetUserWalletsAsync(Guid userId);
        Task<ServiceResult<WalletBalanceDto>> GetWalletBalanceAsync(Guid walletId);
    }
}