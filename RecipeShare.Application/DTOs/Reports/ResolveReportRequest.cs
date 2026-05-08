using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Reports;

public class ResolveReportRequest
{
    public AdminAction ContentAction { get; set; }
    public AdminAction UserAction { get; set; }
    public string? AdminNote { get; set; }
}
