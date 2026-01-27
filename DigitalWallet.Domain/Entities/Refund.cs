using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class Refund : BaseEntity
    {
        public Guid OriginalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public TransactionStatus Status { get; set; }

        // Navigation Property
        public Transaction OriginalTransaction { get; set; } = null!;
    }
}