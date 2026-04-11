using RecipeShare.Application.DTOs.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Services;

public interface IAdminTagService
{
    Task<List<TagResponse>> GetTagsAsync();
    Task<TagResponse> GetTagByIdAsync(int id);
    Task<int> CreateTagAsync(CreateTagRequest request);
    Task UpdateTagAsync(int id, CreateTagRequest request);
    Task DeleteTagAsync(int id);
}
