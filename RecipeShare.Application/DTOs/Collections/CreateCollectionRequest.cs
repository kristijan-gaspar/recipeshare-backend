using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Collections;

public class CreateCollectionRequest
{
    [Required(ErrorMessage = "Collection name is required.")]
    [StringLength(100, ErrorMessage = "Collection name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;
}
