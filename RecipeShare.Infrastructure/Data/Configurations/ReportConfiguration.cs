using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TargetType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(r => r.ContentAction)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(r => r.UserAction)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(r => r.AdminNote)
            .HasMaxLength(500);

        builder.HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReportedUser)
            .WithMany()
            .HasForeignKey(r => r.ReportedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ResolvedByAdmin)
            .WithMany()
            .HasForeignKey(r => r.ResolvedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.TargetType, r.TargetId });
        builder.HasIndex(r => new { r.ReporterId, r.TargetType, r.TargetId });
    }
}
