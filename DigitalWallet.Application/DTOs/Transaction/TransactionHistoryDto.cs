namespace DigitalWallet.Application.DTOs.Transaction
{
    public class TransactionHistoryDto
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional properties for history display
        public string DisplayAmount => Amount >= 0 ? $"+{Amount:N2}" : $"{Amount:N2}";
        public string TransactionTypeDisplay => Type switch
        {
            "Transfer" => "Money Transfer",
            "Bill" => "Bill Payment",
            "Deposit" => "Deposit from Bank",
            "Withdraw" => "Withdrawal to Bank",
            "Refund" => "Refund",
            _ => Type
        };
    }
}