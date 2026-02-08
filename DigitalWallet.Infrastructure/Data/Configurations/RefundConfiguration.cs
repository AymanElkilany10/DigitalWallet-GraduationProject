using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.ToTable("Refunds");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.OriginalTransactionId)
                .IsRequired();

            builder.Property(r => r.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.Reason)
                .HasMaxLength(255);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasDefaultValue(TransactionStatus.Pending);

            builder.Property(r => r.CreatedAt)
                .IsRequired();
        }
    }
}