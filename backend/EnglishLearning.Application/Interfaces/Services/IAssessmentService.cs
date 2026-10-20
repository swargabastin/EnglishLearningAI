using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface IAssessmentService
{
    Task<AssessmentResponseDto> GenerateAssessmentAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default
    );

    Task<AssessmentResponseDto> GetAssessmentBySessionIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default
    );
}
