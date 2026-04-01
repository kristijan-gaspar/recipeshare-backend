namespace RecipeShare.Application.Interfaces.Services;

public interface IImageStorageService
{
    Task<string> UploadAsync(Stream imageStream, string fileName, CancellationToken ct = default);
    Task DeleteAsync(string imageUrl, CancellationToken ct = default);
}