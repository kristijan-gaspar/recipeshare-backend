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
}
