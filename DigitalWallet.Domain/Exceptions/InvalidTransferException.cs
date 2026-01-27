namespace DigitalWallet.Domain.Exceptions
{
    public class InvalidTransferException : DomainException
    {
        public InvalidTransferException(string message) : base(message)
        {
        }
    }
}