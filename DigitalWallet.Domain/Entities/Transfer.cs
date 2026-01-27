using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class Transfer : BaseEntity
    {
        public Guid SenderWalletId { get; set; }
        public Guid ReceiverWalletId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }

        // Navigation Properties
        public Wallet SenderWallet { get; set; } = null!;
        public Wallet ReceiverWallet { get; set; } = null!;
    }
}