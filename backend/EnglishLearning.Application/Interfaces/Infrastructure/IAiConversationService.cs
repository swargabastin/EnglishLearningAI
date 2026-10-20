using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Infrastructure;

public interface IAiConversationService
{
    Task<AiConversationResponse> GenerateResponseAsync(AiConversationRequest request, CancellationToken cancellationToken = default);
}
