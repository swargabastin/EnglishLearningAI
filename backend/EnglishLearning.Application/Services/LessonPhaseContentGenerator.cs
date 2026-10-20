using System.Text.Json;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Services;

public static class LessonPhaseContentGenerator
{
    public static List<StudyPlanPhase> GeneratePhasesForObjective(StudyPlan plan, LearningObjective objective, LearnerCategory category)
    {
        var (p1Duration, p2Duration, p3Duration, p4Duration) = category switch
        {
            LearnerCategory.Basic => (4, 4, 4, 3), // Sum = 15 min
            LearnerCategory.Advanced => (7, 7, 8, 8), // Sum = 30 min
            _ => (5, 5, 5, 5) // Standard Sum = 20 min
        };

        var phases = new List<StudyPlanPhase>();

        // ----------------------------------------------------
        // PHASE 1: Grammar Study
        // ----------------------------------------------------
        var lessonContent = BuildLessonContent(objective.Code);
        var phase1Tasks = BuildPhase1Tasks(objective.Code);

        phases.Add(new StudyPlanPhase
        {
            StudyPlanId = plan.Id,
            PhaseOrder = 1,
            PhaseType = PhaseType.GrammarStudy,
            ObjectiveTitle = $"Grammar Study: {objective.Title}",
            LessonContentJson = JsonSerializer.Serialize(lessonContent),
            TasksJson = JsonSerializer.Serialize(phase1Tasks),
            PlannedDurationMinutes = p1Duration,
            ActualDurationMinutes = 0,
            Attempts = 0,
            Score = null,
            Status = StudyPlanStatus.InProgress, // Phase 1 is unlocked initially
            Feedback = null
        });

        // ----------------------------------------------------
        // PHASE 2: Sentence Making
        // ----------------------------------------------------
        var phase2Tasks = BuildPhase2Tasks(objective.Code);

        phases.Add(new StudyPlanPhase
        {
            StudyPlanId = plan.Id,
            PhaseOrder = 2,
            PhaseType = PhaseType.SentenceMaking,
            ObjectiveTitle = $"Sentence Construction: Applying {objective.Title}",
            LessonContentJson = JsonSerializer.Serialize(new { instruction = "Build and correct sentences applying the rules from Phase 1." }),
            TasksJson = JsonSerializer.Serialize(phase2Tasks),
            PlannedDurationMinutes = p2Duration,
            ActualDurationMinutes = 0,
            Attempts = 0,
            Score = null,
            Status = StudyPlanStatus.NotStarted,
            Feedback = null
        });

        // ----------------------------------------------------
        // PHASE 3: Practice
        // ----------------------------------------------------
        var phase3Tasks = BuildPhase3Tasks(objective.Code);

        phases.Add(new StudyPlanPhase
        {
            StudyPlanId = plan.Id,
            PhaseOrder = 3,
            PhaseType = PhaseType.Practice,
            ObjectiveTitle = $"Applied Practice: Situational Challenges for {objective.Title}",
            LessonContentJson = JsonSerializer.Serialize(new { instruction = "Solve real-world situational challenges using the target grammar and vocabulary." }),
            TasksJson = JsonSerializer.Serialize(phase3Tasks),
            PlannedDurationMinutes = p3Duration,
            ActualDurationMinutes = 0,
            Attempts = 0,
            Score = null,
            Status = StudyPlanStatus.NotStarted,
            Feedback = null
        });

        // ----------------------------------------------------
        // PHASE 4: AI Speaking with Emma
        // ----------------------------------------------------
        var phase4Tasks = BuildPhase4Tasks(objective.Code, objective.Title);

        phases.Add(new StudyPlanPhase
        {
            StudyPlanId = plan.Id,
            PhaseOrder = 4,
            PhaseType = PhaseType.AiSpeaking,
            ObjectiveTitle = $"AI Speaking with Emma: Live Conversation on {objective.Title}",
            LessonContentJson = JsonSerializer.Serialize(new { 
                instruction = "Engage in an interactive conversation with Emma applying all principles from today's study plan.",
                targetTopic = objective.Difficulty == LearnerCategory.Basic ? "Daily Life" : "Job Interview"
            }),
            TasksJson = JsonSerializer.Serialize(phase4Tasks),
            PlannedDurationMinutes = p4Duration,
            ActualDurationMinutes = 0,
            Attempts = 0,
            Score = null,
            Status = StudyPlanStatus.NotStarted,
            Feedback = null
        });

        return phases;
    }

