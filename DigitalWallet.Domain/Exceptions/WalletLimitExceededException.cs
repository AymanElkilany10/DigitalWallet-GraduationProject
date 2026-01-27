namespace DigitalWallet.Domain.Exceptions
{
    public class WalletLimitExceededException : DomainException
    {
        public WalletLimitExceededException(string limitType)
            : base($"{limitType} limit exceeded")
        {
        }

        public WalletLimitExceededException(decimal limit, decimal attempted)
            : base($"Limit exceeded. Limit: {limit}, Attempted: {attempted}")
        {
        }
    }
}