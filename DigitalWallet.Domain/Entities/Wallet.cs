using System.Transactions;
using DigitalWallet.Domain.Common;

namespace DigitalWallet.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }
        public string CurrencyCode { get; set; } = "EGP";
        public decimal Balance { get; set; }
        public decimal DailyLimit { get; set; } = 5000;
        public decimal MonthlyLimit { get; set; } = 20000;

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Transfer> SentTransfers { get; set; } = new List<Transfer>();
        public ICollection<Transfer> ReceivedTransfers { get; set; } = new List<Transfer>();
        public ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();
    }
}