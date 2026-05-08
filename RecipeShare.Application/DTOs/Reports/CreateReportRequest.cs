using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Reports;

public class CreateReportRequest
{
    public ReportTargetType TargetType { get; set; }
    public int TargetId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
}
