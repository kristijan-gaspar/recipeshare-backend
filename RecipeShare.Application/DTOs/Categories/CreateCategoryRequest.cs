using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Categories;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required!")]
    [MaxLength(50, ErrorMessage = "Category name can't be longer than 50 characters!")]
    public string Name { get; set; } = String.Empty!;
}
