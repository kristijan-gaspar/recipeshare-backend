using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
