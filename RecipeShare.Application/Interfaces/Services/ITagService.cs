using RecipeShare.Application.DTOs.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Services;

public interface ITagService
{
    Task<List<TagResponse>> GetTagsAsync();
    Task<List<AdminTagResponse>> GetAdminTagsAsync();
    Task<TagResponse> GetTagByIdAsync(int id);
    Task<AdminTagResponse> GetAdminTagByIdAsync(int id);
    Task<int> CreateTagAsync(CreateTagRequest request);
    Task UpdateTagAsync(int id, CreateTagRequest request);
    Task DeleteTagAsync(int id);
    Task ToggleActiveAsync(int id);
}
