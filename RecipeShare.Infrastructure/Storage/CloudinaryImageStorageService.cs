using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using RecipeShare.Application.DTOs.Storage;
using RecipeShare.Application.Enums;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Infrastructure.Storage;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;

    public CloudinaryImageStorageService(
        Cloudinary cloudinary,
        IOptions<CloudinarySettings> options)
    {
        _cloudinary = cloudinary;
        _settings = options.Value;
    }

    public async Task<UploadedImage> UploadAsync(
        Stream imageStream,
        string fileName,
        ImageFolder folder,
        CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, imageStream),
            Folder = $"{_settings.RootFolder}/{folder.ToFolderName()}",
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error is not null || (int)result.StatusCode >= 400)
            throw new ImageStorageException(
                $"Cloudinary upload failed: {result.Error?.Message ?? result.StatusCode.ToString()}");

        return new UploadedImage(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteAsync(string publicId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return;

        var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
        var result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Error is not null)
            throw new ImageStorageException(
                $"Cloudinary delete failed: {result.Error.Message}");
    }
}
