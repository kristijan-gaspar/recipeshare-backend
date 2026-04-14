using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Users
{
    public class UserSearchResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = String.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}
