using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string? Description { get; set; }
        public string? Reference { get; set; }

        // Navigation Properties
        public Wallet Wallet { get; set; } = null!;
        public Refund? Refund { get; set; }
    }
}