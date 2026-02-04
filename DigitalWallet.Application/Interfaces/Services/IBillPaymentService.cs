using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.BillPayment;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IBillPaymentService
    {
        Task<ServiceResult<IEnumerable<BillerDto>>> GetAllBillersAsync();
        Task<ServiceResult<BillPaymentDto>> PayBillAsync(Guid userId, PayBillRequestDto request);
        Task<ServiceResult<PaginatedResult<BillPaymentDto>>> GetPaymentHistoryAsync(
            Guid userId, int pageNumber = 1, int pageSize = 20);
    }
}