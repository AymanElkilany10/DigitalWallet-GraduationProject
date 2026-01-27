using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class FakeBankTransaction : BaseEntity
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "deposit" or "withdraw"
        public TransactionStatus Status { get; set; }
        public int DelaySeconds { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}