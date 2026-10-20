namespace EnglishLearning.Application.DTOs;

public record TopicDto(
    Guid Id,
    string Name,
    string Description,
    string Difficulty,
    string IconKey,
    bool IsActive,
    int SortOrder
);

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string EnglishLevel,
    string LearningGoal,
    DateTime CreatedAt
);

public record ConversationMessageDto(
    Guid Id,
    Guid SessionId,
    string Role,
    string MessageText,
    string? AudioStoragePath,
    int SequenceNumber,
    DateTime CreatedAt
);

public record ConversationSessionDto(
    Guid Id,
    Guid UserId,
    Guid TopicId,
    string TopicName,
    string EnglishLevel,
    DateTime StartedAt,
    DateTime? EndedAt,
    int DurationSeconds,
    decimal? OverallScore,
    string Status,
    IReadOnlyList<ConversationMessageDto>? Messages = null
);

public record AssessmentDto(
    Guid Id,
    Guid SessionId,
    decimal GrammarScore,
    decimal VocabularyScore,
    decimal FluencyScore,
    decimal? PronunciationScore,
    decimal OverallScore,
    string Feedback,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> AreasForImprovement,
    IReadOnlyList<string> GrammarCorrections,
    IReadOnlyList<string> VocabularySuggestions,
    DateTime CreatedAt
);

public record VocabularyDto(
    Guid Id,
    Guid UserId,
    string Word,
    string Meaning,
    string Example,
    int MasteryLevel,
    DateTime CreatedAt
);
