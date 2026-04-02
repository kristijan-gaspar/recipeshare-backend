using RecipeShare.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;

namespace RecipeShare.Infrastructure.Services;

public class LocalImageStorageService : IImageStorageService
{
    private readonly string _rootPath;

    public LocalImageStorageService(IWebHostEnvironment environment)
    {
        _rootPath = Path.Combine(environment.WebRootPath ?? "wwwroot", "uploads", "profiles");
    }

    public async Task<string> UploadAsync(Stream imageStream, string fileName, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_rootPath);

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_rootPath, uniqueFileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await imageStream.CopyToAsync(fileStream, ct);

        return $"/uploads/profiles/{uniqueFileName}";
    }

    public Task DeleteAsync(string imageUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(imageUrl);
        var fullPath = Path.Combine(_rootPath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}