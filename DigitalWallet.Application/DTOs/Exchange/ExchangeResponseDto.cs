namespace DigitalWallet.Application.DTOs.Exchange
{
    public class ExchangeResponseDto
    {
        public Guid ExchangeId { get; set; }
        public decimal FromAmount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public decimal ToAmount { get; set; }
        public string ToCurrency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ExchangedAt { get; set; }
    }
}