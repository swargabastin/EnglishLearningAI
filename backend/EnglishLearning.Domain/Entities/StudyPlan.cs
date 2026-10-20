using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class StudyPlan : BaseEntity
{
    public Guid UserId { get; set; }
    public string PlanDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public Guid ObjectiveId { get; set; }
    public string ObjectiveCode { get; set; } = string.Empty;
    public int TargetMinutes { get; set; } = 20;
    public int ActualMinutes { get; set; }
    public StudyPlanStatus Status { get; set; } = StudyPlanStatus.NotStarted;
    public decimal? OverallScore { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public virtual User? User { get; set; }
    public virtual LearningObjective? Objective { get; set; }
    public virtual ICollection<StudyPlanPhase> Phases { get; set; } = new List<StudyPlanPhase>();
}
