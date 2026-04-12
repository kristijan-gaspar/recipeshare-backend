using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Tags;

public class CreateTagRequest
{
    [Required(ErrorMessage = "Tag name is required!")]
    [MaxLength(50, ErrorMessage = "Tag name can't be longer than 50 characters!")]
    public string Name { get; set; } = String.Empty!;
}
