using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class FraudLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public FraudType Type { get; set; }
        public string? Description { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}