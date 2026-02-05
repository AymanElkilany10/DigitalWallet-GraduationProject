using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Transaction;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;

namespace DigitalWallet.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResult<TransactionDto>> GetTransactionByIdAsync(Guid transactionId)
        {
            try
            {
                var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
                if (transaction == null)
                    return ServiceResult<TransactionDto>.Failure("Transaction not found");

                var transactionDto = _mapper.Map<TransactionDto>(transaction);
                return ServiceResult<TransactionDto>.Success(transactionDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<TransactionDto>.Failure(
                    $"Error retrieving transaction: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResult<TransactionDto>>> GetWalletTransactionsAsync(
            Guid walletId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetByWalletIdAsync(
                    walletId, pageNumber, pageSize);
                var totalCount = await _unitOfWork.Transactions.GetCountByWalletIdAsync(walletId);

                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);
                var paginatedResult = PaginatedResult<TransactionDto>.Create(
                    transactionDtos, totalCount, pageNumber, pageSize);

                return ServiceResult<PaginatedResult<TransactionDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResult<TransactionDto>>.Failure(
                    $"Error retrieving transactions: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResult<TransactionHistoryDto>>> GetTransactionHistoryAsync(
            Guid walletId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetByWalletIdAsync(
                    walletId, pageNumber, pageSize);
                var totalCount = await _unitOfWork.Transactions.GetCountByWalletIdAsync(walletId);

                var historyDtos = _mapper.Map<List<TransactionHistoryDto>>(transactions);
                var paginatedResult = PaginatedResult<TransactionHistoryDto>.Create(
                    historyDtos, totalCount, pageNumber, pageSize);

                return ServiceResult<PaginatedResult<TransactionHistoryDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResult<TransactionHistoryDto>>.Failure(
                    $"Error retrieving transaction history: {ex.Message}");
            }
        }
    }
}