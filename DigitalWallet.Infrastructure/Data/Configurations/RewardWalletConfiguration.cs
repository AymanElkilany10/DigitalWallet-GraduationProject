using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class RewardWalletConfiguration : IEntityTypeConfiguration<RewardWallet>
    {
        public void Configure(EntityTypeBuilder<RewardWallet> builder)
        {
            builder.ToTable("RewardWallets");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.UserId)
                .IsRequired();

            builder.HasIndex(r => r.UserId)
                .IsUnique();

            builder.Property(r => r.Balance)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m);

            builder.Property(r => r.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("EGP");

            builder.Property(r => r.CreatedAt)
                .IsRequired();
        }
    }
}