using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Categories;

public class CategoryResponse
{

    public int Id { get; set; }
    public string Name { get; set; } = String.Empty!;
    public string Type { get; set; } = "category";
    public int RecipeCount { get; set; } = 0;
}
