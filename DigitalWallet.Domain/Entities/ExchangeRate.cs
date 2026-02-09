using DigitalWallet.Domain.Common;

namespace DigitalWallet.Domain.Entities
{
    public class ExchangeRate : BaseEntity
    {
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Source { get; set; } = "ExchangeRateAPI"; // API source
        public bool IsActive { get; set; } = true;
    }
}