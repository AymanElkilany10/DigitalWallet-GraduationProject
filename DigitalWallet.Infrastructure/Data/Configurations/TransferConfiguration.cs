using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
    {
        public void Configure(EntityTypeBuilder<Transfer> builder)
        {
            builder.ToTable("Transfers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.SenderWalletId)
                .IsRequired();

            builder.Property(t => t.ReceiverWalletId)
                .IsRequired();

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

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.HasIndex(t => t.SenderWalletId);
            builder.HasIndex(t => t.ReceiverWalletId);
            builder.HasIndex(t => t.CreatedAt);
        }
    }
}