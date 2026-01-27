using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class BillPayment : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public Guid BillerId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string? ReceiptPath { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public Wallet Wallet { get; set; } = null!;
        public Biller Biller { get; set; } = null!;
    }
}