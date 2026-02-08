using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class BillPaymentConfiguration : IEntityTypeConfiguration<BillPayment>
    {
        public void Configure(EntityTypeBuilder<BillPayment> builder)
        {
            builder.ToTable("BillPayments");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.UserId)
                .IsRequired();

            builder.Property(b => b.WalletId)
                .IsRequired();

            builder.Property(b => b.BillerId)
                .IsRequired();

            builder.Property(b => b.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(b => b.Status)
                .IsRequired()
                .HasDefaultValue(TransactionStatus.Pending);

            builder.Property(b => b.ReceiptPath)
                .HasMaxLength(255);

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.WalletId);
            builder.HasIndex(b => b.BillerId);
        }
    }
}