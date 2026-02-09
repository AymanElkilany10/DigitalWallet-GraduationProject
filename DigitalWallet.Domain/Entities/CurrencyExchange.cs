using DigitalWallet.Domain.Common;

namespace DigitalWallet.Domain.Entities
{
    public class CurrencyExchange : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid FromWalletId { get; set; }
        public Wallet FromWallet { get; set; } = null!;

        public Guid ToWalletId { get; set; }
        public Wallet ToWallet { get; set; } = null!;

        public decimal FromAmount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;

        public decimal ToAmount { get; set; }
        public string ToCurrency { get; set; } = string.Empty;

        public decimal ExchangeRate { get; set; }
        public decimal Fee { get; set; } = 0; // Optional exchange fee

        public string Status { get; set; } = "Success"; // Success, Failed
        public string? Notes { get; set; }
    }
}