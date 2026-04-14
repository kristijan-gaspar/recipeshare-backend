using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeShare.Application.DTOs.Categories;
using RecipeShare.Domain.Entities;  

namespace RecipeShare.Application.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdWithRecipesAsync(int id);
    Task<bool> NameExistsAsync(string name);
}
