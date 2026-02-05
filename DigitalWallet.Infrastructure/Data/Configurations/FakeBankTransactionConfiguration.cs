using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class FakeBankTransactionConfiguration : IEntityTypeConfiguration<FakeBankTransaction>
    {
        public void Configure(EntityTypeBuilder<FakeBankTransaction> builder)
        {
            builder.ToTable("FakeBankTransactions");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.UserId)
                .IsRequired();

            builder.Property(f => f.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(f => f.Type)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(f => f.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(f => f.DelaySeconds)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.HasIndex(f => f.UserId);
        }
    }
}