namespace EnglishLearning.Application.DTOs;

public record AssessmentQuestionDto(
    string Id,
    string SkillCategory, // Grammar, Vocabulary, SentenceConstruction, Reading, Speaking
    string Prompt,
    IReadOnlyList<string> Options,
    int CorrectOptionIndex,
    string? Passage = null,
    string Difficulty = "Standard", // Basic, Standard, Advanced
    string? SpeechTargetPhrase = null
);

public record QuestionAnswerDto(
    string QuestionId,
    int? SelectedOptionIndex,
    string? TextInput,
    int TimeSpentSeconds
);

public record SubmitAssessmentRequest(
    IReadOnlyList<QuestionAnswerDto> Answers,
    string? SpeechAudioBase64 = null,
    string? SpeechAudioMimeType = null
);

public record LearnerAssessmentResultDto(
    Guid Id,
    string Category, // Basic, Standard, Advanced
    string CefrLevel, // A1, A2, B1, B2, C1, C2
    decimal OverallScore,
    decimal GrammarScore,
    decimal VocabularyScore,
    decimal SentenceScore,
    decimal ReadingScore,
    decimal? SpeakingScore,
    bool IsSpeakingAssessed,
    IReadOnlyList<string> Weaknesses,
    string Confidence,
    bool ReassessmentRecommended,
    DateTime CompletedAt
);

public record PhaseTaskDto(
    string Id,
    string Type, // multiple_choice, sentence_builder, fill_in_the_blank, prompt
    string Instruction,
    string Question,
    IReadOnlyList<string>? Options,
    string? CorrectAnswer,
    string? Explanation
);

public record LessonContentDto(
    string RuleTitle,
    string Summary,
    string Formula,
    IReadOnlyList<string> KeyRules,
    IReadOnlyList<string> Examples,
    IReadOnlyList<string> CommonMistakes
);

public record StudyPlanPhaseDto(
    Guid Id,
    int PhaseOrder, // 1 to 4
    string PhaseType, // GrammarStudy, SentenceMaking, Practice, AiSpeaking
    string ObjectiveTitle,
    int PlannedDurationMinutes,
    int ActualDurationMinutes,
    string Status, // NotStarted, InProgress, Completed, NeedsReview
    decimal? Score,
    LessonContentDto? LessonContent,
    IReadOnlyList<PhaseTaskDto> Tasks,
    string? Feedback,
    bool IsLocked
);

public record DailyStudyPlanDto(
    Guid Id,
    string PlanDate,
    string ObjectiveCode,
    string ObjectiveTitle,
    string Difficulty,
    string CefrLevel,
    int TargetMinutes,
    int ActualMinutes,
    string Status,
    decimal? OverallScore,
    IReadOnlyList<StudyPlanPhaseDto> Phases,
    bool IsIdempotentCached
);

public record UpdatePhaseProgressRequest(
    int ActiveDurationSecondsToAdd
);

public record CompletePhaseRequest(
    Dictionary<string, string> TaskAnswers,
    int ActiveDurationSeconds
);

public record PhaseCompletionResultDto(
    Guid PhaseId,
    decimal Score,
    string Status,
    bool IsMastered,
    string Feedback,
    bool NextPhaseUnlocked,
    string OverallPlanStatus
);

public record LearningPlanHistoryDto(
    IReadOnlyList<DailyStudyPlanDto> RecentPlans,
    int MasteredObjectivesCount,
    int TotalActiveMinutes,
    IReadOnlyList<LearnerSkillMasteryItemDto> MasteryBreakdown,
    IReadOnlyList<string> RecommendedRevisions
);

public record LearnerSkillMasteryItemDto(
    string ObjectiveCode,
    decimal MasteryScore,
    int AttemptCount,
    bool IsMastered,
    DateTime LastPracticedAt,
    DateTime? NextRevisionDueDate
);

// Option 3: Bilingual Translation DTOs
public record TranslateRequest(
    string Text,
    string? SourceLanguage = "Auto",
    string? TargetStyle = "Conversational"
);

public record TranslateResultDto(
    string OriginalText,
    string NaturalEnglish,
    string FormalEnglish,
    string Explanation,
    IReadOnlyList<VocabularyPairDto> KeyVocabulary,
    string PronunciationTip
);

public record VocabularyPairDto(
    string Word,
    string Meaning
);

// Option 4: Document / PDF Analysis DTOs
public record DocumentAnalysisRequest(
    string DocumentTitle,
    string DocumentText
);

public record DocumentAnalysisResultDto(
    string Title,
    string Summary,
    string CefrDifficulty,
    IReadOnlyList<DocumentVocabItemDto> KeyVocabulary,
    IReadOnlyList<string> DiscussionQuestions,
    string StarterPrompt
);

public record DocumentVocabItemDto(
    string Word,
    string Meaning,
    string ExampleUsage
);
