namespace DigitalWallet.Application.DTOs.Admin
{
    public class WalletManagementDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal MonthlyLimit { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}