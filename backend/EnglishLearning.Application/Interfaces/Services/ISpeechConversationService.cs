using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface ISpeechConversationService
{
    Task<SpeechConversationResponseDto> ProcessSpeechAsync(
        Guid sessionId,
        Stream audioStream,
        string fileName,
        string contentType,
        long fileLength,
        CancellationToken cancellationToken = default
    );
}
