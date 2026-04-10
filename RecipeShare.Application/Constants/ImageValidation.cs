namespace RecipeShare.Application.Constants;

public static class ImageValidation
{
    public static readonly IReadOnlySet<string> AllowedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
}
