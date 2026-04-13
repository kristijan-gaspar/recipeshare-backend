using RecipeShare.Application.DTOs.Tags;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TagService(ITagRepository tagRepo, IUnitOfWork unitOfWork)
    {
        _tagRepo = tagRepo;
        _unitOfWork = unitOfWork;
    }

    //---User----
    public async Task<List<TagResponse>> GetTagsAsync()
    {
        var tags = await _tagRepo.GetAllAsync();

        return tags
            .Where(t => t.IsActive)
            .Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            RecipeCount = 0
        }).ToList();
    }

    public async Task<TagResponse> GetTagByIdAsync(int id)
    {
        var tag = await _tagRepo.GetByIdAsync(id);

        if(tag == null)
            throw new NotFoundException("Tag not found.");

        return new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            RecipeCount = 0
        };
    }

    //---Admin----
    public async Task<List<AdminTagResponse>> GetAdminTagsAsync()
    {
        var tags = await _tagRepo.GetAllAsync();

        return tags.Select(t => new AdminTagResponse
            {
                Id = t.Id,
                Name = t.Name,
                RecipeCount = 0,
                CreatedaAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                IsActive = t.IsActive
        }).ToList();
    }

    public async Task<AdminTagResponse> GetAdminTagByIdAsync(int id)
    {
        var tag = await _tagRepo.GetByIdAsync(id);

        if (tag == null)
            throw new NotFoundException("Tag not found.");

        return new AdminTagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            RecipeCount = 0,
            CreatedaAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt,
            IsActive = tag.IsActive
        };
    }

    public async Task<int> CreateTagAsync(CreateTagRequest request)
    {
        var normalizedName = request.Name.Trim();
        if (await _tagRepo.NameExistsAsync(normalizedName))
            throw new BadRequestException("Tag with the same name already exists.");

        var tag = new Tag
        {
            Name = normalizedName,
            CreatedAt = DateTime.UtcNow
        };

        await _tagRepo.AddAsync(tag);
        await _unitOfWork.SaveChangesAsync();

        return tag.Id;
    }

    public async Task UpdateTagAsync(int id, CreateTagRequest request)
    {
        var tag = await _tagRepo.GetByIdAsync(id);

        if(tag == null)
            throw new NotFoundException("Tag not found.");

        var normalizedName = request.Name.Trim();

        tag.UpdatedAt = DateTime.UtcNow;

        var nameExists = await _tagRepo.NameExistsAsync(normalizedName);
        if (nameExists && !string.Equals(tag.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Tag with the same name already exists.");

        tag.Name = normalizedName;

        _tagRepo.Update(tag);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteTagAsync(int id)
    {
        var tag = _tagRepo.GetByIdAsync(id).Result;

        if(tag == null)
            throw new NotFoundException("Tag not found.");

        _tagRepo.Delete(tag);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var tag = await _tagRepo.GetByIdAsync(id);

        if (tag == null)
            throw new NotFoundException("Category not found.");

        tag.IsActive = !tag.IsActive;

        _tagRepo.Update(tag);
        await _unitOfWork.SaveChangesAsync();
    }
}
