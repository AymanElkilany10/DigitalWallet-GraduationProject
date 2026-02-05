using System.Security.Cryptography;

namespace DigitalWallet.Application.Helpers
{
    public static class OtpGenerator
    {
        /// <summary>
        /// Generates a random 6-digit OTP code
        /// </summary>
        public static string GenerateOtpCode(int length = 6)
        {
            if (length < 4 || length > 10)
                throw new ArgumentException("OTP length must be between 4 and 10", nameof(length));

            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var randomNumber = BitConverter.ToUInt32(bytes, 0);

                var maxValue = (int)Math.Pow(10, length);
                var otp = randomNumber % maxValue;

                return otp.ToString($"D{length}");
            }
        }

        /// <summary>
        /// Generates a more secure numeric OTP
        /// </summary>
        public static string GenerateSecureOtp(int length = 6)
        {
            var random = new Random();
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length) - 1;
            return random.Next(min, max).ToString();
        }

        /// <summary>
        /// Generates an alphanumeric OTP
        /// </summary>
        public static string GenerateAlphanumericOtp(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a random account number for fake bank
        /// </summary>
        public static string GenerateAccountNumber()
        {
            var random = new Random();
            return $"FBA{random.Next(10000000, 99999999)}";
        }

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        public static string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}