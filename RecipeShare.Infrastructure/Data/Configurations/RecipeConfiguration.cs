using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeShare.Domain.Entities;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Infrastructure.Data.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.Property(r => r.ImageUrl)
            .HasMaxLength(500);

        builder.Property(r => r.ImagePublicId)
            .HasMaxLength(200);

        builder.Property(r => r.Difficulty)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasOne(r => r.User)
            .WithMany(u => u.Recipes)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Category)
            .WithMany(c => c.Recipes)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Tags)
            .WithMany(t => t.Recipes)
            .UsingEntity(j => j.ToTable("RecipeTags"));

        builder.HasIndex(r => new { r.CreatedAt, r.Id });
    }
}
