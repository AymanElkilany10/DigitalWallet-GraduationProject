using DigitalWallet.Domain.Common;

namespace DigitalWallet.Domain.Entities
{
    public class RewardWallet : BaseEntity
    {
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; } = "EGP";

        // Navigation Property
        public User User { get; set; } = null!;
    }
}