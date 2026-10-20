namespace EnglishLearning.Application.DTOs;

public record GrammarCorrectionDto(
    string Original,
    string Corrected,
    string Explanation,
    string? RuleCategory = null
);

public record VocabularySuggestionDto(
    string OriginalWord,
    string SuggestedAlternative,
    string Context,
    string? CefrLevel = null
);

public record AssessmentAiRequest(
    string TopicName,
    string TopicDescription,
    string TargetCefrLevel,
    IReadOnlyList<AiMessageContext> Transcript
);

public record AssessmentAiResponse(
    decimal GrammarScore,
    decimal VocabularyScore,
    decimal FluencyScore,
    decimal? PronunciationScore,
    string Feedback,
    List<string> Strengths,
    List<string> AreasForImprovement,
    List<GrammarCorrectionDto> GrammarCorrections,
    List<VocabularySuggestionDto> VocabularySuggestions,
    List<string> Recommendations
);

public record AssessmentResponseDto(
    Guid Id,
    Guid SessionId,
    string TopicName,
    string EnglishLevel,
    decimal OverallScore,
    decimal GrammarScore,
    decimal VocabularyScore,
    decimal FluencyScore,
    decimal? PronunciationScore,
    string Feedback,
    List<string> Strengths,
    List<string> AreasForImprovement,
    List<GrammarCorrectionDto> GrammarCorrections,
    List<VocabularySuggestionDto> VocabularySuggestions,
    List<string> Recommendations,
    DateTime CreatedAt
);
