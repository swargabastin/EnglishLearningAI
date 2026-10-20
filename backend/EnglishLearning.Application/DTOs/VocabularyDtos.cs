namespace EnglishLearning.Application.DTOs;

public record VocabularyItemDto(
    Guid Id,
    string Word,
    string Meaning,
    string Example,
    int MasteryLevel,
    Guid? ContextSessionId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AddVocabularyRequest(
    string Word,
    string Meaning,
    string Example,
    Guid? ContextSessionId = null
);

public record UpdateMasteryRequest(
    int MasteryLevel
);
