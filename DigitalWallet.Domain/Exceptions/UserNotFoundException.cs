namespace DigitalWallet.Domain.Exceptions
{
    public class UserNotFoundException : DomainException
    {
        public UserNotFoundException()
            : base("User not found")
        {
        }

        public UserNotFoundException(Guid userId)
            : base($"User with ID {userId} not found")
        {
        }
    }
}