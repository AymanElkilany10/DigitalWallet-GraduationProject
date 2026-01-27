namespace DigitalWallet.Domain.Exceptions
{
    public class InvalidOtpException : DomainException
    {
        public InvalidOtpException()
            : base("Invalid or expired OTP code")
        {
        }

        public InvalidOtpException(string message) : base(message)
        {
        }
    }
}