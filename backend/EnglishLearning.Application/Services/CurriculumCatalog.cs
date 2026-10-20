using EnglishLearning.Application.DTOs;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Services;

public static class CurriculumCatalog
{
    public static readonly IReadOnlyList<LearningObjective> MasterObjectives = new List<LearningObjective>
    {
        // Basic (A1/A2)
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Code = "OBJ_B_PRES_SIMPLE",
            Title = "Present Simple: Habits, Routines & Facts",
            Description = "Master affirmative, negative, and question forms using do/does and third-person singular -s rules.",
            SkillCategory = SkillCategory.Grammar,
            Difficulty = LearnerCategory.Basic,
            CefrLevel = EnglishProficiencyLevel.A2,
            PrerequisiteCodesJson = "[]",
            MasteryScoreThreshold = 75.0m,
            SuggestedDurationMinutes = 15,
            SortOrder = 1,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            Code = "OBJ_B_PAST_SIMPLE",
            Title = "Past Simple: Completed Events & Regular/Irregular Verbs",
            Description = "Confidently narrate past experiences using regular -ed and common irregular past verbs with did/didn't.",
            SkillCategory = SkillCategory.SentenceConstruction,
            Difficulty = LearnerCategory.Basic,
            CefrLevel = EnglishProficiencyLevel.A2,
            PrerequisiteCodesJson = "[\"OBJ_B_PRES_SIMPLE\"]",
            MasteryScoreThreshold = 75.0m,
            SuggestedDurationMinutes = 15,
            SortOrder = 2,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            Code = "OBJ_B_FUTURE_PLANS",
            Title = "Future Intentions: Going To vs Will",
            Description = "Distinguish planned decisions (going to) from spontaneous offers and predictions (will).",
            SkillCategory = SkillCategory.Grammar,
            Difficulty = LearnerCategory.Basic,
            CefrLevel = EnglishProficiencyLevel.A2,
            PrerequisiteCodesJson = "[\"OBJ_B_PRES_SIMPLE\"]",
            MasteryScoreThreshold = 75.0m,
            SuggestedDurationMinutes = 15,
            SortOrder = 3,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
            Code = "OBJ_B_DAILY_VOCAB",
            Title = "Daily Life & Workplace Vocabulary Expansion",
            Description = "Build functional English vocabulary for workplace tasks, office interactions, and everyday routines.",
            SkillCategory = SkillCategory.Vocabulary,
            Difficulty = LearnerCategory.Basic,
            CefrLevel = EnglishProficiencyLevel.A2,
            PrerequisiteCodesJson = "[]",
            MasteryScoreThreshold = 75.0m,
            SuggestedDurationMinutes = 15,
            SortOrder = 4,
            IsActive = true
        },

