using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class Topic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EnglishProficiencyLevel RecommendedDifficulty { get; set; } = EnglishProficiencyLevel.B1;
    public string IconKey { get; set; } = "MessageCircle";
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<ConversationSession> ConversationSessions { get; set; } = new List<ConversationSession>();
}
