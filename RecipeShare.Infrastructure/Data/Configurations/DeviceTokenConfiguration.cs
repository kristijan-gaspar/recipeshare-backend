using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Infrastructure.Data.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(d => d.Token)
            .IsUnique();

        builder.HasIndex(d => d.UserId);

        builder.HasOne(d => d.User)
            .WithMany(u => u.DeviceTokens)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
