using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class FakeBankAccountConfiguration : IEntityTypeConfiguration<FakeBankAccount>
    {
        public void Configure(EntityTypeBuilder<FakeBankAccount> builder)
        {
            builder.ToTable("FakeBankAccounts");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.UserId)
                .IsRequired();

            builder.HasIndex(f => f.UserId)
                .IsUnique();

            builder.Property(f => f.AccountNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(f => f.AccountNumber)
                .IsUnique();

            builder.Property(f => f.Balance)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m);

            builder.Property(f => f.CreatedAt)
                .IsRequired();
        }
    }
}