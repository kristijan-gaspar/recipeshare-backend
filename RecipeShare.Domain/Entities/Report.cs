using RecipeShare.Domain.Enums;

namespace RecipeShare.Domain.Entities;

public class Report
{
    public int Id { get; set; }

    public ReportTargetType TargetType { get; set; }
    public int TargetId { get; set; }
    public int ReportedUserId { get; set; }

    public int ReporterId { get; set; }

    public ReportReason Reason { get; set; }
    public string? Description { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public AdminAction ContentAction { get; set; } = AdminAction.None;
    public AdminAction UserAction { get; set; } = AdminAction.None;
    public string? AdminNote { get; set; }
    public int? ResolvedByAdminId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public User Reporter { get; set; } = null!;
    public User ReportedUser { get; set; } = null!;
    public User? ResolvedByAdmin { get; set; }
}
