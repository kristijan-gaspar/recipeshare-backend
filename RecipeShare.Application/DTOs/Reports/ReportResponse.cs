namespace RecipeShare.Application.DTOs.Reports;

public class ReportResponse
{
    public int Id { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string ReporterUsername { get; set; } = string.Empty;
    public string ReportedUsername { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
