using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
    {
        public void Configure(EntityTypeBuilder<AdminUser> builder)
        {
            builder.ToTable("AdminUsers");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(a => a.Email)
                .IsUnique();

            builder.Property(a => a.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(AdminRole.Support);  // ✅ Use enum value

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(a => a.Actions)
                .WithOne(l => l.Admin)
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}