namespace RecipeShare.Application.Enums;

public enum ImageFolder
{
    Profiles,
    Recipes
}

public static class ImageFolderExtensions
{
    public static string ToFolderName(this ImageFolder folder) => folder switch
    {
        ImageFolder.Profiles => "profiles",
        ImageFolder.Recipes => "recipes",
        _ => throw new ArgumentOutOfRangeException(nameof(folder))
    };
}
