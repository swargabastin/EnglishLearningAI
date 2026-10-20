namespace EnglishLearning.Application.DTOs;

public record StartConversationRequest(
    Guid TopicId,
    string? EnglishLevel = null,
    string? TargetObjective = null
);

public record SendMessageRequest(
    string Message
);

public record SendMessageResponseDto(
    ConversationSessionDto Session,
    ConversationMessageDto UserMessage,
    ConversationMessageDto AiMessage,
    string? PedagogicalTip = null
);

public record AiMessageContext(
    string Role,
    string Content
);

public record AiConversationRequest(
    string TopicName,
    string TopicDescription,
    string EnglishLevel,
    IReadOnlyList<AiMessageContext> Messages,
    string? TargetObjective = null
);

public record AiConversationResponse(
    string Message,
    string? PedagogicalTip = null
);
