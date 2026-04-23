using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Domain.Entities;

public class CollectionRecipe
{
    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}
