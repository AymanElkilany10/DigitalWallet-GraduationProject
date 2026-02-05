using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Infrastructure.Data.Configurations
{
    public class AdminActionsLogConfiguration : IEntityTypeConfiguration<AdminActionsLog>
    {
        public void Configure(EntityTypeBuilder<AdminActionsLog> builder)
        {
            builder.ToTable("AdminActionsLogs");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AdminId)
                .IsRequired();

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.TargetUserId)
                .IsRequired(false);

            builder.Property(a => a.Data)
                .HasColumnType("nvarchar(max)");

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.HasIndex(a => a.AdminId);
            builder.HasIndex(a => a.TargetUserId);

            // Relationship with User (optional)
            builder.HasOne(a => a.TargetUser)
                .WithMany()
                .HasForeignKey(a => a.TargetUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}