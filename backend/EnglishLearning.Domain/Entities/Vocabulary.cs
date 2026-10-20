using EnglishLearning.Domain.Common;

namespace EnglishLearning.Domain.Entities;

public class Vocabulary : BaseEntity
{
    public Guid UserId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
    public int MasteryLevel { get; set; } = 1;
    public Guid? ContextSessionId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public ConversationSession? ContextSession { get; set; }
}
