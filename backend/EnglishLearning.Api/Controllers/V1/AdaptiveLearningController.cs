using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AdaptiveLearningController : ControllerBase
{
    private readonly IAdaptiveLearningService _adaptiveLearningService;

    public AdaptiveLearningController(IAdaptiveLearningService adaptiveLearningService)
    {
        _adaptiveLearningService = adaptiveLearningService;
    }

    [HttpGet("assessment/questions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AssessmentQuestionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDiagnosticQuestions(CancellationToken cancellationToken)
    {
        var questions = await _adaptiveLearningService.GetDiagnosticQuestionsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AssessmentQuestionDto>>.Ok(questions));
    }

    [HttpPost("assessment/submit")]
    [ProducesResponseType(typeof(ApiResponse<LearnerAssessmentResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitAssessment([FromBody] SubmitAssessmentRequest request, CancellationToken cancellationToken)
    {
        if (request.Answers == null || request.Answers.Count == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Assessment Submission",
                Detail = "At least one question must be answered to evaluate your English proficiency.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _adaptiveLearningService.SubmitAssessmentAsync(request, cancellationToken);
        return Ok(ApiResponse<LearnerAssessmentResultDto>.Ok(result, "Assessment evaluated and learning level assigned."));
    }

    [HttpGet("assessment/latest")]
    [ProducesResponseType(typeof(ApiResponse<LearnerAssessmentResultDto?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestAssessment(CancellationToken cancellationToken)
    {
        var assessment = await _adaptiveLearningService.GetLatestAssessmentAsync(cancellationToken);
        return Ok(ApiResponse<LearnerAssessmentResultDto?>.Ok(assessment));
    }

    [HttpGet("plan/today")]
    [ProducesResponseType(typeof(ApiResponse<DailyStudyPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayPlan(
        [FromQuery] string? date = null,
        [FromQuery] bool forceRegenerate = false,
        CancellationToken cancellationToken = default)
    {
        var plan = await _adaptiveLearningService.GetOrCreateTodayPlanAsync(date, forceRegenerate, cancellationToken);
        return Ok(ApiResponse<DailyStudyPlanDto>.Ok(plan));
    }

    [HttpPost("plan/phase/{phaseId:guid}/progress")]
    [ProducesResponseType(typeof(ApiResponse<PhaseCompletionResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePhaseProgress(
        Guid phaseId,
        [FromBody] UpdatePhaseProgressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adaptiveLearningService.UpdatePhaseProgressAsync(phaseId, request, cancellationToken);
        return Ok(ApiResponse<PhaseCompletionResultDto>.Ok(result, "Active learning progress updated."));
    }

    [HttpPost("plan/phase/{phaseId:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<PhaseCompletionResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompletePhase(
        Guid phaseId,
        [FromBody] CompletePhaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adaptiveLearningService.CompletePhaseAsync(phaseId, request, cancellationToken);
        return Ok(ApiResponse<PhaseCompletionResultDto>.Ok(result, "Phase completed and scored."));
    }

    [HttpGet("plan/history")]
    [ProducesResponseType(typeof(ApiResponse<LearningPlanHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLearningHistory(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var history = await _adaptiveLearningService.GetLearningHistoryAsync(limit, cancellationToken);
        return Ok(ApiResponse<LearningPlanHistoryDto>.Ok(history));
    }

    // =========================================================================
    // Option 3: Bilingual Translation Mode
    // =========================================================================
    [HttpPost("translate")]
    [ProducesResponseType(typeof(ApiResponse<TranslateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TranslatePhrase(
        [FromBody] TranslateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = "Text to translate cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _adaptiveLearningService.TranslateToEnglishAsync(request, cancellationToken);
        return Ok(ApiResponse<TranslateResultDto>.Ok(result, "Translation and grammar analysis complete."));
    }

    // =========================================================================
    // Option 4: Document / PDF English Practice
    // =========================================================================
    [HttpPost("document/analyze")]
    [ProducesResponseType(typeof(ApiResponse<DocumentAnalysisResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeDocument(
        [FromBody] DocumentAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentText))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = "Document text cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _adaptiveLearningService.AnalyzeDocumentAsync(request, cancellationToken);
        return Ok(ApiResponse<DocumentAnalysisResultDto>.Ok(result, "Document analyzed for discussion."));
    }

    [HttpPost("document/upload")]
    [ProducesResponseType(typeof(ApiResponse<DocumentAnalysisResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = "Please provide a valid document or text file.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        string content;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            content = await reader.ReadToEndAsync(cancellationToken);
        }

        var request = new DocumentAnalysisRequest(file.FileName, content);
        var result = await _adaptiveLearningService.AnalyzeDocumentAsync(request, cancellationToken);
        return Ok(ApiResponse<DocumentAnalysisResultDto>.Ok(result, "Uploaded file parsed and analyzed."));
    }
}
