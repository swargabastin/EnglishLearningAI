namespace EnglishLearning.Application.DTOs;

public record SpeechConversationResponseDto(
    ConversationSessionDto Session,
    ConversationMessageDto UserMessage,
    ConversationMessageDto AiMessage,
    string RecognizedText,
    string? AudioBase64,
    string AudioContentType = "audio/mpeg",
    string? PedagogicalTip = null,
    PronunciationAssessmentDto? Pronunciation = null
);

public record PronunciationScoreRequest(
    string SpokenText,
    string? TargetPhrase = null
);

public record PronunciationAssessmentDto(
    decimal OverallScore,
    decimal AccuracyScore,
    decimal FluencyScore,
    decimal CompletenessScore,
    IReadOnlyList<PronunciationWordScoreDto> Words,
    string Feedback
);

public record PronunciationWordScoreDto(
    string Word,
    decimal Score,
    string Status, // Good, Acceptable, NeedsPractice
    IReadOnlyList<PronunciationPhonemeScoreDto> Phonemes
);

public record PronunciationPhonemeScoreDto(
    string Phoneme,
    decimal Score,
    string ColorHex
);
