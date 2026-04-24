using Microsoft.EntityFrameworkCore;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Step> Steps => Set<Step>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionRecipe> CollectionRecipes => Set<CollectionRecipe>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
