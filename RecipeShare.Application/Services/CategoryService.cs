using RecipeShare.Application.DTOs.Categories;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace RecipeShare.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepo, IUnitOfWork unitOfWork)
    {
        _categoryRepo = categoryRepo;
        _unitOfWork = unitOfWork;
    }


    //----User----
    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();

        return categories
            .Where(c => c.IsActive)
            .Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            RecipeCount = 0
        }).ToList();
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);

        if(category == null)
            throw new NotFoundException("Category not found.");

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            RecipeCount = 0
        };
    }


    //----Admin----
    public async Task<List<AdminCategoryResponse>> GetAdminCategoriesAsync(string? searchTerm)
    {
        var categories = await _categoryRepo.GetAllAsync(searchTerm);

        return categories.Select(c => new AdminCategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            RecipeCount = 0,
            CreatedaAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            IsActive = c.IsActive,
        }).ToList();

    }

    public async Task<AdminCategoryResponse> GetAdminCategoryByIdAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);

        if (category == null)
            throw new NotFoundException("Category not found.");

        return new AdminCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            RecipeCount = 0,
            CreatedaAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            IsActive = category.IsActive,
        };
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryRequest request)
    {
        if(await _categoryRepo.NameExistsAsync(request.Name))
            throw new BadRequestException("Category with the same name already exists.");

        var normalizedName = request.Name.Trim();

        var category = new Category
        {
            Name = normalizedName,
            CreatedAt = DateTime.UtcNow,
        };

        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category.Id;
    }

    public async Task UpdateCategoryAsync(int id, CreateCategoryRequest request)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if(category == null)
            throw new NotFoundException("Category not found.");

        var nameExists = await _categoryRepo.NameExistsAsync(request.Name);
        if(nameExists && !string.Equals(category.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Category with the same name already exists.");

        var normalizedName = request.Name.Trim();
        category.Name = normalizedName;

        category.UpdatedAt = DateTime.UtcNow;

        _categoryRepo.Update(category);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);

        if(category == null)
            throw new NotFoundException("Category not found.");

        _categoryRepo.Delete(category);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);

        if (category == null)
            throw new NotFoundException("Category not found.");

        category.IsActive = !category.IsActive;

        _categoryRepo.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
