using EnglishLearning.Domain.Common;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public EnglishProficiencyLevel EnglishLevel { get; set; } = EnglishProficiencyLevel.B1;
    public LearningGoal LearningGoal { get; set; } = LearningGoal.SpeakingFluency;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<ConversationSession> ConversationSessions { get; set; } = new List<ConversationSession>();
    public ICollection<Vocabulary> VocabularyItems { get; set; } = new List<Vocabulary>();
}
