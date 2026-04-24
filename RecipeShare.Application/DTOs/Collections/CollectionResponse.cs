using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Collections;

public class CollectionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RecipeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
