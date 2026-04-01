using RecipeShare.Domain.Entities;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);


    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetProfileByIdAsync(int id);
}
