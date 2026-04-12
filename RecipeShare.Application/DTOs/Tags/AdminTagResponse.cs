using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Tags;

public class AdminTagResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty!;
    public string Type { get; set; } = "tag";
    public int RecipeCount { get; set; } = 0;
    public DateTime CreatedaAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
