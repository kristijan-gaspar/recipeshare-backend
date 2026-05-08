namespace RecipeShare.Application.DTOs.Reports;

public class ReportDetailResponse
{
    public int Id { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string ReporterUsername { get; set; } = string.Empty;
    public int ReporterId { get; set; }
    public string ReportedUsername { get; set; } = string.Empty;
    public int ReportedUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ContentAction { get; set; } = string.Empty;
    public string UserAction { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public string? ResolvedByAdminUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? TargetContent { get; set; }
}
