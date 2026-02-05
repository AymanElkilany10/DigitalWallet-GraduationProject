namespace DigitalWallet.Infrastructure.ExternalServices.SMS
{
    public class SmsService : ISmsService
    {
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            // Mock implementation
            // In production, use SMS service provider like Twilio, AWS SNS, etc.
            await Task.Delay(100); // Simulate SMS sending
            Console.WriteLine($"[SMS SENT] To: {phoneNumber}, Message: {message}");
            return true;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
        {
            var message = $"Your Digital Wallet OTP code is: {otpCode}. Valid for 5 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }
    }
}