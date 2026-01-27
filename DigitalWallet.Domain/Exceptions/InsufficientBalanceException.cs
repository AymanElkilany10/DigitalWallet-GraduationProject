namespace DigitalWallet.Domain.Exceptions
{
    public class InsufficientBalanceException : DomainException
    {
        public InsufficientBalanceException()
            : base("Insufficient balance in wallet")
        {
        }

        public InsufficientBalanceException(decimal balance, decimal required)
            : base($"Insufficient balance. Available: {balance}, Required: {required}")
        {
        }
    }
}