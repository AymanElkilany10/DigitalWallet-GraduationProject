namespace DigitalWallet.Infrastructure.ExternalServices.Email
{
    public class EmailService : IEmailService
    {
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            // Mock implementation
            // In production, use SMTP or email service provider like SendGrid, AWS SES, etc.
            await Task.Delay(100); // Simulate email sending
            Console.WriteLine($"[EMAIL SENT] To: {to}, Subject: {subject}");
            return true;
        }

        public async Task<bool> SendOtpEmailAsync(string to, string otpCode)
        {
            var subject = "Your OTP Code - Digital Wallet";
            var body = $"Your OTP code is: {otpCode}. It will expire in 5 minutes.";
            return await SendEmailAsync(to, subject, body);
        }
    }
}