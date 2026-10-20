import { AssessmentResult } from "@/types";

export const mockAssessment: AssessmentResult = {
  sessionId: "session-8821",
  topicTitle: "Job Interview Practice",
  englishLevel: "B1",
  completedAt: "Today, 10:15 AM",
  overallScore: 78,
  grammarScore: 75,
  vocabularyScore: 82,
  fluencyScore: 76,
  pronunciationScore: 80,
  strengths: [
    "Used relevant workplace and technology vocabulary accurately",
    "Maintained a confident and polite conversational tone",
    "Successfully answered open-ended follow-up questions with clear structure"
  ],
  areasToImprove: [
    "Consistency with present perfect vs. past simple tenses",
    "Sentence expansion when elaborating on complex technical challenges",
    "Reducing conversational filler pauses during spontaneous replies"
  ],
  grammarCorrections: [
    {
      id: "gc-1",
      original: "I am working as a software developer since 2022.",
      corrected: "I have been working as a software developer since 2022.",
      explanation: "Use the present perfect continuous tense ('have been working') for an action that began in the past and continues into the present.",
      ruleCategory: "Verb Tense Consistency"
    },
    {
      id: "gc-2",
      original: "We had a very tight deadline for client release.",
      corrected: "We had a very tight deadline for the client release.",
      explanation: "Add the definite article 'the' before specific singular nouns in professional context.",
      ruleCategory: "Articles & Determiners"
    }
  ],
  vocabularySuggestions: [
    {
      id: "vs-1",
      originalWord: "delivered on time",
      suggestedAlternative: "successfully met our release milestone",
      context: "Describing project delivery in interviews",
      cefrLevel: "B2"
    },
    {
      id: "vs-2",
      originalWord: "coordinated with team",
      suggestedAlternative: "orchestrated cross-functional collaboration",
      context: "Emphasizing leadership skills",
      cefrLevel: "C1"
    },
    {
      id: "vs-3",
      originalWord: "database crashed",
      suggestedAlternative: "experienced an unexpected database outage",
      context: "Formal incident reporting",
      cefrLevel: "B2"
    }
  ],
  fluencyFeedback: {
    wordCount: 184,
    speakingRateWpm: 112,
    pauseCount: 4,
    summary: "Steady speaking pace with natural phrasing. Minor hesitation observed before technical explanations."
  },
  recommendedPractice: {
    title: "Present Perfect & Workplace Phrasings",
    description: "Practice combining time prepositions ('since', 'for') with present perfect continuous constructions in professional scenarios.",
    actionTopicId: "topic-6"
  }
};
