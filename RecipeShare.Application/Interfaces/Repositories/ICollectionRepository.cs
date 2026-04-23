using RecipeShare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface ICollectionRepository : IGenericRepository<Collection>
{
    Task<List<Collection>> GetByUserIdAsync(int userId);
    Task<Collection?> GetWithRecipesAsync(int collectionId);
    Task<Collection?> GetByIdAndUserIdAsync(int collectionId, int userId);
    Task<bool> NameExistsForUserAsync(int userId, string name);
}
