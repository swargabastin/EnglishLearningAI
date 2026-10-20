using System.Text.Json;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Application.Services;

public class AssessmentService : IAssessmentService
{
    private readonly IConversationSessionRepository _sessionRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiAssessmentService _aiAssessmentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AssessmentService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AssessmentService(
        IConversationSessionRepository sessionRepository,
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        IAiAssessmentService aiAssessmentService,
        ICurrentUserService currentUserService,
        ILogger<AssessmentService> logger)
    {
        _sessionRepository = sessionRepository;
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _aiAssessmentService = aiAssessmentService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AssessmentResponseDto> GenerateAssessmentAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new EntityNotFoundException(nameof(ConversationSession), sessionId);
        }

        if (session.UserId != currentUserId)
        {
            throw new UnauthorizedSessionAccessException(sessionId);
        }

        if (session.Status != SessionStatus.Completed)
        {
            throw new InvalidSessionStateException("Assessment can only be generated for completed conversation sessions.");
        }

        var userMessages = session.Messages.Where(m => m.Role == SpeakerRole.User).ToList();
        if (userMessages.Count == 0)
        {
            throw new InvalidSessionStateException("Cannot generate assessment for a conversation session with no user messages.");
        }

        // Idempotency: Return existing assessment if already generated
        var existing = await _assessmentRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (existing != null)
        {
            _logger.LogInformation("Returning existing assessment for session {SessionId}", sessionId);
            return MapToDto(existing, session);
        }

        // Build transcript from stored messages
        var transcript = session.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new AiMessageContext(
                m.Role == SpeakerRole.User ? "user" : "assistant",
                m.MessageText
            ))
            .ToList();

        var topicName = session.Topic?.Name ?? "General English Practice";
        var topicDesc = session.Topic?.Description ?? "English Conversation Practice";

        var aiRequest = new AssessmentAiRequest(
            topicName,
            topicDesc,
            session.EnglishLevel.ToString(),
            transcript
        );

        _logger.LogInformation("Generating AI assessment for completed session {SessionId} with {MessageCount} messages", sessionId, transcript.Count);
        var aiResult = await _aiAssessmentService.EvaluateConversationAsync(aiRequest, cancellationToken);

        var grammarScore = Math.Clamp(aiResult.GrammarScore, 0m, 100m);
        var vocabularyScore = Math.Clamp(aiResult.VocabularyScore, 0m, 100m);
        var fluencyScore = Math.Clamp(aiResult.FluencyScore, 0m, 100m);
        decimal? pronunciationScore = aiResult.PronunciationScore.HasValue
            ? Math.Clamp(aiResult.PronunciationScore.Value, 0m, 100m)
            : null;

        // Authoritative score formula (SRS FR-018 / Section 9):
        // Overall score is the rounded arithmetic average of validated component scores
        decimal overallScore;
        if (pronunciationScore.HasValue)
        {
            overallScore = Math.Round((grammarScore + vocabularyScore + fluencyScore + pronunciationScore.Value) / 4.0m, 1);
        }
        else
        {
            overallScore = Math.Round((grammarScore + vocabularyScore + fluencyScore) / 3.0m, 1);
        }

        session.OverallScore = overallScore;

        var assessment = new Assessment
        {
            SessionId = session.Id,
            GrammarScore = grammarScore,
            VocabularyScore = vocabularyScore,
            FluencyScore = fluencyScore,
            PronunciationScore = pronunciationScore,
            OverallScore = overallScore,
            Feedback = !string.IsNullOrWhiteSpace(aiResult.Feedback)
                ? aiResult.Feedback.Trim()
                : "Great practice session! Continue speaking regularly to expand your vocabulary and accuracy.",
            StrengthsJson = JsonSerializer.Serialize(aiResult.Strengths ?? new List<string>(), JsonOptions),
            AreasForImprovementJson = JsonSerializer.Serialize(aiResult.AreasForImprovement ?? new List<string>(), JsonOptions),
            GrammarCorrectionsJson = JsonSerializer.Serialize(aiResult.GrammarCorrections ?? new List<GrammarCorrectionDto>(), JsonOptions),
            VocabularySuggestionsJson = JsonSerializer.Serialize(aiResult.VocabularySuggestions ?? new List<VocabularySuggestionDto>(), JsonOptions),
            CreatedAt = DateTime.UtcNow
        };

        await _assessmentRepository.AddAsync(assessment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assessment successfully saved for session {SessionId} with overall score {OverallScore}", sessionId, overallScore);

        return MapToDto(assessment, session, aiResult.Recommendations);
    }

    public async Task<AssessmentResponseDto> GetAssessmentBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new EntityNotFoundException(nameof(ConversationSession), sessionId);
        }

        if (session.UserId != currentUserId)
        {
            throw new UnauthorizedSessionAccessException(sessionId);
        }

        var assessment = await _assessmentRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (assessment == null)
        {
            throw new EntityNotFoundException(nameof(Assessment), sessionId);
        }

        return MapToDto(assessment, session);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new InvalidCredentialsException("User is not authenticated.");
        }

        return _currentUserService.UserId.Value;
    }

    private static AssessmentResponseDto MapToDto(Assessment assessment, ConversationSession session, List<string>? recommendations = null)
    {
        var strengths = DeserializeJsonList<string>(assessment.StrengthsJson);
        var areas = DeserializeJsonList<string>(assessment.AreasForImprovementJson);
        var grammarCorrections = DeserializeJsonList<GrammarCorrectionDto>(assessment.GrammarCorrectionsJson);
        var vocabSuggestions = DeserializeJsonList<VocabularySuggestionDto>(assessment.VocabularySuggestionsJson);

        var recList = recommendations ?? new List<string>
        {
            "Practice asking follow-up questions to expand conversational turns.",
            "Incorporate richer descriptive adjectives and specific vocabulary."
        };

        return new AssessmentResponseDto(
            assessment.Id,
            assessment.SessionId,
            session.Topic?.Name ?? "English Speaking Practice",
            session.EnglishLevel.ToString(),
            assessment.OverallScore,
            assessment.GrammarScore,
            assessment.VocabularyScore,
            assessment.FluencyScore,
            assessment.PronunciationScore,
            assessment.Feedback,
            strengths,
            areas,
            grammarCorrections,
            vocabSuggestions,
            recList,
            assessment.CreatedAt
        );
    }

    private static List<T> DeserializeJsonList<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<T>();
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }
}
