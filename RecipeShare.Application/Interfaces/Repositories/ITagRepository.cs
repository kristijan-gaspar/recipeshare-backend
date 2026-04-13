using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface ITagRepository : IGenericRepository<Tag>
{
    Task<List<Tag>> GetAllAsync(string? searchTerm = null);
    Task<bool> NameExistsAsync(string name);
}
