export type CEFRLevel = 'A1' | 'A2' | 'B1' | 'B2' | 'C1' | 'C2';

export type LearningGoal =
  | 'Speaking Fluency'
  | 'Grammar & Accuracy'
  | 'Workplace English'
  | 'Job Interviews'
  | 'Travel & Daily Life'
  | 'Vocabulary Expansion';

export interface UserProfile {
  id: string;
  name: string;
  email: string;
  level: CEFRLevel;
  learningGoal: LearningGoal;
  dailyPracticeTargetMinutes: number;
  todayPracticeMinutes: number;
  streakDays: number;
  avatarUrl?: string;
  createdAt: string;
  preferences: {
    tutorVoice: 'alloy' | 'nova' | 'echo' | 'shimmer';
    playbackSpeed: 0.8 | 1.0 | 1.2;
    autoPlayAudio: boolean;
    showLiveTranscript: boolean;
  };
}

export interface RegisterPayload {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  englishLevel: string;
  learningGoal: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface AuthUserDto {
  id: string;
  name: string;
  email: string;
  englishLevel: string;
  learningGoal: string;
  createdAt: string;
}

export interface AuthResponseData {
  accessToken: string;
  expiresAt: string;
  user: AuthUserDto;
}

export interface ApiResult<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

export interface Topic {
  id: string;
  title: string;
  description: string;
  difficulty: CEFRLevel | 'All';
  category: 'General' | 'Professional' | 'Situational' | 'Academic';
  iconKey: string;
  estimatedMinutes: number;
  promptGuidance?: string;
  popular?: boolean;
}

export type ConversationRole = 'user' | 'assistant' | 'system';

export interface PronunciationPhonemeScore {
  phoneme: string;
  score: number;
  colorHex: string;
}

export interface PronunciationWordScore {
  word: string;
  score: number;
  status: "Good" | "Acceptable" | "NeedsPractice" | string;
  phonemes: PronunciationPhonemeScore[];
}

export interface PronunciationAssessment {
  overallScore: number;
  accuracyScore: number;
  fluencyScore: number;
  completenessScore: number;
  words: PronunciationWordScore[];
  feedback: string;
}

export interface ConversationMessage {
  id: string;
  role: ConversationRole;
  content: string;
  timestamp: string;
  audioDurationSec?: number;
  audioBase64?: string;
  pedagogicalTip?: string;
  pronunciation?: PronunciationAssessment;
}

export interface VocabularyItem {
  id: string;
  word: string;
  meaning: string;
  example: string;
  masteryLevel: number;
  contextSessionId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface GrammarCorrection {
  id: string;
  original: string;
  corrected: string;
  explanation: string;
  ruleCategory: string;
}

export interface VocabularySuggestion {
  id: string;
  originalWord: string;
  suggestedAlternative: string;
  context: string;
  cefrLevel: CEFRLevel;
}

export interface FluencyFeedback {
  wordCount: number;
  speakingRateWpm: number;
  pauseCount: number;
  summary: string;
}

export interface AssessmentResult {
  sessionId: string;
  topicTitle: string;
  englishLevel: CEFRLevel;
  completedAt: string;
  overallScore: number;
  grammarScore: number;
  vocabularyScore: number;
  fluencyScore: number;
  pronunciationScore?: number;
  strengths: string[];
  areasToImprove: string[];
  grammarCorrections: GrammarCorrection[];
  vocabularySuggestions: VocabularySuggestion[];
  fluencyFeedback: FluencyFeedback;
  recommendedPractice: {
    title: string;
    description: string;
    actionTopicId?: string;
  };
}

export interface HistorySession {
  id: string;
  topicId: string;
  topicTitle: string;
  englishLevel: CEFRLevel;
  date: string;
  durationMinutes: number;
  overallScore: number;
  grammarScore: number;
  vocabularyScore: number;
  fluencyScore: number;
  messageCount: number;
}

// Adaptive Learning Types
export type LearnerCategory = 'Basic' | 'Standard' | 'Advanced';
export type StudyPlanStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'NeedsReview';
export type AdaptivePhaseType = 'GrammarStudy' | 'SentenceMaking' | 'Practice' | 'AiSpeaking';

export interface AssessmentQuestion {
  id: string;
  skillCategory: string; // Grammar, Vocabulary, SentenceConstruction, Reading, Speaking
  prompt: string;
  options: string[];
  correctOptionIndex: number;
  passage?: string | null;
  difficulty: string;
  speechTargetPhrase?: string | null;
}

export interface QuestionAnswerPayload {
  questionId: string;
  selectedOptionIndex?: number | null;
  textInput?: string | null;
  timeSpentSeconds: number;
}

export interface SubmitAssessmentPayload {
  answers: QuestionAnswerPayload[];
  speechAudioBase64?: string | null;
  speechAudioMimeType?: string | null;
}

export interface LearnerAssessmentResult {
  id: string;
  category: LearnerCategory;
  cefrLevel: string;
  overallScore: number;
  grammarScore: number;
  vocabularyScore: number;
  sentenceScore: number;
  readingScore: number;
  speakingScore?: number | null;
  isSpeakingAssessed: boolean;
  weaknesses: string[];
  confidence: string;
  reassessmentRecommended: boolean;
  completedAt: string;
}

export interface PhaseTask {
  id: string;
  type: 'multiple_choice' | 'sentence_builder' | 'fill_in_the_blank' | 'prompt';
  instruction: string;
  question: string;
  options?: string[] | null;
  correctAnswer?: string | null;
  explanation?: string | null;
}

export interface LessonContent {
  ruleTitle: string;
  summary: string;
  formula: string;
  keyRules: string[];
  examples: string[];
  commonMistakes: string[];
}

export interface StudyPlanPhase {
  id: string;
  phaseOrder: number;
  phaseType: AdaptivePhaseType;
  objectiveTitle: string;
  plannedDurationMinutes: number;
  actualDurationMinutes: number;
  status: StudyPlanStatus;
  score?: number | null;
  lessonContent?: LessonContent | null;
  tasks: PhaseTask[];
  feedback?: string | null;
  isLocked: boolean;
}

export interface DailyStudyPlan {
  id: string;
  planDate: string;
  objectiveCode: string;
  objectiveTitle: string;
  difficulty: string;
  cefrLevel: string;
  targetMinutes: number;
  actualMinutes: number;
  status: StudyPlanStatus;
  overallScore?: number | null;
  phases: StudyPlanPhase[];
  isIdempotentCached: boolean;
}

export interface PhaseCompletionResult {
  phaseId: string;
  score: number;
  status: StudyPlanStatus;
  isMastered: boolean;
  feedback: string;
  nextPhaseUnlocked: boolean;
  overallPlanStatus: StudyPlanStatus;
}

export interface LearningObjectiveItem {
  id: string;
  code: string;
  title: string;
  description: string;
  skillCategory: string;
  difficulty: string;
  cefrLevel: string;
  suggestedDurationMinutes: number;
  sortOrder: number;
  isMastered: boolean;
  masteryScore?: number | null;
}

export interface LearnerSkillMasteryItem {
  objectiveCode: string;
  masteryScore: number;
  attemptCount: number;
  isMastered: boolean;
  lastPracticedAt: string;
  nextRevisionDueDate?: string | null;
}

export interface LearningPlanHistory {
  recentPlans: DailyStudyPlan[];
  masteredObjectivesCount: number;
  totalActiveMinutes: number;
  masteryBreakdown: LearnerSkillMasteryItem[];
  recommendedRevisions: string[];
}

export interface VocabularyPair {
  word: string;
  meaning: string;
}

export interface TranslateResult {
  originalText: string;
  naturalEnglish: string;
  formalEnglish: string;
  explanation: string;
  keyVocabulary: VocabularyPair[];
  pronunciationTip: string;
}

export interface DocumentVocabItem {
  word: string;
  meaning: string;
  exampleUsage: string;
}

export interface DocumentAnalysisResult {
  title: string;
  summary: string;
  cefrDifficulty: string;
  keyVocabulary: DocumentVocabItem[];
  discussionQuestions: string[];
  starterPrompt: string;
}

