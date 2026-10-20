using EnglishLearning.Domain.Common;

namespace EnglishLearning.Domain.Entities;

public class LearnerSkillMastery : BaseEntity
{
    public Guid UserId { get; set; }
    public string ObjectiveCode { get; set; } = string.Empty;
    public decimal MasteryScore { get; set; }
    public int AttemptCount { get; set; } = 1;
    public bool IsMastered { get; set; }
    public DateTime LastPracticedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextRevisionDueDate { get; set; }

    // Navigation
    public virtual User? User { get; set; }
}
