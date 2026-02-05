using Microsoft.Extensions.DependencyInjection;

namespace DigitalWallet.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Adds file logging services to dependency injection
        /// </summary>
        public static IServiceCollection AddFileLogging(this IServiceCollection services)
        {
            services.AddSingleton<IFileLogger, FileLogger>();
            services.AddSingleton<TransactionLogger>();
            services.AddSingleton<SecurityLogger>();

            return services;
        }
    }
}