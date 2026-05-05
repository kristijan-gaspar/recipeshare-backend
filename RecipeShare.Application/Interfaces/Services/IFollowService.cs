using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Services;

public interface IFollowService
{
    public Task<bool> ToggleFollowAsync(int targetUserId, int currentUserId, string followerUsername);
}
