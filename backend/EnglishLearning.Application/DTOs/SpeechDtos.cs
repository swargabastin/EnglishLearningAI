namespace EnglishLearning.Application.DTOs;

public record SpeechConversationResponseDto(
    ConversationSessionDto Session,
    ConversationMessageDto UserMessage,
    ConversationMessageDto AiMessage,
    string RecognizedText,
    string? AudioBase64,
    string AudioContentType = "audio/mpeg",
    string? PedagogicalTip = null
);
