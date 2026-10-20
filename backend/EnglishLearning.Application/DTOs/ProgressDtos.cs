namespace EnglishLearning.Application.DTOs;

public record UserProfileSummaryDto(
    Guid Id,
    string Name,
    string Email,
    string EnglishLevel,
    string LearningGoal,
    int TotalPracticeMinutes,
    int TotalSessionsCompleted
);

public record ProgressStatsDto(
    int TotalCompletedSessions,
    int TotalPracticeMinutes,
    decimal AverageOverallScore,
    decimal LatestScore,
    decimal GrammarAverage,
    decimal VocabularyAverage,
    decimal FluencyAverage,
    string AssessedLevel
);

public record RecentSessionSummaryDto(
    Guid SessionId,
    Guid TopicId,
    string TopicName,
    string EnglishLevel,
    DateTime StartedAt,
    int DurationMinutes,
    decimal OverallScore,
    bool HasAssessment
);

public record ProgressChartPointDto(
    string Date,
    decimal OverallScore,
    decimal GrammarScore,
    decimal VocabularyScore,
    decimal FluencyScore,
    string TopicName
);

public record DashboardResponseDto(
    UserProfileSummaryDto User,
    ProgressStatsDto Stats,
    IReadOnlyList<RecentSessionSummaryDto> RecentSessions,
    IReadOnlyList<ProgressChartPointDto> ScoreProgression
);

public record HistoryItemDto(
    Guid SessionId,
    Guid TopicId,
    string TopicName,
    string EnglishLevel,
    DateTime StartedAt,
    DateTime? EndedAt,
    int DurationMinutes,
    decimal OverallScore,
    decimal GrammarScore,
    decimal VocabularyScore,
    decimal FluencyScore,
    int MessageCount,
    bool HasAssessment,
    Guid? AssessmentId
);

public record PaginatedListDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
)
{
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public record SessionDetailDto(
    Guid SessionId,
    Guid TopicId,
    string TopicName,
    string TopicDescription,
    string EnglishLevel,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    int DurationSeconds,
    decimal? OverallScore,
    IReadOnlyList<ConversationMessageDto> Messages,
    AssessmentResponseDto? Assessment
);
