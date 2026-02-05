using System.Text.Json;

namespace DigitalWallet.Infrastructure.Logging
{
    public interface IFileLogger
    {
        void LogInfo(string message, object? data = null);
        void LogWarning(string message, object? data = null);
        void LogError(string message, Exception? exception = null, object? data = null);
        void LogDebug(string message, object? data = null);
    }

    public class FileLogger : IFileLogger
    {
        private readonly string _logDirectory;
        private readonly string _applicationName;
        private static readonly object _lock = new object();

        public FileLogger(string logDirectory = "Logs", string applicationName = "DigitalWallet")
        {
            _logDirectory = logDirectory;
            _applicationName = applicationName;

            // Ensure log directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void LogInfo(string message, object? data = null)
        {
            WriteLog("INFO", message, data: data);
        }

        public void LogWarning(string message, object? data = null)
        {
            WriteLog("WARNING", message, data: data);
        }

        public void LogError(string message, Exception? exception = null, object? data = null)
        {
            WriteLog("ERROR", message, exception, data);
        }

        public void LogDebug(string message, object? data = null)
        {
            WriteLog("DEBUG", message, data: data);
        }

        private void WriteLog(string level, string message, Exception? exception = null, object? data = null)
        {
            try
            {
                var logEntry = new
                {
                    Timestamp = DateTime.UtcNow,
                    Level = level,
                    Application = _applicationName,
                    Message = message,
                    Exception = exception != null ? new
                    {
                        Type = exception.GetType().Name,
                        Message = exception.Message,
                        StackTrace = exception.StackTrace,
                        InnerException = exception.InnerException?.Message
                    } : null,
                    Data = data
                };

                var logText = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var fileName = $"{DateTime.UtcNow:yyyy-MM-dd}.log";
                var filePath = Path.Combine(_logDirectory, fileName);

                lock (_lock)
                {
                    File.AppendAllText(filePath, logText + Environment.NewLine + new string('-', 80) + Environment.NewLine);
                }

                // Also write to console in development
                Console.WriteLine($"[{level}] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
            }
            catch (Exception ex)
            {
                // Fallback to console if file logging fails
                Console.WriteLine($"[LOGGING ERROR] Failed to write log: {ex.Message}");
                Console.WriteLine($"[{level}] {message}");
            }
        }
    }

    /// <summary>
    /// Transaction logger for auditing financial operations
    /// </summary>
    public class TransactionLogger : FileLogger
    {
        public TransactionLogger() : base("Logs/Transactions", "TransactionLog")
        {
        }

        public void LogTransaction(Guid userId, string transactionType, decimal amount, string status, object? details = null)
        {
            var transactionData = new
            {
                UserId = userId,
                TransactionType = transactionType,
                Amount = amount,
                Status = status,
                Details = details
            };

            LogInfo($"Transaction: {transactionType} - {amount:C} - {status}", transactionData);
        }
    }

    /// <summary>
    /// Security logger for authentication and authorization events
    /// </summary>
    public class SecurityLogger : FileLogger
    {
        public SecurityLogger() : base("Logs/Security", "SecurityLog")
        {
        }

        public void LogLoginAttempt(string emailOrPhone, bool success, string? ipAddress = null)
        {
            var loginData = new
            {
                EmailOrPhone = emailOrPhone,
                Success = success,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            if (success)
                LogInfo($"Successful login: {emailOrPhone}", loginData);
            else
                LogWarning($"Failed login attempt: {emailOrPhone}", loginData);
        }

        public void LogOtpGeneration(Guid userId, string otpType)
        {
            LogInfo($"OTP generated for user {userId}, type: {otpType}");
        }

        public void LogOtpVerification(Guid userId, bool success)
        {
            if (success)
                LogInfo($"OTP verified successfully for user {userId}");
            else
                LogWarning($"OTP verification failed for user {userId}");
        }

        public void LogSuspiciousActivity(Guid userId, string activityType, string description)
        {
            var suspiciousData = new
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description
            };

            LogWarning($"Suspicious activity detected: {activityType}", suspiciousData);
        }
    }
}