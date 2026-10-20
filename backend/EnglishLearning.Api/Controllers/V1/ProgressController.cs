using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _progressService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<DashboardResponseDto>.Ok(dashboard));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedListDto<HistoryItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var history = await _progressService.GetHistoryAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PaginatedListDto<HistoryItemDto>>.Ok(history));
    }

    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SessionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessionDetail(Guid sessionId, CancellationToken cancellationToken)
    {
        var detail = await _progressService.GetSessionDetailAsync(sessionId, cancellationToken);
        return Ok(ApiResponse<SessionDetailDto>.Ok(detail));
    }
}
