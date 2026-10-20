using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class StudyPlanPhase : BaseEntity
{
    public Guid StudyPlanId { get; set; }
    public int PhaseOrder { get; set; } // 1, 2, 3, 4
    public PhaseType PhaseType { get; set; } = PhaseType.GrammarStudy;
    public string ObjectiveTitle { get; set; } = string.Empty;
    public string LessonContentJson { get; set; } = "{}";
    public string TasksJson { get; set; } = "[]";
    public int PlannedDurationMinutes { get; set; } = 5;
    public int ActualDurationMinutes { get; set; }
    public int Attempts { get; set; }
    public decimal? Score { get; set; }
    public StudyPlanStatus Status { get; set; } = StudyPlanStatus.NotStarted;
    public string? Feedback { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public virtual StudyPlan? StudyPlan { get; set; }
}
