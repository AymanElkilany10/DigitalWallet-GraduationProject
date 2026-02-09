namespace DigitalWallet.Application.DTOs.Exchange
{
    public class ExchangeRequestDto
    {
        public Guid FromWalletId { get; set; }
        public Guid ToWalletId { get; set; }
        public decimal Amount { get; set; }
        public string? OtpCode { get; set; } // Optional OTP for large amounts
    }
}