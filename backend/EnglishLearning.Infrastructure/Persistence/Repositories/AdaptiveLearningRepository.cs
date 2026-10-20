using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence.Repositories;

public class AdaptiveLearningRepository : IAdaptiveLearningRepository
{
    private readonly AppDbContext _context;

    public AdaptiveLearningRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LearnerAssessment?> GetLatestAssessmentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.LearnerAssessments
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAssessmentAsync(LearnerAssessment assessment, CancellationToken cancellationToken = default)
    {
        await _context.LearnerAssessments.AddAsync(assessment, cancellationToken);
    }

    public async Task<IReadOnlyList<LearningObjective>> GetAllActiveObjectivesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LearningObjectives
            .Where(o => o.IsActive)
            .OrderBy(o => o.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<LearningObjective?> GetObjectiveByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.LearningObjectives
            .FirstOrDefaultAsync(o => o.Code == code, cancellationToken);
    }

    public async Task<LearningObjective?> GetObjectiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LearningObjectives
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddObjectiveAsync(LearningObjective objective, CancellationToken cancellationToken = default)
    {
        await _context.LearningObjectives.AddAsync(objective, cancellationToken);
    }

    public async Task<StudyPlan?> GetStudyPlanByDateAsync(Guid userId, string dateString, CancellationToken cancellationToken = default)
    {
        return await _context.StudyPlans
            .Include(p => p.Phases.OrderBy(ph => ph.PhaseOrder))
            .Include(p => p.Objective)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PlanDate == dateString, cancellationToken);
    }

    public async Task<StudyPlan?> GetStudyPlanWithPhasesAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        return await _context.StudyPlans
            .Include(p => p.Phases.OrderBy(ph => ph.PhaseOrder))
            .Include(p => p.Objective)
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudyPlan>> GetStudyPlanHistoryAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.StudyPlans
            .Include(p => p.Phases.OrderBy(ph => ph.PhaseOrder))
            .Include(p => p.Objective)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PlanDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddStudyPlanAsync(StudyPlan plan, CancellationToken cancellationToken = default)
    {
        await _context.StudyPlans.AddAsync(plan, cancellationToken);
    }

    public void UpdateStudyPlan(StudyPlan plan)
    {
        _context.StudyPlans.Update(plan);
    }

    public async Task<StudyPlanPhase?> GetPhaseByIdAsync(Guid phaseId, CancellationToken cancellationToken = default)
    {
        return await _context.StudyPlanPhases
            .Include(ph => ph.StudyPlan)
                .ThenInclude(p => p!.Phases)
            .FirstOrDefaultAsync(ph => ph.Id == phaseId, cancellationToken);
    }

    public void UpdatePhase(StudyPlanPhase phase)
    {
        _context.StudyPlanPhases.Update(phase);
    }

    public async Task<LearnerSkillMastery?> GetSkillMasteryAsync(Guid userId, string objectiveCode, CancellationToken cancellationToken = default)
    {
        return await _context.LearnerSkillMasteries
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ObjectiveCode == objectiveCode, cancellationToken);
    }

    public async Task<IReadOnlyList<LearnerSkillMastery>> GetAllSkillMasteriesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.LearnerSkillMasteries
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertSkillMasteryAsync(LearnerSkillMastery mastery, CancellationToken cancellationToken = default)
    {
        var existing = await _context.LearnerSkillMasteries
            .FirstOrDefaultAsync(m => m.UserId == mastery.UserId && m.ObjectiveCode == mastery.ObjectiveCode, cancellationToken);

        if (existing == null)
        {
            await _context.LearnerSkillMasteries.AddAsync(mastery, cancellationToken);
        }
        else
        {
            existing.MasteryScore = mastery.MasteryScore;
            existing.AttemptCount = mastery.AttemptCount;
            existing.IsMastered = mastery.IsMastered;
            existing.LastPracticedAt = mastery.LastPracticedAt;
            existing.NextRevisionDueDate = mastery.NextRevisionDueDate;
            _context.LearnerSkillMasteries.Update(existing);
        }
    }

    public async Task AddActivityHistoryAsync(LearningActivityHistory history, CancellationToken cancellationToken = default)
    {
        await _context.LearningActivityHistories.AddAsync(history, cancellationToken);
    }

    public async Task<IReadOnlyList<LearningActivityHistory>> GetRecentActivitiesAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.LearningActivityHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
