using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Infrastructure;

public interface IAiAssessmentService
{
    Task<AssessmentAiResponse> EvaluateConversationAsync(
        AssessmentAiRequest request,
        CancellationToken cancellationToken = default
    );
}
