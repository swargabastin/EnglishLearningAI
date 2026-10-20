using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class ConversationMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public SpeakerRole Role { get; set; } = SpeakerRole.User;
    public string MessageText { get; set; } = string.Empty;
    public string? AudioStoragePath { get; set; }
    public int SequenceNumber { get; set; } = 1;

    // Navigation Properties
    public ConversationSession Session { get; set; } = null!;
}
