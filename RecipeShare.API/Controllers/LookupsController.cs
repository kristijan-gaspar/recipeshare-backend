using Microsoft.AspNetCore.Mvc;
using RecipeShare.Domain.Enums;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    [HttpGet("measurement-units")]
    public ActionResult<IEnumerable<string>> GetMeasurementUnits()
    {
        return Ok(Enum.GetNames<MeasurementUnit>());
    }

    [HttpGet("difficulty-levels")]
    public ActionResult<IEnumerable<string>> GetDifficultyLevels()
    {
        return Ok(Enum.GetNames<DifficultyLevel>());
    }

    [HttpGet("report-reasons")]
    public ActionResult<IEnumerable<string>> GetReportReasons()
    {
        return Ok(Enum.GetNames<ReportReason>());
    }

    [HttpGet("report-target-types")]
    public ActionResult<IEnumerable<string>> GetReportTargetTypes()
    {
        return Ok(Enum.GetNames<ReportTargetType>());
    }

    [HttpGet("admin-actions")]
    public ActionResult<IEnumerable<string>> GetAdminActions()
    {
        return Ok(Enum.GetNames<AdminAction>());
    }
}
