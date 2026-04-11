using RecipeShare.Application.DTOs.Storage;
using RecipeShare.Application.Enums;

namespace RecipeShare.Application.Interfaces.Services;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadAsync(
        Stream imageStream,
        string fileName,
        ImageFolder folder,
        CancellationToken ct = default);

    Task DeleteAsync(string publicId, CancellationToken ct = default);
}
