namespace DigitalWallet.Infrastructure.ExternalServices.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendOtpEmailAsync(string to, string otpCode);
    }
}