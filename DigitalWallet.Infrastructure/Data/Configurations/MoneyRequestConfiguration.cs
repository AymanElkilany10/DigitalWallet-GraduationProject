using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class MoneyRequestConfiguration : IEntityTypeConfiguration<MoneyRequest>
    {
        public void Configure(EntityTypeBuilder<MoneyRequest> builder)
        {
            builder.ToTable("MoneyRequests");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.FromUserId)
                .IsRequired();

            builder.Property(m => m.ToUserId)
                .IsRequired();

            builder.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(m => m.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(m => m.Status)
                .IsRequired()
                .HasDefaultValue(MoneyRequestStatus.Pending);

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.HasIndex(m => m.FromUserId);
            builder.HasIndex(m => m.ToUserId);
            builder.HasIndex(m => m.Status);
        }
    }
}