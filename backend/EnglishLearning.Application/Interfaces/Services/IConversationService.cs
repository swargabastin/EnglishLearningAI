using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface IConversationService
{
    Task<ConversationSessionDto> StartConversationAsync(StartConversationRequest request, CancellationToken cancellationToken = default);
    Task<SendMessageResponseDto> SendMessageAsync(Guid sessionId, SendMessageRequest request, CancellationToken cancellationToken = default);
    Task<ConversationSessionDto> GetConversationByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<ConversationSessionDto> EndConversationAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
