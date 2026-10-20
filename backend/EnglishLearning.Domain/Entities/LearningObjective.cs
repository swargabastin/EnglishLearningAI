using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class LearningObjective : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SkillCategory SkillCategory { get; set; } = SkillCategory.Grammar;
    public LearnerCategory Difficulty { get; set; } = LearnerCategory.Basic;
    public EnglishProficiencyLevel CefrLevel { get; set; } = EnglishProficiencyLevel.A2;
    public string PrerequisiteCodesJson { get; set; } = "[]";
    public decimal MasteryScoreThreshold { get; set; } = 75.0m;
    public int SuggestedDurationMinutes { get; set; } = 15;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
