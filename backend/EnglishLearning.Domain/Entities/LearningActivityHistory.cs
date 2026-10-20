using EnglishLearning.Domain.Common;

namespace EnglishLearning.Domain.Entities;

public class LearningActivityHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid StudyPlanId { get; set; }
    public Guid PhaseId { get; set; }
    public string TaskIdentifier { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public int DurationSeconds { get; set; }
    public string AnswerPayloadJson { get; set; } = "{}";
    public bool IsCorrect { get; set; }

    // Navigation
    public virtual User? User { get; set; }
}
