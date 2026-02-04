using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Transfer;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransferService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResult<TransferResponseDto>> SendMoneyAsync(SendMoneyRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Verify OTP
                var senderWallet = await _unitOfWork.Wallets.GetByIdAsync(request.SenderWalletId);
                if (senderWallet == null)
                    return ServiceResult<TransferResponseDto>.Failure("Sender wallet not found");

                var otp = await _unitOfWork.OtpCodes.GetValidOtpAsync(
                    senderWallet.UserId,
                    request.OtpCode,
                    OtpType.Transfer);

                if (otp == null)
                    return ServiceResult<TransferResponseDto>.Failure("Invalid or expired OTP");

                // Find receiver
                User? receiver = null;
                if (request.ReceiverPhoneOrEmail.Contains("@"))
                    receiver = await _unitOfWork.Users.GetByEmailAsync(request.ReceiverPhoneOrEmail);
                else
                    receiver = await _unitOfWork.Users.GetByPhoneNumberAsync(request.ReceiverPhoneOrEmail);

                if (receiver == null)
                    return ServiceResult<TransferResponseDto>.Failure("Receiver not found");

                // Get receiver wallet
                var receiverWallet = await _unitOfWork.Wallets.GetByUserIdAndCurrencyAsync(
                    receiver.Id,
                    senderWallet.CurrencyCode);

                if (receiverWallet == null)
                    return ServiceResult<TransferResponseDto>.Failure(
                        $"Receiver doesn't have a {senderWallet.CurrencyCode} wallet");

                // Validate balance
                if (senderWallet.Balance < request.Amount)
                    return ServiceResult<TransferResponseDto>.Failure("Insufficient balance");

                // Check daily limit
                // TODO: Implement daily limit check

                // Create transfer
                var transfer = new Transfer
                {
                    SenderWalletId = senderWallet.Id,
                    ReceiverWalletId = receiverWallet.Id,
                    Amount = request.Amount,
                    CurrencyCode = senderWallet.CurrencyCode,
                    Status = TransactionStatus.Success
                };

                await _unitOfWork.Transfers.AddAsync(transfer);

                // Update balances
                senderWallet.Balance -= request.Amount;
                receiverWallet.Balance += request.Amount;

                await _unitOfWork.Wallets.UpdateAsync(senderWallet);
                await _unitOfWork.Wallets.UpdateAsync(receiverWallet);

                // Create transactions
                var senderTransaction = new Domain.Entities.Transaction
                {
                    WalletId = senderWallet.Id,
                    Type = TransactionType.Transfer,
                    Amount = -request.Amount,
                    CurrencyCode = senderWallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = request.Description ?? $"Transfer to {receiver.FullName}",
                    Reference = transfer.Id.ToString()
                };

                var receiverTransaction = new Domain.Entities.Transaction
                {
                    WalletId = receiverWallet.Id,
                    Type = TransactionType.Transfer,
                    Amount = request.Amount,
                    CurrencyCode = receiverWallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = $"Transfer from {senderWallet.User?.FullName ?? "User"}",
                    Reference = transfer.Id.ToString()
                };

                await _unitOfWork.Transactions.AddAsync(senderTransaction);
                await _unitOfWork.Transactions.AddAsync(receiverTransaction);

                // Mark OTP as used
                await _unitOfWork.OtpCodes.MarkAsUsedAsync(otp.Id);

                // Create notifications
                var senderNotification = new Notification
                {
                    UserId = senderWallet.UserId,
                    Title = "Transfer Sent",
                    Body = $"You sent {request.Amount} {senderWallet.CurrencyCode} to {receiver.FullName}",
                    Type = NotificationType.Transaction,
                    IsRead = false
                };

                var receiverNotification = new Notification
                {
                    UserId = receiver.Id,
                    Title = "Money Received",
                    Body = $"You received {request.Amount} {receiverWallet.CurrencyCode}",
                    Type = NotificationType.Transaction,
                    IsRead = false
                };

                await _unitOfWork.Notifications.AddAsync(senderNotification);
                await _unitOfWork.Notifications.AddAsync(receiverNotification);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var response = new TransferResponseDto
                {
                    TransferId = transfer.Id,
                    ReceiverName = receiver.FullName,
                    Amount = request.Amount,
                    CurrencyCode = senderWallet.CurrencyCode,
                    Status = "Success",
                    TransferredAt = DateTime.UtcNow
                };

                return ServiceResult<TransferResponseDto>.Success(
                    response,
                    "Transfer completed successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult<TransferResponseDto>.Failure($"Transfer failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResult<TransferDto>>> GetTransferHistoryAsync(
            Guid walletId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var sentTransfers = await _unitOfWork.Transfers.GetBySenderWalletIdAsync(
                    walletId, pageNumber, pageSize);
                var receivedTransfers = await _unitOfWork.Transfers.GetByReceiverWalletIdAsync(
                    walletId, pageNumber, pageSize);

                var allTransfers = sentTransfers.Concat(receivedTransfers)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(pageSize)
                    .ToList();

                var transferDtos = _mapper.Map<List<TransferDto>>(allTransfers);
                var totalCount = allTransfers.Count;

                var paginatedResult = PaginatedResult<TransferDto>.Create(
                    transferDtos, totalCount, pageNumber, pageSize);

                return ServiceResult<PaginatedResult<TransferDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResult<TransferDto>>.Failure(
                    $"Error retrieving transfer history: {ex.Message}");
            }
        }
    }
}