    private static LessonContentDto BuildLessonContent(string code)
    {
        return code switch
        {
            "OBJ_B_PRES_SIMPLE" => new LessonContentDto(
                RuleTitle: "Present Simple Tense",
                Summary: "Used for habitual actions, facts, and permanent situations. Add '-s' or '-es' for third-person singular (he, she, it).",
                Formula: "Subject + Base Verb (+ -s/-es for he/she/it) | Negative: Subject + do/does not + Base Verb",
                KeyRules: new[]
                {
                    "Use do not / don't with I, you, we, they.",
                    "Use does not / doesn't with he, she, it.",
                    "In questions, invert: Do/Does + Subject + Base Verb?"
                },
                Examples: new[]
                {
                    "I work as a software professional in an international firm.",
                    "She manages client communications every Tuesday morning.",
                    "Does your manager review project milestones weekly?"
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'She work here.' -> Correct: 'She works here.'",
                    "Incorrect: 'Does he lives here?' -> Correct: 'Does he live here?'"
                }
            ),

            "OBJ_S_PRES_PERF" => new LessonContentDto(
                RuleTitle: "Present Perfect vs Past Simple",
                Summary: "Present Perfect connects past actions to the present (experience, unfinished time). Past Simple describes events finished at a specific past time.",
                Formula: "Subject + have/has + Past Participle (V3) | Time signals: since, for, already, yet",
                KeyRules: new[]
                {
                    "Use 'since' for a specific starting point (since 2021, since Monday).",
                    "Use 'for' with a duration of time (for three years, for two months).",
                    "Never use Present Perfect with finished past time words like 'yesterday' or 'in 2019'."
                },
                Examples: new[]
                {
                    "I have worked with this framework for over three years.",
                    "She has lived in Bangalore since last October.",
                    "We finished the security audit yesterday afternoon (Past Simple)."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'I have seen him yesterday.' -> Correct: 'I saw him yesterday.'",
                    "Incorrect: 'I have three years experience.' -> Correct: 'I have three years of experience.'"
                }
            ),

            "OBJ_A_COND_THIRD_MIX" => new LessonContentDto(
                RuleTitle: "Third & Mixed Conditionals",
                Summary: "Express imaginary past situations and their past or present consequences (hypothetical regrets and alternate timelines).",
                Formula: "Third: If + had + V3, would have + V3 | Mixed: If + had + V3, would + Base Verb (past condition with present impact)",
                KeyRules: new[]
                {
                    "Third Conditional reflects on an unchangeable past event.",
                    "Mixed Conditional connects a past decision to a current, ongoing state.",
                    "Contracted speech: 'would have' is pronounced /wʊdəv/ ('would've')."
                },
                Examples: new[]
                {
                    "If we had identified the bottleneck earlier, we would have delivered the release on schedule.",
                    "If I had completed my cloud certification last year, I would be leading this architecture team today."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'If I would have known, I would tell you.' -> Correct: 'If I had known, I would have told you.'"
                }
            ),

            _ => new LessonContentDto(
                RuleTitle: "Core Language Principle",
                Summary: "Focus on clarity, grammatical precision, and natural fluency in conversational and professional English.",
                Formula: "Subject + Verb + Object + Contextual Modifiers",
                KeyRules: new[]
                {
                    "Ensure subject-verb agreement across all clauses.",
                    "Select precise vocabulary over vague approximations."
                },
                Examples: new[]
                {
                    "Clear communication fosters productive team collaboration."
                },
                CommonMistakes: new[]
                {
                    "Avoid word-for-word translation from your mother tongue."
                }
            )
        };
    }

    private static List<PhaseTaskDto> BuildPhase1Tasks(string code)
    {
        return new List<PhaseTaskDto>
        {
            new(
                Id: "p1_task_1",
                Type: "multiple_choice",
                Instruction: "Check your conceptual understanding of the rule:",
                Question: code switch
                {
                    "OBJ_S_PRES_PERF" => "Which sentence correctly indicates an ongoing duration up to the present?",
                    "OBJ_A_COND_THIRD_MIX" => "Which sentence represents a correct third conditional structure?",
                    _ => "Which sentence demonstrates the correct third-person singular form?"
                },
                Options: code switch
                {
                    "OBJ_S_PRES_PERF" => new[] { "I am working here since 2 years.", "I have worked here for two years.", "I worked here for two years ago.", "I have been work here since two years." },
                    "OBJ_A_COND_THIRD_MIX" => new[] { "If we had tested earlier, we would have prevented the outage.", "If we would test earlier, we had prevented the outage.", "If we tested earlier, we would prevent the outage.", "If we had tested earlier, we will prevent the outage." },
                    _ => new[] { "He manage the deployment pipeline.", "He manages the deployment pipeline.", "He is manage the deployment pipeline.", "He managing the deployment pipeline." }
                },
                CorrectAnswer: code switch
                {
                    "OBJ_S_PRES_PERF" => "I have worked here for two years.",
                    "OBJ_A_COND_THIRD_MIX" => "If we had tested earlier, we would have prevented the outage.",
                    _ => "He manages the deployment pipeline."
                },
                Explanation: "Applies the core grammatical formula accurately without common ESL interference."
            )
        };
    }

