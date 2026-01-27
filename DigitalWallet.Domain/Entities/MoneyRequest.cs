using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class MoneyRequest : BaseEntity
    {
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public MoneyRequestStatus Status { get; set; }

        // Navigation Properties
        public User FromUser { get; set; } = null!;
        public User ToUser { get; set; } = null!;
    }
}