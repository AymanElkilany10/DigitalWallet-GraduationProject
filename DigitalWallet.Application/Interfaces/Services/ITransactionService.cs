using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Transaction;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface ITransactionService
    {
        Task<ServiceResult<TransactionDto>> GetTransactionByIdAsync(Guid transactionId);
        Task<ServiceResult<PaginatedResult<TransactionDto>>> GetWalletTransactionsAsync(
            Guid walletId, int pageNumber = 1, int pageSize = 20);
    }
}