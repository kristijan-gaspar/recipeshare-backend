using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<List<User>> SearchByUsername(string query, int pageNumber, int pageSize);
    Task<int> CountByUsernameAsync(string query);
    Task<IEnumerable<User>> GetAllPagedAsync(string? query, int pageNumber, int pageSize);
    Task<int> CountAllAsync(string? query);
}
