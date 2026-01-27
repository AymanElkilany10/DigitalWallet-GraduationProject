using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class AdminUser : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public AdminRole Role { get; set; }

        // Navigation Property
        public ICollection<AdminActionsLog> Actions { get; set; } = new List<AdminActionsLog>();
    }
}