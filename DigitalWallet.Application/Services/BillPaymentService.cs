using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.BillPayment;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Services
{
    public class BillPaymentService : IBillPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BillPaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<BillerDto>>> GetAllBillersAsync()
        {
            try
            {
                var billers = await _unitOfWork.Billers.GetActiveBillersAsync();
                var billerDtos = _mapper.Map<IEnumerable<BillerDto>>(billers);
                return ServiceResult<IEnumerable<BillerDto>>.Success(billerDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<BillerDto>>.Failure(
                    $"Error retrieving billers: {ex.Message}");
            }
        }

        public async Task<ServiceResult<BillPaymentDto>> PayBillAsync(
            Guid userId,
            PayBillRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Verify OTP
                var otp = await _unitOfWork.OtpCodes.GetValidOtpAsync(
                    userId,
                    request.OtpCode,
                    OtpType.Transfer);

                if (otp == null)
                    return ServiceResult<BillPaymentDto>.Failure("Invalid or expired OTP");

                // Verify wallet
                var wallet = await _unitOfWork.Wallets.GetByIdAsync(request.WalletId);
                if (wallet == null || wallet.UserId != userId)
                    return ServiceResult<BillPaymentDto>.Failure("Wallet not found");

                if (wallet.Balance < request.Amount)
                    return ServiceResult<BillPaymentDto>.Failure("Insufficient balance");

                // Verify biller
                var biller = await _unitOfWork.Billers.GetByIdAsync(request.BillerId);
                if (biller == null || !biller.IsActive)
                    return ServiceResult<BillPaymentDto>.Failure("Biller not available");

                // Create bill payment
                var billPayment = new BillPayment
                {
                    UserId = userId,
                    WalletId = wallet.Id,
                    BillerId = biller.Id,
                    Amount = request.Amount,
                    CurrencyCode = wallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    ReceiptPath = $"/receipts/bill_{Guid.NewGuid()}.pdf"
                };

                await _unitOfWork.BillPayments.AddAsync(billPayment);

                // Update wallet balance
                wallet.Balance -= request.Amount;
                await _unitOfWork.Wallets.UpdateAsync(wallet);

                // Create transaction record
                var transaction = new Domain.Entities.Transaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Bill,
                    Amount = -request.Amount,
                    CurrencyCode = wallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = $"Bill payment - {biller.Name}",
                    Reference = billPayment.Id.ToString()
                };

                await _unitOfWork.Transactions.AddAsync(transaction);

                // Mark OTP as used
                await _unitOfWork.OtpCodes.MarkAsUsedAsync(otp.Id);

                // Create notification
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Bill Payment Successful",
                    Body = $"Paid {request.Amount} {wallet.CurrencyCode} to {biller.Name}",
                    Type = NotificationType.Transaction,
                    IsRead = false
                };

                await _unitOfWork.Notifications.AddAsync(notification);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var paymentDto = _mapper.Map<BillPaymentDto>(billPayment);
                return ServiceResult<BillPaymentDto>.Success(
                    paymentDto,
                    "Bill payment successful");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult<BillPaymentDto>.Failure(
                    $"Bill payment failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResult<BillPaymentDto>>> GetPaymentHistoryAsync(
            Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var payments = await _unitOfWork.BillPayments.GetByUserIdAsync(
                    userId, pageNumber, pageSize);
                var totalCount = payments.Count();

                var paymentDtos = _mapper.Map<List<BillPaymentDto>>(payments);
                var paginatedResult = PaginatedResult<BillPaymentDto>.Create(
                    paymentDtos, totalCount, pageNumber, pageSize);

                return ServiceResult<PaginatedResult<BillPaymentDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResult<BillPaymentDto>>.Failure(
                    $"Error retrieving payment history: {ex.Message}");
            }
        }
    }
}