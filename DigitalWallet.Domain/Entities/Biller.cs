using DigitalWallet.Domain.Common;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Domain.Entities
{
    public class Biller : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public BillCategory Category { get; set; }
        public bool IsActive { get; set; }

        // Navigation Property
        public ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();
    }
}