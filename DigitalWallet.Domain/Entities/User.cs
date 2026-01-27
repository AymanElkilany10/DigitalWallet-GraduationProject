using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public KycLevel KycLevel { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation Properties
        public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
        public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
        public RewardWallet? RewardWallet { get; set; }
        public FakeBankAccount? FakeBankAccount { get; set; }
        public ICollection<FakeBankTransaction> FakeBankTransactions { get; set; } = new List<FakeBankTransaction>();
        public ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<FraudLog> FraudLogs { get; set; } = new List<FraudLog>();
        public ICollection<MoneyRequest> SentRequests { get; set; } = new List<MoneyRequest>();
        public ICollection<MoneyRequest> ReceivedRequests { get; set; } = new List<MoneyRequest>();
    }
}