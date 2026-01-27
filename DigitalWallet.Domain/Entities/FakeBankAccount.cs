using DigitalWallet.Domain.Common;

namespace DigitalWallet.Domain.Entities
{
    public class FakeBankAccount : BaseEntity
    {
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}