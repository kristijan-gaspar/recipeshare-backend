using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeShare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Infrastructure.Data.Configurations;

internal class CollectionRecipeConfiguration : IEntityTypeConfiguration<CollectionRecipe>
{
    public void Configure(EntityTypeBuilder<CollectionRecipe> builder)
    {
        builder.HasKey(cr => new { cr.CollectionId, cr.RecipeId });

        builder.Property(cr => cr.CollectionId).IsRequired();

        builder.HasOne(cr => cr.Collection)
            .WithMany(c => c.CollectionRecipes)
            .HasForeignKey(cr => cr.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.Recipe)
            .WithMany(r => r.CollectionRecipes)
            .HasForeignKey(cr => cr.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
