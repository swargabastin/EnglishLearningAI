using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class ConversationSession : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TopicId { get; set; }
    public EnglishProficiencyLevel EnglishLevel { get; set; } = EnglishProficiencyLevel.B1;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int DurationSeconds { get; set; } = 0;
    public decimal? OverallScore { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
    public Assessment? Assessment { get; set; }
}
