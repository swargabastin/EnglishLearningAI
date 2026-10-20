using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly ISpeechConversationService _speechService;
    private readonly IAssessmentService _assessmentService;
    private readonly IValidator<StartConversationRequest> _startValidator;
    private readonly IValidator<SendMessageRequest> _messageValidator;

    public ConversationsController(
        IConversationService conversationService,
        ISpeechConversationService speechService,
        IAssessmentService assessmentService,
        IValidator<StartConversationRequest> startValidator,
        IValidator<SendMessageRequest> messageValidator)
    {
        _conversationService = conversationService;
        _speechService = speechService;
        _assessmentService = assessmentService;
        _startValidator = startValidator;
        _messageValidator = messageValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ConversationSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _startValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var session = await _conversationService.StartConversationAsync(request, cancellationToken);
        return Ok(ApiResponse<ConversationSessionDto>.Ok(session, "Conversation session started."));
    }

    [HttpPost("{sessionId:guid}/messages")]
    [ProducesResponseType(typeof(ApiResponse<SendMessageResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _messageValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var response = await _conversationService.SendMessageAsync(sessionId, request, cancellationToken);
        return Ok(ApiResponse<SendMessageResponseDto>.Ok(response));
    }

    [HttpPost("{sessionId:guid}/speech")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<SpeechConversationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendSpeech(Guid sessionId, IFormFile? audio, CancellationToken cancellationToken)
    {
        if (audio == null || audio.Length == 0)
        {
            throw new InvalidAudioException("An audio file must be provided in the 'audio' form field.");
        }

        using var stream = audio.OpenReadStream();
        var response = await _speechService.ProcessSpeechAsync(
            sessionId,
            stream,
            audio.FileName,
            audio.ContentType,
            audio.Length,
            cancellationToken
        );

        return Ok(ApiResponse<SpeechConversationResponseDto>.Ok(response, "Speech processed successfully."));
    }

    [HttpGet("{sessionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ConversationSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _conversationService.GetConversationByIdAsync(sessionId, cancellationToken);
        return Ok(ApiResponse<ConversationSessionDto>.Ok(session));
    }

    [HttpPost("{sessionId:guid}/end")]
    [ProducesResponseType(typeof(ApiResponse<ConversationSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndConversation(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _conversationService.EndConversationAsync(sessionId, cancellationToken);
        return Ok(ApiResponse<ConversationSessionDto>.Ok(session, "Conversation session ended."));
    }

    [HttpPost("{sessionId:guid}/assessment")]
    [ProducesResponseType(typeof(ApiResponse<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateAssessment(Guid sessionId, CancellationToken cancellationToken)
    {
        var assessment = await _assessmentService.GenerateAssessmentAsync(sessionId, cancellationToken);
        return Ok(ApiResponse<AssessmentResponseDto>.Ok(assessment, "Assessment generated successfully."));
    }

    [HttpGet("{sessionId:guid}/assessment")]
    [ProducesResponseType(typeof(ApiResponse<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssessment(Guid sessionId, CancellationToken cancellationToken)
    {
        var assessment = await _assessmentService.GetAssessmentBySessionIdAsync(sessionId, cancellationToken);
        return Ok(ApiResponse<AssessmentResponseDto>.Ok(assessment));
    }
}
