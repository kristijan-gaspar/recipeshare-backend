using CloudinaryDotNet;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Auth;
using RecipeShare.Infrastructure.Data;
using RecipeShare.Infrastructure.Firebase;
using RecipeShare.Infrastructure.Repositories;
using RecipeShare.Infrastructure.Storage;

namespace RecipeShare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();


        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();

        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        // Firebase
        services.Configure<FirebaseSettings>(config.GetSection("Firebase"));
        services.AddSingleton(_ =>
        {
            var settings = config.GetSection("Firebase").Get<FirebaseSettings>()!;
            var credential = GoogleCredential.FromFile(settings.CredentialsFilePath);
            return FirebaseApp.Create(new AppOptions { Credential = credential });
        });

        // Cloudinary
        services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
            return new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
        });

        // Infrastructure services
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IRecipeSocialStatsService, RecipeSocialStatsService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IUserStatsService, UserStatsService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminRecipeService, AdminRecipeService>();
        services.AddScoped<IAdminCommentService, AdminCommentService>();

        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ICollectionService, CollectionService>();

        services.AddScoped<IFeedService, FeedService>();
        services.AddScoped<IRecipeResponseMapper, RecipeResponseMapper>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<IDeviceTokenService, DeviceTokenService>();
        services.AddScoped<IPushNotificationSender, FirebasePushNotificationSender>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
