using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Auth
{
    public record TokenResult(string Token, DateTime ExpiresAt);
}