    private static List<PhaseTaskDto> BuildPhase2Tasks(string code)
    {
        return new List<PhaseTaskDto>
        {
            new(
                Id: "p2_task_1",
                Type: "sentence_builder",
                Instruction: "Assemble the words into a grammatically correct sentence:",
                Question: code switch
                {
                    "OBJ_S_PRES_PERF" => "our team / successfully / has delivered / the sprint goals",
                    "OBJ_A_COND_THIRD_MIX" => "had consulted / if they / the architect / saved time / they would have",
                    _ => "conducts / every morning / the standup meeting / our team lead"
                },
                Options: null,
                CorrectAnswer: code switch
                {
                    "OBJ_S_PRES_PERF" => "Our team has successfully delivered the sprint goals.",
                    "OBJ_A_COND_THIRD_MIX" => "If they had consulted the architect they would have saved time.",
                    _ => "Our team lead conducts the standup meeting every morning."
                },
                Explanation: "Proper syntax order with subject, auxiliary verb, adverb, and object."
            ),
            new(
                Id: "p2_task_2",
                Type: "fill_in_the_blank",
                Instruction: "Identify and correct the single grammatical mistake in this sentence:",
                Question: code switch
                {
                    "OBJ_S_PRES_PERF" => "I am living in this city since five years.",
                    "OBJ_A_COND_THIRD_MIX" => "If I would have known about the meeting, I would have attended.",
                    _ => "My colleague do not agree with the new proposal."
                },
                Options: null,
                CorrectAnswer: code switch
                {
                    "OBJ_S_PRES_PERF" => "I have lived in this city for five years.",
                    "OBJ_A_COND_THIRD_MIX" => "If I had known about the meeting, I would have attended.",
                    _ => "My colleague does not agree with the new proposal."
                },
                Explanation: "Corrects common prepositional or auxiliary verb errors."
            )
        };
    }

    private static List<PhaseTaskDto> BuildPhase3Tasks(string code)
    {
        return new List<PhaseTaskDto>
        {
            new(
                Id: "p3_task_1",
                Type: "multiple_choice",
                Instruction: "Select the most articulate response in this professional situation:",
                Question: "During a client review, you want to explain that a delay occurred in the past, but the solution has already been deployed.",
                Options: new[]
                {
                    "We have faced a minor delay last week, but we solve it now.",
                    "We encountered a minor delay last week; however, we have already resolved it and deployed the patch.",
                    "We was facing a delay yesterday, but everything is agree.",
                    "There was delay before, and we are discuss about that."
                },
                CorrectAnswer: "We encountered a minor delay last week; however, we have already resolved it and deployed the patch.",
                Explanation: "Skillfully combines Past Simple for the finished past time ('last week') with Present Perfect for the current accomplishment ('have already resolved')."
            ),
            new(
                Id: "p3_task_2",
                Type: "fill_in_the_blank",
                Instruction: "Complete the sentence with the appropriate transition word (Furthermore, In contrast, Since, Consequently):",
                Question: "The initial database schema was suboptimal; ________, query performance degraded under peak traffic.",
                Options: new[] { "consequently", "furthermore", "in contrast", "since" },
                CorrectAnswer: "consequently",
                Explanation: "Expresses cause and effect with professional precision."
            )
        };
    }

    private static List<PhaseTaskDto> BuildPhase4Tasks(string code, string title)
    {
        return new List<PhaseTaskDto>
        {
            new(
                Id: "p4_speaking_task",
                Type: "prompt",
                Instruction: "Speak aloud for 30–60 seconds in the Emma Practice Room focusing on today's objective:",
                Question: $"Describe a real situation from your work or studies where you applied concepts from '{title}'. Use at least two full sentences with appropriate connectors.",
                Options: null,
                CorrectAnswer: null,
                Explanation: "Focus on clear pronunciation, natural pacing, and deliberate usage of today's target grammatical structure."
            )
        };
    }
}
