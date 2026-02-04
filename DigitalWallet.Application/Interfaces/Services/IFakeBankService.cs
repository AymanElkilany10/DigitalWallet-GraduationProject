using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.FakeBank;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IFakeBankService
    {
        Task<ServiceResult<FakeBankTransactionDto>> DepositAsync(DepositRequestDto request);
        Task<ServiceResult<FakeBankTransactionDto>> WithdrawAsync(WithdrawRequestDto request);
        Task<ServiceResult<decimal>> GetBankBalanceAsync(Guid userId);
    }
}