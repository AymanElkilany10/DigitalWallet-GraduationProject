using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class OtpCode : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Code { get; set; } = string.Empty;
        public OtpType Type { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}