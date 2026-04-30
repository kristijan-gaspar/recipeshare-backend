using RecipeShare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IFollowRepository : IGenericRepository<Follow>
{
    Task<Follow?> GetByUsersAsync(int followerId, int followedId);
    Task<int> GetFollowerCountAsync(int userId);
    Task<int> GetFollowingCountAsync(int userId);
    Task<bool> ExistsAsync(int followerId, int followedId);
    Task<List<int>> GetFollowingUserIdsAsync(int userId);
}
