using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Users
{
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }

        public int RecipeCount { get; set; } = 0;
        public int FollowerCount { get; set; } = 0;
        public int FollowingCount { get; set; } = 0;
        public bool IsFollowedByCurrentUser { get; set; } = false;
    }
}
