using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.UserId)
                .IsRequired();

            builder.Property(w => w.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("EGP");

            builder.Property(w => w.Balance)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m);

            builder.Property(w => w.DailyLimit)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(5000.00m);

            builder.Property(w => w.MonthlyLimit)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(20000.00m);

            builder.Property(w => w.CreatedAt)
                .IsRequired();

            builder.HasIndex(w => w.UserId);
            builder.HasIndex(w => w.CurrencyCode);

            // Relationships
            builder.HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.SentTransfers)
                .WithOne(t => t.SenderWallet)
                .HasForeignKey(t => t.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.ReceivedTransfers)
                .WithOne(t => t.ReceiverWallet)
                .HasForeignKey(t => t.ReceiverWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.BillPayments)
                .WithOne(b => b.Wallet)
                .HasForeignKey(b => b.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}