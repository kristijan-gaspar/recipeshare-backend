using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Reports;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Enums;

namespace RecipeShare.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public AdminReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ReportResponse>>> GetAll([FromQuery] AdminReportsListQuery query)
        {
            var result = await _reportService.GetReportsAsync(query.ReportStatus, query.PageNumber, query.PageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReportDetailResponse>> GetById(int id)
        {
            var result = await _reportService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPatch("{id:int}/resolve")]
        public async Task<IActionResult> Resolve(int id, [FromBody] ResolveReportRequest request)
        {
            var adminId = User.GetUserId();
            await _reportService.ResolveAsync(id, adminId, request);
            return NoContent();
        }

        [HttpPatch("{id:int}/dismiss")]
        public async Task<IActionResult> Dismiss(int id)
        {
            var adminId = User.GetUserId();
            await _reportService.DismissAsync(id, adminId);
            return NoContent();
        }
    }
}
