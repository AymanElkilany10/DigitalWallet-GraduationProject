using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class BillerConfiguration : IEntityTypeConfiguration<Biller>
    {
        public void Configure(EntityTypeBuilder<Biller> builder)
        {
            builder.ToTable("Billers");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(b => b.Category)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(b => b.BillPayments)
                .WithOne(p => p.Biller)
                .HasForeignKey(p => p.BillerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}