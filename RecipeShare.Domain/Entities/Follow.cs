using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Domain.Entities;

public class Follow
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int FollowerId { get; set; }
    public User Follower { get; set; } = null!;

    public int FollowedId { get; set; }
    public User Followed { get; set; } = null!;
}
