using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class LearnerAssessment : BaseEntity
{
    public Guid UserId { get; set; }
    public LearnerCategory Category { get; set; } = LearnerCategory.Standard;
    public EnglishProficiencyLevel CefrLevel { get; set; } = EnglishProficiencyLevel.B1;
    public decimal GrammarScore { get; set; }
    public decimal VocabularyScore { get; set; }
    public decimal SentenceScore { get; set; }
    public decimal ReadingScore { get; set; }
    public decimal? SpeakingScore { get; set; }
    public decimal OverallScore { get; set; }
    public string Confidence { get; set; } = "High";
    public string ResponsesJson { get; set; } = "{}";
    public string WeaknessesJson { get; set; } = "[]";
    public bool ReassessmentRecommended { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User? User { get; set; }
}