        // Standard (B1/B2)
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            Code = "OBJ_S_PRES_PERF",
            Title = "Present Perfect vs Past Simple",
            Description = "Master life experiences, unfinished periods, and recent events using have/has + past participle with since, for, and already.",
            SkillCategory = SkillCategory.Grammar,
            Difficulty = LearnerCategory.Standard,
            CefrLevel = EnglishProficiencyLevel.B1,
            PrerequisiteCodesJson = "[\"OBJ_B_PAST_SIMPLE\"]",
            MasteryScoreThreshold = 75.0m,
            SuggestedDurationMinutes = 20,
            SortOrder = 5,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            Code = "OBJ_S_MODALS_OBLIG",
            Title = "Modals of Obligation, Advice & Possibility",
            Description = "Use should, must, have to, and could with accurate nuance in personal and professional situations.",
            SkillCategory = SkillCategory.SentenceConstruction,
            Difficulty = LearnerCategory.Standard,
            CefrLevel = EnglishProficiencyLevel.B1,
            PrerequisiteCodesJson = "[\"OBJ_B_PRES_SIMPLE\"]",
            MasteryScoreThreshold = 75.0m,
            SuggestedDurationMinutes = 20,
            SortOrder = 6,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
            Code = "OBJ_S_COND_FIRST_SEC",
            Title = "First & Second Conditionals: Real vs Hypothetical",
            Description = "Formulate real cause-and-effect conditions (If + present, will) and imaginary possibilities (If + past, would).",
            SkillCategory = SkillCategory.Grammar,
            Difficulty = LearnerCategory.Standard,
            CefrLevel = EnglishProficiencyLevel.B2,
            PrerequisiteCodesJson = "[\"OBJ_B_FUTURE_PLANS\", \"OBJ_B_PAST_SIMPLE\"]",
            MasteryScoreThreshold = 80.0m,
            SuggestedDurationMinutes = 20,
            SortOrder = 7,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
            Code = "OBJ_S_PROF_TRANSITIONS",
            Title = "Professional Connectors & Meeting Transitions",
            Description = "Use sophisticated discourse markers (Furthermore, However, Specifically, Consequently) in speaking and writing.",
            SkillCategory = SkillCategory.Vocabulary,
            Difficulty = LearnerCategory.Standard,
            CefrLevel = EnglishProficiencyLevel.B2,
            PrerequisiteCodesJson = "[\"OBJ_S_PRES_PERF\"]",
            MasteryScoreThreshold = 80.0m,
            SuggestedDurationMinutes = 20,
            SortOrder = 8,
            IsActive = true
        },

        // Advanced (C1/C2)
        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Code = "OBJ_A_COND_THIRD_MIX",
            Title = "Third & Mixed Conditionals: Past Regrets & Alternate Realities",
            Description = "Express past hypothetical outcomes (If + had + pp, would have + pp) and mixed past-present cause-and-effect conditions.",
            SkillCategory = SkillCategory.Grammar,
            Difficulty = LearnerCategory.Advanced,
            CefrLevel = EnglishProficiencyLevel.C1,
            PrerequisiteCodesJson = "[\"OBJ_S_COND_FIRST_SEC\"]",
            MasteryScoreThreshold = 80.0m,
            SuggestedDurationMinutes = 30,
            SortOrder = 9,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            Code = "OBJ_A_PASSIVE_REPORT",
            Title = "Advanced Passive Structures & Impersonal Reporting",
            Description = "Communicate with executive precision using structures like 'It is understood that...' and 'The deadline has been moved...'",
            SkillCategory = SkillCategory.SentenceConstruction,
            Difficulty = LearnerCategory.Advanced,
            CefrLevel = EnglishProficiencyLevel.C1,
            PrerequisiteCodesJson = "[\"OBJ_S_PRES_PERF\"]",
            MasteryScoreThreshold = 85.0m,
            SuggestedDurationMinutes = 30,
            SortOrder = 10,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            Code = "OBJ_A_IDIOMATIC_FLUENCY",
            Title = "Nuanced Idioms & Executive Professional Phrasing",
            Description = "Incorporate natural workplace idioms and metaphors (spearhead, ballpark figure, touch base, double-edged sword) effortlessly.",
            SkillCategory = SkillCategory.Vocabulary,
            Difficulty = LearnerCategory.Advanced,
            CefrLevel = EnglishProficiencyLevel.C1,
            PrerequisiteCodesJson = "[\"OBJ_S_PROF_TRANSITIONS\"]",
            MasteryScoreThreshold = 85.0m,
            SuggestedDurationMinutes = 30,
            SortOrder = 11,
            IsActive = true
        },
        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
            Code = "OBJ_A_STRATEGIC_DEBATE",
            Title = "Strategic Negotiation, Persuasion & Concession",
            Description = "Master diplomatic disagreement, concessive clauses (Granted that..., While I agree on X, Y remains critical), and persuasive rhetoric.",
            SkillCategory = SkillCategory.Speaking,
            Difficulty = LearnerCategory.Advanced,
            CefrLevel = EnglishProficiencyLevel.C2,
            PrerequisiteCodesJson = "[\"OBJ_A_COND_THIRD_MIX\"]",
            MasteryScoreThreshold = 85.0m,
            SuggestedDurationMinutes = 30,
            SortOrder = 12,
            IsActive = true
        }
    };

    public static readonly IReadOnlyList<AssessmentQuestionDto> DiagnosticAssessmentQuestions = new List<AssessmentQuestionDto>
    {
        // 1. Grammar (Basic -> Advanced)
        new(
            Id: "diag_g1",
            SkillCategory: "Grammar",
            Prompt: "Choose the correct sentence to describe your daily habit:",
            Options: new[] { "She work in an international office.", "She works in an international office.", "She is work in an international office.", "She working in an international office." },
            CorrectOptionIndex: 1,
            Difficulty: "Basic"
        ),
        new(
            Id: "diag_g2",
            SkillCategory: "Grammar",
            Prompt: "Select the sentence with the correct tense usage:",
            Options: new[] { "I have lived in this city since three years.", "I lived in this city since three years ago.", "I have lived in this city for three years.", "I am living in this city for three years." },
            CorrectOptionIndex: 2,
            Difficulty: "Standard"
        ),
        new(
            Id: "diag_g3",
            SkillCategory: "Grammar",
            Prompt: "Complete the conditional statement: If the client ________ our proposal yesterday, we ________ the project today.",
            Options: new[] { "approved; would start", "had approved; would be starting", "has approved; will start", "would approve; had started" },
            CorrectOptionIndex: 1,
            Difficulty: "Advanced"
        ),

        // 2. Vocabulary
        new(
            Id: "diag_v1",
            SkillCategory: "Vocabulary",
            Prompt: "What is the most accurate synonym for the word 'Resilient'?",
            Options: new[] { "Fragile and easily tired", "Able to recover quickly from challenges", "Rigid and unchanging", "Careless with deadlines" },
            CorrectOptionIndex: 1,
            Difficulty: "Standard"
        ),
        new(
            Id: "diag_v2",
            SkillCategory: "Vocabulary",
            Prompt: "In a professional business context, what does 'to spearhead an initiative' mean?",
            Options: new[] { "To criticize the initiative publicly", "To lead and drive the initiative forward", "To postpone the initiative indefinitely", "To assign the initiative to an external contractor" },
            CorrectOptionIndex: 1,
            Difficulty: "Advanced"
        ),

        // 3. Sentence Construction
        new(
            Id: "diag_sc1",
            SkillCategory: "SentenceConstruction",
            Prompt: "Which of the following phrases is grammatically natural and correct in formal English?",
            Options: new[] { "Myself John and I am software engineer.", "I am agree with your proposal completely.", "I have five years of experience in project management.", "We discussed about the budget yesterday." },
            CorrectOptionIndex: 2,
            Difficulty: "Standard"
        ),
        new(
            Id: "diag_sc2",
            SkillCategory: "SentenceConstruction",
            Prompt: "Choose the sentence that correctly uses an inversion for dramatic or formal emphasis:",
            Options: new[] { "Rarely we have seen such dedicated effort.", "Rarely have we seen such dedicated effort.", "Have we rarely seen such dedicated effort.", "Rarely we had seen such dedicated effort." },
            CorrectOptionIndex: 1,
            Difficulty: "Advanced"
        ),

        // 4. Reading and Comprehension
        new(
            Id: "diag_r1",
            SkillCategory: "Reading",
            Passage: "Remote collaboration tools have revolutionized teamwork, yet they also introduce communication friction. When team members rely predominantly on brief text messages, misunderstandings regarding tone and urgency can emerge. Leading organizations therefore encourage hybrid sync-ups to maintain interpersonal rapport.",
            Prompt: "According to the passage, why do misunderstandings occur in remote teams?",
            Options: new[] { "Because team members attend too many video conferences.", "Because brief text messages can obscure the intended tone and urgency.", "Because hybrid sync-ups are ineffective.", "Because collaboration tools lack technical reliability." },
            CorrectOptionIndex: 1,
            Difficulty: "Standard"
        ),

        // 5. Speaking, Fluency & Coherence
        new(
            Id: "diag_sp1",
            SkillCategory: "Speaking",
            Prompt: "Read this sentence aloud clearly: 'Effective communication requires both active listening and articulate expression.'",
            Options: new[] { "I completed the spoken reading prompt", "Microphone was unavailable / skipped" },
            CorrectOptionIndex: 0,
            Difficulty: "Standard",
            SpeechTargetPhrase: "Effective communication requires both active listening and articulate expression."
        )
    };
}
