using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Transfer;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface ITransferService
    {
        Task<ServiceResult<TransferResponseDto>> SendMoneyAsync(SendMoneyRequestDto request);
        Task<ServiceResult<PaginatedResult<TransferDto>>> GetTransferHistoryAsync(
            Guid walletId, int pageNumber = 1, int pageSize = 20);
    }
}