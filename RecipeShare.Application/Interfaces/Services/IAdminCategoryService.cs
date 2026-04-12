using RecipeShare.Application.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Services;

public interface IAdminCategoryService
{
    Task<List<CategoryResponse>> GetCategoriesAsync();
    Task<CategoryResponse> GetCategoryByIdAsync(int id);
    Task<int> CreateCategoryAsync(CreateCategoryRequest request);
    Task UpdateCategoryAsync(int id, CreateCategoryRequest request);
    Task DeleteCategoryAsync(int id);
}
