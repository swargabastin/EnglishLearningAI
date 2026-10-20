using System.Text.Json;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Application.Services;

public class ProgressService : IProgressService
{
    private readonly IUserRepository _userRepository;
    private readonly IConversationSessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProgressService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProgressService(
        IUserRepository userRepository,
        IConversationSessionRepository sessionRepository,
        ICurrentUserService currentUserService,
        ILogger<ProgressService> logger)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<DashboardResponseDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException(nameof(User), currentUserId);
        }

        var completedSessions = await _sessionRepository.GetCompletedSessionsWithAssessmentsByUserIdAsync(
            currentUserId,
            limit: 50,
            cancellationToken
        );

        var totalSessions = completedSessions.Count;
        var totalMinutes = (int)Math.Ceiling(completedSessions.Sum(s => s.DurationSeconds) / 60.0);

        var assessedSessions = completedSessions
            .Where(s => s.Assessment != null)
            .ToList();

        decimal avgOverall = 0m;
        decimal latestScore = 0m;
        decimal grammarAvg = 0m;
        decimal vocabAvg = 0m;
        decimal fluencyAvg = 0m;
        var assessedLevel = user.EnglishLevel.ToString();

        if (assessedSessions.Count > 0)
        {
            avgOverall = Math.Round(assessedSessions.Average(s => s.Assessment!.OverallScore), 1);
            latestScore = assessedSessions[0].Assessment!.OverallScore;
            grammarAvg = Math.Round(assessedSessions.Average(s => s.Assessment!.GrammarScore), 1);
            vocabAvg = Math.Round(assessedSessions.Average(s => s.Assessment!.VocabularyScore), 1);
            fluencyAvg = Math.Round(assessedSessions.Average(s => s.Assessment!.FluencyScore), 1);
            assessedLevel = assessedSessions[0].EnglishLevel.ToString();
        }

        var userSummary = new UserProfileSummaryDto(
            user.Id,
            user.Name,
            user.Email,
            user.EnglishLevel.ToString(),
            user.LearningGoal.ToString(),
            totalMinutes,
            totalSessions
        );

        var stats = new ProgressStatsDto(
            totalSessions,
            totalMinutes,
            avgOverall,
            latestScore,
            grammarAvg,
            vocabAvg,
            fluencyAvg,
            assessedLevel
        );

        var recentList = completedSessions
            .Take(5)
            .Select(s => new RecentSessionSummaryDto(
                s.Id,
                s.TopicId,
                s.Topic?.Name ?? "English Practice",
                s.EnglishLevel.ToString(),
                s.StartedAt,
                Math.Max(1, (int)Math.Ceiling(s.DurationSeconds / 60.0)),
                s.Assessment?.OverallScore ?? s.OverallScore ?? 0m,
                s.Assessment != null
            ))
            .ToList();

        var progressionPoints = assessedSessions
            .Take(10)
            .OrderBy(s => s.StartedAt)
            .Select(s => new ProgressChartPointDto(
                s.StartedAt.ToString("yyyy-MM-dd"),
                s.Assessment!.OverallScore,
                s.Assessment.GrammarScore,
                s.Assessment.VocabularyScore,
                s.Assessment.FluencyScore,
                s.Topic?.Name ?? "Practice"
            ))
            .ToList();

        return new DashboardResponseDto(userSummary, stats, recentList, progressionPoints);
    }

    public async Task<PaginatedListDto<HistoryItemDto>> GetHistoryAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await _sessionRepository.GetPaginatedHistoryByUserIdAsync(
            currentUserId,
            page,
            pageSize,
            cancellationToken
        );

        var historyDtos = items.Select(s => new HistoryItemDto(
            s.Id,
            s.TopicId,
            s.Topic?.Name ?? "English Practice",
            s.EnglishLevel.ToString(),
            s.StartedAt,
            s.EndedAt,
            Math.Max(1, (int)Math.Ceiling(s.DurationSeconds / 60.0)),
            s.Assessment?.OverallScore ?? s.OverallScore ?? 0m,
            s.Assessment?.GrammarScore ?? 0m,
            s.Assessment?.VocabularyScore ?? 0m,
            s.Assessment?.FluencyScore ?? 0m,
            s.Messages.Count,
            s.Assessment != null,
            s.Assessment?.Id
        )).ToList();

        var totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;

        return new PaginatedListDto<HistoryItemDto>(
            historyDtos,
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<SessionDetailDto> GetSessionDetailAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
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

        var messageDtos = session.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new ConversationMessageDto(
                m.Id,
                m.SessionId,
                m.Role == SpeakerRole.User ? "user" : "assistant",
                m.MessageText,
                m.AudioStoragePath,
                m.SequenceNumber,
                m.CreatedAt
            ))
            .ToList();

        AssessmentResponseDto? assessmentDto = null;
        if (session.Assessment != null)
        {
            assessmentDto = MapAssessmentToDto(session.Assessment, session);
        }

        return new SessionDetailDto(
            session.Id,
            session.TopicId,
            session.Topic?.Name ?? "English Practice",
            session.Topic?.Description ?? string.Empty,
            session.EnglishLevel.ToString(),
            session.Status.ToString(),
            session.StartedAt,
            session.EndedAt,
            session.DurationSeconds,
            session.OverallScore,
            messageDtos,
            assessmentDto
        );
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new InvalidCredentialsException("User is not authenticated.");
        }

        return _currentUserService.UserId.Value;
    }

    private static AssessmentResponseDto MapAssessmentToDto(Assessment assessment, ConversationSession session)
    {
        var strengths = DeserializeJsonList<string>(assessment.StrengthsJson);
        var areas = DeserializeJsonList<string>(assessment.AreasForImprovementJson);
        var grammarCorrections = DeserializeJsonList<GrammarCorrectionDto>(assessment.GrammarCorrectionsJson);
        var vocabSuggestions = DeserializeJsonList<VocabularySuggestionDto>(assessment.VocabularySuggestionsJson);

        var recommendations = new List<string>
        {
            "Practice conversational turns daily",
            "Review suggested grammar structures"
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
            recommendations,
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
