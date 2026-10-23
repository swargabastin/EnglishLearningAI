using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface IAdaptiveLearningService
{
    Task<IReadOnlyList<AssessmentQuestionDto>> GetDiagnosticQuestionsAsync(CancellationToken cancellationToken = default);
    Task<LearnerAssessmentResultDto> SubmitAssessmentAsync(SubmitAssessmentRequest request, CancellationToken cancellationToken = default);
    Task<LearnerAssessmentResultDto?> GetLatestAssessmentAsync(CancellationToken cancellationToken = default);

    Task<DailyStudyPlanDto> GetOrCreateTodayPlanAsync(string? explicitDate = null, bool forceRegenerate = false, string? targetObjectiveCode = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LearningObjectiveDto>> GetAllObjectivesAsync(CancellationToken cancellationToken = default);
    Task<PhaseCompletionResultDto> UpdatePhaseProgressAsync(Guid phaseId, UpdatePhaseProgressRequest request, CancellationToken cancellationToken = default);
    Task<PhaseCompletionResultDto> CompletePhaseAsync(Guid phaseId, CompletePhaseRequest request, CancellationToken cancellationToken = default);

    Task<LearningPlanHistoryDto> GetLearningHistoryAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task EnsureCurriculumSeededAsync(CancellationToken cancellationToken = default);

    // Option 3: Bilingual Translation
    Task<TranslateResultDto> TranslateToEnglishAsync(TranslateRequest request, CancellationToken cancellationToken = default);

    // Option 4: Document Analysis
    Task<DocumentAnalysisResultDto> AnalyzeDocumentAsync(DocumentAnalysisRequest request, CancellationToken cancellationToken = default);
}
