using Microsoft.EntityFrameworkCore;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Infrastructure.Data.Configurations;
using System.Reflection;

namespace DigitalWallet.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<RewardWallet> RewardWallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<MoneyRequest> MoneyRequests { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<FakeBankAccount> FakeBankAccounts { get; set; }
        public DbSet<FakeBankTransaction> FakeBankTransactions { get; set; }
        public DbSet<Biller> Billers { get; set; }
        public DbSet<BillPayment> BillPayments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<AdminActionsLog> AdminActionsLogs { get; set; }
        public DbSet<FraudLog> FraudLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all configurations from assembly automatically
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            

            base.OnModelCreating(modelBuilder);
        }
    }
}