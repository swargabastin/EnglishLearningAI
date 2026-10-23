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
            Status = StudyPlanStatus.InProgress,
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
            // 1. Present Simple
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
                    "I work as a software engineer in an international firm.",
                    "She manages client communications every Tuesday morning.",
                    "Does your manager review project milestones weekly?"
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'She work here.' -> Correct: 'She works here.'",
                    "Incorrect: 'Does he lives here?' -> Correct: 'Does he live here?'"
                }
            ),

            // 2. Present Continuous
            "OBJ_B_PRES_CONT" => new LessonContentDto(
                RuleTitle: "Present Continuous Tense",
                Summary: "Used for actions happening right now, temporary situations, or near-future planned arrangements.",
                Formula: "Subject + am/is/are + Verb-ing | Negative: Subject + am/is/are not + Verb-ing",
                KeyRules: new[]
                {
                    "Use 'am' with I; 'is' with he/she/it; 'are' with you/we/they.",
                    "Stative verbs (like, know, belong, understand) are rarely used in continuous form.",
                    "Add '-ing' to the base verb (write -> writing; run -> running)."
                },
                Examples: new[]
                {
                    "I am currently leading the client onboarding sprint.",
                    "Our developers are investigating the latency issue right now.",
                    "We are meeting with the product director tomorrow afternoon."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'I am knowing the answer.' -> Correct: 'I know the answer.'",
                    "Incorrect: 'He working today.' -> Correct: 'He is working today.'"
                }
            ),

            // 3. Past Simple
            "OBJ_B_PAST_SIMPLE" => new LessonContentDto(
                RuleTitle: "Past Simple Tense",
                Summary: "Used for completed past events at a specific time. Regular verbs add '-ed'; irregular verbs have unique past forms.",
                Formula: "Subject + Past Verb (V2) | Negative: Subject + did not + Base Verb",
                KeyRules: new[]
                {
                    "Use 'did not / didn't' with all subjects, followed by the BASE verb.",
                    "Common irregular verbs: go -> went; see -> saw; write -> wrote; make -> made.",
                    "Specific past time markers: yesterday, last week, in 2023, two hours ago."
                },
                Examples: new[]
                {
                    "Our team deployed the new feature yesterday afternoon.",
                    "She graduated from the university two years ago.",
                    "Did you review the release notes this morning?"
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'I didn't saw him.' -> Correct: 'I didn't see him.'",
                    "Incorrect: 'We passed out from college last year.' -> Correct: 'We graduated from college last year.'"
                }
            ),

            // 4. Past Continuous
            "OBJ_B_PAST_CONT" => new LessonContentDto(
                RuleTitle: "Past Continuous Tense",
                Summary: "Used for actions in progress at a specific past moment, or an ongoing background action interrupted by a shorter past simple event.",
                Formula: "Subject + was/were + Verb-ing | Keywords: while, when, as",
                KeyRules: new[]
                {
                    "Use 'was' with I, he, she, it; 'were' with you, we, they.",
                    "Combine with Past Simple using 'when' or 'while' (While I was working, the power went out).",
                    "Sets the scene in storytelling and professional incident descriptions."
                },
                Examples: new[]
                {
                    "I was presenting our architectural proposal when the network dropped.",
                    "They were analyzing the test logs all morning yesterday.",
                    "What were you working on at 3:00 PM yesterday?"
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'While I worked, the server crashed.' -> Correct: 'While I was working, the server crashed.'",
                    "Incorrect: 'We was preparing the slides.' -> Correct: 'We were preparing the slides.'"
                }
            ),

            // 5. Future Simple (Will & Going To)
            "OBJ_B_FUTURE_PLANS" => new LessonContentDto(
                RuleTitle: "Future Simple: Will vs Going To",
                Summary: "Use 'going to' for pre-planned decisions and intentions. Use 'will' for spontaneous decisions, offers, promises, and future predictions.",
                Formula: "Will: Subject + will + Base Verb | Going To: Subject + am/is/are going to + Base Verb",
                KeyRules: new[]
                {
                    "Spontaneous decision at the moment of speaking: use 'will' (I will answer the phone).",
                    "Prior plan or intention decided earlier: use 'be going to' (I am going to change my job next month).",
                    "Predictions with present evidence: use 'going to' (Look at the clouds, it is going to rain)."
                },
                Examples: new[]
                {
                    "I am going to deliver our quarterly presentation next Friday.",
                    "Don't worry about the minutes; I will send them to the team right now.",
                    "AI will transform software engineering over the next decade."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'I will go to visit my parents tomorrow.' (planned) -> Correct: 'I am going to visit my parents tomorrow.'",
                    "Incorrect: 'I going to help you.' -> Correct: 'I am going to help you.'"
                }
            ),

            // 6. Present Perfect
            "OBJ_S_PRES_PERF" => new LessonContentDto(
                RuleTitle: "Present Perfect vs Past Simple",
                Summary: "Present Perfect connects past actions to the present (experience, unfinished time, recent results). Past Simple describes events finished at a specific past time.",
                Formula: "Subject + have/has + Past Participle (V3) | Time signals: since, for, already, yet, ever",
                KeyRules: new[]
                {
                    "Use 'since' for a specific starting point (since 2021, since Monday).",
                    "Use 'for' with a duration of time (for three years, for two months).",
                    "Never use Present Perfect with finished past time words like 'yesterday' or 'in 2019'."
                },
                Examples: new[]
                {
                    "I have worked with this tech stack for over three years.",
                    "She has lived in Bangalore since last October.",
                    "We finished the security audit yesterday afternoon (Past Simple)."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'I have seen him yesterday.' -> Correct: 'I saw him yesterday.'",
                    "Incorrect: 'I am working here since two years.' -> Correct: 'I have worked / have been working here for two years.'"
                }
            ),

            // 7. Present Perfect Continuous
            "OBJ_S_PRES_PERF_CONT" => new LessonContentDto(
                RuleTitle: "Present Perfect Continuous",
                Summary: "Emphasizes the duration and continuity of an action that started in the past and is still ongoing right now, or recently stopped with noticeable present evidence.",
                Formula: "Subject + have/has been + Verb-ing | Time signals: for, since, all day, lately",
                KeyRules: new[]
                {
                    "Emphasizes the continuous activity itself rather than the finished product.",
                    "Use with 'how long': 'How long have you been studying English?'",
                    "With stative verbs, fall back to Present Perfect Simple (I have known him for years, NOT have been knowing)."
                },
                Examples: new[]
                {
                    "I have been developing web applications since 2020.",
                    "Our engineering team has been refactoring the codebase all week.",
                    "She has been preparing for her technical interview for three months."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'I am learning English since two years.' -> Correct: 'I have been learning English for two years.'",
                    "Incorrect: 'He has been worked here for a month.' -> Correct: 'He has been working here for a month.'"
                }
            ),

            // 8. Past Perfect
            "OBJ_S_PAST_PERF" => new LessonContentDto(
                RuleTitle: "Past Perfect Tense",
                Summary: "Used when referring to an action that occurred before another specific action or milestone in the past (the 'earlier past').",
                Formula: "Subject + had + Past Participle (V3) | Time signals: before, after, by the time, already",
                KeyRules: new[]
                {
                    "The earlier action uses Past Perfect ('had + V3'); the later action uses Past Simple.",
                    "By the time + Past Simple clause, Subject + had + V3.",
                    "Avoid overusing: only necessary when clarifying which past event happened first."
                },
                Examples: new[]
                {
                    "By the time the client joined the meeting, we had already resolved the bug.",
                    "She had completed her master's degree before moving to London.",
                    "The deployment had finished before the database migration began."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'When I arrived, the train left.' (ambiguous) -> Correct: 'When I arrived, the train had left.'",
                    "Incorrect: 'She had went home before lunch.' -> Correct: 'She had gone home before lunch.'"
                }
            ),

            // 9. Past Perfect Continuous
            "OBJ_S_PAST_PERF_CONT" => new LessonContentDto(
                RuleTitle: "Past Perfect Continuous",
                Summary: "Expresses an ongoing, continuous action that was taking place up until another specific event occurred in the past, often explaining the cause of a past situation.",
                Formula: "Subject + had been + Verb-ing | Time signals: for, since, before",
                KeyRules: new[]
                {
                    "Shows the duration of a past action before another past milestone.",
                    "Explains past causes: 'I was exhausted because I had been debugging code for eight hours.'",
                    "Subject pronoun + had been + verb-ing across all grammatical persons."
                },
                Examples: new[]
                {
                    "We had been troubleshooting the connection for two hours before finding the firewall issue.",
                    "She had been working at that consultancy for five years before she launched her startup.",
                    "The engineers were tired because they had been testing all night."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'I had been knowing him before 2020.' -> Correct: 'I had known him before 2020.' (stative verb)",
                    "Incorrect: 'They have been waiting for an hour when the CEO arrived.' -> Correct: 'They had been waiting for an hour when the CEO arrived.'"
                }
            ),

            // 10. Future Continuous
            "OBJ_S_FUTURE_CONT" => new LessonContentDto(
                RuleTitle: "Future Continuous Tense",
                Summary: "Describes an action that will be actively in progress at a specific time in the future, or used for polite questions regarding future plans.",
                Formula: "Subject + will be + Verb-ing | Time signals: at this time tomorrow, this time next week",
                KeyRules: new[]
                {
                    "Focuses on being in the middle of doing something at a future moment.",
                    "Polite inquiry without putting pressure: 'Will you be using the projector this afternoon?'",
                    "Do not use in time clauses starting with when, as soon as (use present continuous instead)."
                },
                Examples: new[]
                {
                    "At 10:00 AM tomorrow, I will be presenting our product roadmap to investors.",
                    "This time next week, we will be celebrating our successful launch.",
                    "Will you be attending the team retro on Thursday?"
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'Tomorrow at 10 AM I will present.' (misses ongoing nuance) -> Better: 'I will be presenting.'",
                    "Incorrect: 'I will be have lunch at 1 PM.' -> Correct: 'I will be having lunch at 1 PM.'"
                }
            ),

            // 11. Future Perfect
            "OBJ_A_FUTURE_PERF" => new LessonContentDto(
                RuleTitle: "Future Perfect Tense",
                Summary: "Used to look back from a point in the future and say that an action will already be completed before that time.",
                Formula: "Subject + will have + Past Participle (V3) | Time signals: by tomorrow, by 2028, in two weeks' time",
                KeyRules: new[]
                {
                    "Essential for discussing professional milestones and deliverables.",
                    "Always pair with a future cutoff point introduced by 'by' or 'by the time'.",
                    "Formula remains 'will have + V3' regardless of subject (he will have, they will have)."
                },
                Examples: new[]
                {
                    "By the end of this quarter, our team will have migrated all microservices to AWS.",
                    "In two months' time, she will have completed her executive leadership curriculum.",
                    "By the time you finish your review, I will have pushed the revised pull request."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'By tomorrow I will finish the report.' -> Correct: 'By tomorrow I will have finished the report.'",
                    "Incorrect: 'By next year he will has completed it.' -> Correct: 'He will have completed it.'"
                }
            ),

            // 12. Future Perfect Continuous
            "OBJ_A_FUTURE_PERF_CONT" => new LessonContentDto(
                RuleTitle: "Future Perfect Continuous",
                Summary: "Emphasizes the continuous duration of an activity that will be ongoing up to a specific future milestone.",
                Formula: "Subject + will have been + Verb-ing | Time signals: by [future date], for [duration]",
                KeyRules: new[]
                {
                    "Calculates total time in a role, career, or project at a future date.",
                    "Combines a future deadline ('by next December') with an ongoing duration ('for ten years').",
                    "Stative verbs use Future Perfect Simple instead."
                },
                Examples: new[]
                {
                    "By this November, I will have been leading this distributed engineering team for five years.",
                    "Next month, they will have been working on this AI research initiative for a full decade.",
                    "When he retires, he will have been mentoring young professionals for over thirty years."
                },
                CommonMistakes: new[]
                {
                    "Incorrect: 'By next year I will be working here for 5 years.' -> Correct: 'I will have been working here for 5 years.'",
                    "Incorrect: 'By 2027 we will have been knowing each other for a decade.' -> Correct: 'We will have known each other for a decade.'"
                }
            ),

            // Conditionals
            "OBJ_A_COND_THIRD_MIX" => new LessonContentDto(
                RuleTitle: "Third & Mixed Conditionals",
                Summary: "Express imaginary past situations and their past or present consequences (hypothetical regrets and alternate timelines).",
                Formula: "Third: If + had + V3, would have + V3 | Mixed: If + had + V3, would + Base Verb",
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
                Summary: "Focus on grammatical precision, appropriate tense selection, and natural fluency in conversational and professional English.",
                Formula: "Subject + Verb + Object + Contextual Modifiers",
                KeyRules: new[]
                {
                    "Ensure subject-verb agreement across all clauses.",
                    "Match your chosen grammatical tense to the exact timeline of your action."
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
                    "OBJ_B_PRES_CONT" => "Which sentence describes an action happening right at this moment?",
                    "OBJ_B_PAST_CONT" => "Which sentence correctly shows an ongoing background action interrupted by a shorter event?",
                    "OBJ_S_PRES_PERF_CONT" => "Which sentence emphasizes an action that started in the past and is still ongoing right now?",
                    "OBJ_S_PAST_PERF" => "Which sentence correctly describes which of two past actions happened first?",
                    "OBJ_S_PAST_PERF_CONT" => "Which sentence correctly highlights past duration before another past event?",
                    "OBJ_S_FUTURE_CONT" => "Which sentence describes an action in progress at a specific future moment?",
                    "OBJ_A_FUTURE_PERF" => "Which sentence correctly shows a deliverable finished by a future deadline?",
                    "OBJ_A_FUTURE_PERF_CONT" => "Which sentence highlights cumulative duration up to a future point?",
                    "OBJ_S_PRES_PERF" => "Which sentence correctly indicates an ongoing duration up to the present?",
                    "OBJ_A_COND_THIRD_MIX" => "Which sentence represents a correct third conditional structure?",
                    _ => "Which sentence demonstrates the correct grammatical form?"
                },
                Options: code switch
                {
                    "OBJ_B_PRES_CONT" => new[] { "I am currently refactoring the payment service.", "I refactor the payment service right now.", "I am refactor the payment service.", "I refactoring the payment service right now." },
                    "OBJ_B_PAST_CONT" => new[] { "I was explaining the bug when the network connection dropped.", "I explained the bug while the network dropped.", "I was explain the bug when the network was drop.", "I were explaining the bug when the network dropped." },
                    "OBJ_S_PRES_PERF_CONT" => new[] { "I have been designing cloud architectures for seven years.", "I am designing cloud architectures since seven years.", "I have been design cloud architectures since seven years.", "I had designed cloud architectures for seven years now." },
                    "OBJ_S_PAST_PERF" => new[] { "By the time the manager arrived, the team had already resolved the incident.", "By the time the manager arrived, the team has already resolved the incident.", "When the manager had arrived, the team resolved the incident.", "The team resolved the incident before the manager had arrived." },
                    "OBJ_S_PAST_PERF_CONT" => new[] { "We had been testing the API for three hours before discovering the bottleneck.", "We have been testing the API for three hours before discovering the bottleneck.", "We were test the API for three hours before discovering the bottleneck.", "We had tested the API for three hours while discovering the bottleneck." },
                    "OBJ_S_FUTURE_CONT" => new[] { "At 3:00 PM tomorrow, I will be hosting our client roadmap presentation.", "At 3:00 PM tomorrow, I will host our client roadmap presentation.", "At 3:00 PM tomorrow, I am host our client roadmap presentation.", "At 3:00 PM tomorrow, I will being host our presentation." },
                    "OBJ_A_FUTURE_PERF" => new[] { "By Friday evening, our team will have deployed the new release.", "By Friday evening, our team will deploy the new release.", "By Friday evening, our team has deployed the new release.", "By Friday evening, our team will have deploy the new release." },
                    "OBJ_A_FUTURE_PERF_CONT" => new[] { "By next March, she will have been managing this engineering department for five years.", "By next March, she will be managing this department for five years.", "By next March, she has been managing this department for five years.", "By next March, she will have managing this department for five years." },
                    "OBJ_S_PRES_PERF" => new[] { "I am working here since 2 years.", "I have worked here for two years.", "I worked here for two years ago.", "I have been work here since two years." },
                    "OBJ_A_COND_THIRD_MIX" => new[] { "If we had tested earlier, we would have prevented the outage.", "If we would test earlier, we had prevented the outage.", "If we tested earlier, we would prevent the outage.", "If we had tested earlier, we will prevent the outage." },
                    _ => new[] { "He manages the deployment pipeline.", "He manage the deployment pipeline.", "He is manage the deployment pipeline.", "He managing the deployment pipeline." }
                },
                CorrectAnswer: code switch
                {
                    "OBJ_B_PRES_CONT" => "I am currently refactoring the payment service.",
                    "OBJ_B_PAST_CONT" => "I was explaining the bug when the network connection dropped.",
                    "OBJ_S_PRES_PERF_CONT" => "I have been designing cloud architectures for seven years.",
                    "OBJ_S_PAST_PERF" => "By the time the manager arrived, the team had already resolved the incident.",
                    "OBJ_S_PAST_PERF_CONT" => "We had been testing the API for three hours before discovering the bottleneck.",
                    "OBJ_S_FUTURE_CONT" => "At 3:00 PM tomorrow, I will be hosting our client roadmap presentation.",
                    "OBJ_A_FUTURE_PERF" => "By Friday evening, our team will have deployed the new release.",
                    "OBJ_A_FUTURE_PERF_CONT" => "By next March, she will have been managing this engineering department for five years.",
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
                    "OBJ_B_PRES_CONT" => "is preparing / our team / the deployment release / currently",
                    "OBJ_B_PAST_CONT" => "I was reviewing / the server logs / when the notification / arrived",
                    "OBJ_S_PRES_PERF_CONT" => "has been collaborating / our team / on this initiative / for three months",
                    "OBJ_S_PAST_PERF" => "had verified / the credentials / before / we / granting access",
                    "OBJ_S_PAST_PERF_CONT" => "had been investigating / the engineers / the error / for several hours",
                    "OBJ_S_FUTURE_CONT" => "will be conducting / the interview / at 2:00 PM / our director",
                    "OBJ_A_FUTURE_PERF" => "will have finalized / by tomorrow morning / the agreement / the committee",
                    "OBJ_A_FUTURE_PERF_CONT" => "will have been leading / by next year / she / this organization / for a decade",
                    "OBJ_S_PRES_PERF" => "our team / successfully / has delivered / the sprint goals",
                    "OBJ_A_COND_THIRD_MIX" => "had consulted / if they / the architect / saved time / they would have",
                    _ => "conducts / every morning / the standup meeting / our team lead"
                },
                Options: null,
                CorrectAnswer: code switch
                {
                    "OBJ_B_PRES_CONT" => "Our team is currently preparing the deployment release.",
                    "OBJ_B_PAST_CONT" => "I was reviewing the server logs when the notification arrived.",
                    "OBJ_S_PRES_PERF_CONT" => "Our team has been collaborating on this initiative for three months.",
                    "OBJ_S_PAST_PERF" => "We had verified the credentials before granting access.",
                    "OBJ_S_PAST_PERF_CONT" => "The engineers had been investigating the error for several hours.",
                    "OBJ_S_FUTURE_CONT" => "Our director will be conducting the interview at 2:00 PM.",
                    "OBJ_A_FUTURE_PERF" => "The committee will have finalized the agreement by tomorrow morning.",
                    "OBJ_A_FUTURE_PERF_CONT" => "By next year she will have been leading this organization for a decade.",
                    "OBJ_S_PRES_PERF" => "Our team has successfully delivered the sprint goals.",
                    "OBJ_A_COND_THIRD_MIX" => "If they had consulted the architect they would have saved time.",
                    _ => "Our team lead conducts the standup meeting every morning."
                },
                Explanation: "Proper syntax order with subject, auxiliary verb, adverb, and object."
            ),
            new(
                Id: "p2_task_2",
                Type: "fill_in_the_blank",
                Instruction: "Identify and correct the grammatical mistake in this sentence:",
                Question: code switch
                {
                    "OBJ_B_PRES_CONT" => "I am work on the presentation right now.",
                    "OBJ_B_PAST_CONT" => "We was analyzing the reports when the call started.",
                    "OBJ_S_PRES_PERF_CONT" => "I am learning English since two years.",
                    "OBJ_S_PAST_PERF" => "When I arrived, the keynote speech already started.",
                    "OBJ_S_PAST_PERF_CONT" => "He has been coding for ten hours before he went to sleep.",
                    "OBJ_S_FUTURE_CONT" => "Tomorrow at 4 PM, I will meeting the investors.",
                    "OBJ_A_FUTURE_PERF" => "By 5:00 PM, I will finish the entire report.",
                    "OBJ_A_FUTURE_PERF_CONT" => "By next year, I will be working here for ten years.",
                    "OBJ_S_PRES_PERF" => "I am living in this city since five years.",
                    "OBJ_A_COND_THIRD_MIX" => "If I would have known about the meeting, I would have attended.",
                    _ => "My colleague do not agree with the new proposal."
                },
                Options: null,
                CorrectAnswer: code switch
                {
                    "OBJ_B_PRES_CONT" => "I am working on the presentation right now.",
                    "OBJ_B_PAST_CONT" => "We were analyzing the reports when the call started.",
                    "OBJ_S_PRES_PERF_CONT" => "I have been learning English for two years.",
                    "OBJ_S_PAST_PERF" => "When I arrived, the keynote speech had already started.",
                    "OBJ_S_PAST_PERF_CONT" => "He had been coding for ten hours before he went to sleep.",
                    "OBJ_S_FUTURE_CONT" => "Tomorrow at 4 PM, I will be meeting the investors.",
                    "OBJ_A_FUTURE_PERF" => "By 5:00 PM, I will have finished the entire report.",
                    "OBJ_A_FUTURE_PERF_CONT" => "By next year, I will have been working here for ten years.",
                    "OBJ_S_PRES_PERF" => "I have lived in this city for five years.",
                    "OBJ_A_COND_THIRD_MIX" => "If I had known about the meeting, I would have attended.",
                    _ => "My colleague does not agree with the new proposal."
                },
                Explanation: "Corrects common prepositional, auxiliary verb, or tense choice errors."
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
                Question: code switch
                {
                    "OBJ_B_PRES_CONT" => "In a team standup, you want to state what you are actively working on today and what you are doing tomorrow.",
                    "OBJ_S_PRES_PERF_CONT" => "In an interview, you want to describe your cumulative experience and what you have been focusing on recently.",
                    "OBJ_S_PAST_PERF" => "During an incident post-mortem, you want to explain that a patch had been developed prior to the system crash.",
                    "OBJ_A_FUTURE_PERF" => "In an executive meeting, you want to commit to completing a deliverable before a specific date.",
                    _ => "During a client review, you want to explain that a delay occurred in the past, but the solution has already been deployed."
                },
                Options: code switch
                {
                    "OBJ_B_PRES_CONT" => new[]
                    {
                        "I am currently optimizing our payment service, and tomorrow I am meeting with the security team.",
                        "I optimize the payment service today, and tomorrow I meet.",
                        "I am optimize the payment service today, and tomorrow I will meeting.",
                        "I optimizing payment service today, meeting tomorrow."
                    },
                    "OBJ_S_PRES_PERF_CONT" => new[]
                    {
                        "I have been working as a full-stack engineer for five years, and lately I have been designing AI microservices.",
                        "I am working as a full-stack engineer since five years, and recently I design AI microservices.",
                        "I was working as a full-stack engineer for five years, and I am design AI microservices.",
                        "I have worked as a full-stack engineer since five years, lately I designing AI microservices."
                    },
                    "OBJ_S_PAST_PERF" => new[]
                    {
                        "Our engineering team had already committed the security patch before the alert triggered.",
                        "Our engineering team already committed the security patch before the alert has triggered.",
                        "Our engineering team has already committed the security patch before the alert had triggered.",
                        "Our engineering team was committed the patch before alert triggered."
                    },
                    "OBJ_A_FUTURE_PERF" => new[]
                    {
                        "By the end of next month, we will have finalized the cloud migration and delivered all security audits.",
                        "By the end of next month, we will finalize the cloud migration and deliver all security audits.",
                        "By the end of next month, we have finalized the cloud migration and deliver all security audits.",
                        "By the end of next month, we are finalized the cloud migration."
                    },
                    _ => new[]
                    {
                        "We encountered a minor delay last week; however, we have already resolved it and deployed the patch.",
                        "We have faced a minor delay last week, but we solve it now.",
                        "We was facing a delay yesterday, but everything is agree.",
                        "There was delay before, and we are discuss about that."
                    }
                },
                CorrectAnswer: code switch
                {
                    "OBJ_B_PRES_CONT" => "I am currently optimizing our payment service, and tomorrow I am meeting with the security team.",
                    "OBJ_S_PRES_PERF_CONT" => "I have been working as a full-stack engineer for five years, and lately I have been designing AI microservices.",
                    "OBJ_S_PAST_PERF" => "Our engineering team had already committed the security patch before the alert triggered.",
                    "OBJ_A_FUTURE_PERF" => "By the end of next month, we will have finalized the cloud migration and delivered all security audits.",
                    _ => "We encountered a minor delay last week; however, we have already resolved it and deployed the patch."
                },
                Explanation: "Skillfully combines appropriate grammatical tenses with professional workplace vocabulary."
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
                Question: $"Describe a real situation from your work or studies applying '{title}'. Speak in at least three full sentences, paying careful attention to correct auxiliary verbs and tense endings.",
                Options: null,
                CorrectAnswer: null,
                Explanation: "Focus on clear pronunciation, natural pacing, and deliberate usage of today's target grammatical structure."
            )
        };
    }
}
