using DigitalWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
    {
        public void Configure(EntityTypeBuilder<ExchangeRate> builder)
        {
            builder.ToTable("ExchangeRates");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.FromCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(e => e.ToCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(e => e.Rate)
                .HasColumnType("decimal(18, 6)");

            builder.Property(e => e.Source)
                .HasMaxLength(50);

            builder.HasIndex(e => new { e.FromCurrency, e.ToCurrency })
                .IsUnique();
        }
    }

    public class CurrencyExchangeConfiguration : IEntityTypeConfiguration<CurrencyExchange>
    {
        public void Configure(EntityTypeBuilder<CurrencyExchange> builder)
        {
            builder.ToTable("CurrencyExchanges");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.FromAmount)
                .HasColumnType("decimal(18, 2)");

            builder.Property(e => e.ToAmount)
                .HasColumnType("decimal(18, 2)");

            builder.Property(e => e.ExchangeRate)
                .HasColumnType("decimal(18, 6)");

            builder.Property(e => e.Fee)
                .HasColumnType("decimal(18, 2)");

            builder.Property(e => e.FromCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(e => e.ToCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(e => e.Status)
                .HasMaxLength(20);

            // Relationships
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.FromWallet)
                .WithMany()
                .HasForeignKey(e => e.FromWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ToWallet)
                .WithMany()
                .HasForeignKey(e => e.ToWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.CreatedAt);
        }
    }
}