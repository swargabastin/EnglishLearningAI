using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Interfaces.Repositories;

public interface IAdaptiveLearningRepository
{
    // Assessments
    Task<LearnerAssessment?> GetLatestAssessmentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAssessmentAsync(LearnerAssessment assessment, CancellationToken cancellationToken = default);

    // Objectives
    Task<IReadOnlyList<LearningObjective>> GetAllActiveObjectivesAsync(CancellationToken cancellationToken = default);
    Task<LearningObjective?> GetObjectiveByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<LearningObjective?> GetObjectiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddObjectiveAsync(LearningObjective objective, CancellationToken cancellationToken = default);

    // Study Plans
    Task<StudyPlan?> GetStudyPlanByDateAsync(Guid userId, string dateString, CancellationToken cancellationToken = default);
    Task<StudyPlan?> GetStudyPlanWithPhasesAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudyPlan>> GetStudyPlanHistoryAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default);
    Task AddStudyPlanAsync(StudyPlan plan, CancellationToken cancellationToken = default);
    void UpdateStudyPlan(StudyPlan plan);

    // Phases
    Task<StudyPlanPhase?> GetPhaseByIdAsync(Guid phaseId, CancellationToken cancellationToken = default);
    void UpdatePhase(StudyPlanPhase phase);

    // Mastery
    Task<LearnerSkillMastery?> GetSkillMasteryAsync(Guid userId, string objectiveCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LearnerSkillMastery>> GetAllSkillMasteriesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpsertSkillMasteryAsync(LearnerSkillMastery mastery, CancellationToken cancellationToken = default);

    // Activity History
    Task AddActivityHistoryAsync(LearningActivityHistory history, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LearningActivityHistory>> GetRecentActivitiesAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);
}
