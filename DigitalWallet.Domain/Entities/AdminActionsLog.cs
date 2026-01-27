using DigitalWallet.Domain.Common;

namespace DigitalWallet.Domain.Entities
{
    public class AdminActionsLog : BaseEntity
    {
        public Guid AdminId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid? TargetUserId { get; set; }
        public string? Data { get; set; } // JSON

        // Navigation Properties
        public AdminUser Admin { get; set; } = null!;
        public User? TargetUser { get; set; }
    }
}