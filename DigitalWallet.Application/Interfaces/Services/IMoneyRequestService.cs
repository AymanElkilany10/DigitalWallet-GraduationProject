using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.MoneyRequest;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IMoneyRequestService
    {
        Task<ServiceResult<MoneyRequestDto>> CreateRequestAsync(Guid fromUserId, CreateMoneyRequestDto request);
        Task<ServiceResult<IEnumerable<MoneyRequestDto>>> GetSentRequestsAsync(Guid userId);
        Task<ServiceResult<IEnumerable<MoneyRequestDto>>> GetReceivedRequestsAsync(Guid userId);
        Task<ServiceResult<bool>> AcceptOrRejectRequestAsync(Guid userId, AcceptRejectRequestDto request);
    }
}