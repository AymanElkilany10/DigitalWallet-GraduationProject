using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.Transaction
{
    public class TransactionFilterDto
    {
        public Guid? WalletId { get; set; }
        public TransactionType? Type { get; set; }
        public TransactionStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}