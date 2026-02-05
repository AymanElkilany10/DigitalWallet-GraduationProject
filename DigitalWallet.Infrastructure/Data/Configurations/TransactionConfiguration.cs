using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.WalletId)
                .IsRequired();

            builder.Property(t => t.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(t => t.Description)
                .HasMaxLength(255);

            builder.Property(t => t.Reference)
                .HasMaxLength(100);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.HasIndex(t => t.WalletId);
            builder.HasIndex(t => t.Type);
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => t.CreatedAt);

            // Relationship
            builder.HasOne(t => t.Refund)
                .WithOne(r => r.OriginalTransaction)
                .HasForeignKey<Refund>(r => r.OriginalTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}