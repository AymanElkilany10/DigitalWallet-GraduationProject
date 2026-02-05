using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class FraudLogConfiguration : IEntityTypeConfiguration<FraudLog>
    {
        public void Configure(EntityTypeBuilder<FraudLog> builder)
        {
            builder.ToTable("FraudLogs");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.UserId)
                .IsRequired();

            builder.Property(f => f.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(f => f.Description)
                .HasMaxLength(255);

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.HasIndex(f => f.UserId);
            builder.HasIndex(f => f.CreatedAt);
        }
    }
}