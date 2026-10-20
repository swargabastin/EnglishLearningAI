using EnglishLearning.Domain.Common;

namespace EnglishLearning.Domain.Entities;

public class Assessment : BaseEntity
{
    public Guid SessionId { get; set; }
    public decimal GrammarScore { get; set; }
    public decimal VocabularyScore { get; set; }
    public decimal FluencyScore { get; set; }
    public decimal? PronunciationScore { get; set; }
    public decimal OverallScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string StrengthsJson { get; set; } = "[]";
    public string AreasForImprovementJson { get; set; } = "[]";
    public string GrammarCorrectionsJson { get; set; } = "[]";
    public string VocabularySuggestionsJson { get; set; } = "[]";

    // Navigation Properties
    public ConversationSession Session { get; set; } = null!;
}
