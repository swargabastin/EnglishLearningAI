import { apiRequest } from "./apiClient";
import {
  AssessmentQuestion,
  SubmitAssessmentPayload,
  LearnerAssessmentResult,
  DailyStudyPlan,
  PhaseCompletionResult,
  LearningPlanHistory,
  LearningObjectiveItem,
  ApiResult,
} from "@/types";

export const adaptiveLearningService = {
  /**
   * Fetch diagnostic assessment questions covering Grammar, Vocabulary,
   * Sentence Construction, Reading, and Speaking.
   */
  async getAssessmentQuestions(): Promise<AssessmentQuestion[]> {
    const res = await apiRequest<ApiResult<AssessmentQuestion[]>>(
      "/adaptivelearning/assessment/questions",
      {
        method: "GET",
      }
    );
    return res.data || [];
  },

  /**
   * Submit diagnostic assessment answers and optional recorded speech audio.
   */
  async submitAssessment(
    payload: SubmitAssessmentPayload
  ): Promise<LearnerAssessmentResult> {
    const res = await apiRequest<ApiResult<LearnerAssessmentResult>>(
      "/adaptivelearning/assessment/submit",
      {
        method: "POST",
        body: JSON.stringify(payload),
      }
    );
    if (!res.data) {
      throw new Error(res.message || "Failed to submit assessment.");
    }
    return res.data;
  },

  /**
   * Retrieve the learner's latest validated level assessment, or null if unassessed.
   */
  async getLatestAssessment(): Promise<LearnerAssessmentResult | null> {
    try {
      const res = await apiRequest<ApiResult<LearnerAssessmentResult | null>>(
        "/adaptivelearning/assessment/latest",
        {
          method: "GET",
        }
      );
      return res.data ?? null;
    } catch {
      return null;
    }
  },

  /**
   * Retrieve all available curriculum objectives / English tenses.
   */
  async getAllObjectives(): Promise<LearningObjectiveItem[]> {
    const res = await apiRequest<ApiResult<LearningObjectiveItem[]>>(
      "/adaptivelearning/objectives",
      {
        method: "GET",
      }
    );
    return res.data || [];
  },

  /**
   * Retrieve or idempotently create today's 4-phase daily study plan.
   * Can optionally force regeneration or request a specific tense/objective.
   */
  async getTodayPlan(forceRegenerate = false, objectiveCode?: string): Promise<DailyStudyPlan> {
    const params = new URLSearchParams();
    if (forceRegenerate) params.set("forceRegenerate", "true");
    if (objectiveCode) params.set("objective", objectiveCode);
    const queryString = params.toString() ? `?${params.toString()}` : "";

    const res = await apiRequest<ApiResult<DailyStudyPlan>>(
      `/adaptivelearning/plan/today${queryString}`,
      {
        method: "GET",
      }
    );
    if (!res.data) {
      throw new Error(res.message || "Failed to load today's study plan.");
    }
    return res.data;
  },

  /**
   * Increment active learning time for a specific phase (anti-idle tracking).
   */
  async updatePhaseProgress(
    phaseId: string,
    activeDurationSecondsToAdd: number
  ): Promise<{ phaseId: string; activeDurationMinutes: number; status: string }> {
    const res = await apiRequest<ApiResult<PhaseCompletionResult>>(
      `/adaptivelearning/plan/phase/${phaseId}/progress`,
      {
        method: "POST",
        body: JSON.stringify({ activeDurationSecondsToAdd }),
      }
    );
    return {
      phaseId,
      activeDurationMinutes: Math.round(activeDurationSecondsToAdd / 60),
      status: res.data?.status || "InProgress",
    };
  },

  /**
   * Submit verified task answers to complete a phase and unlock the next phase.
   */
  async completePhase(
    phaseId: string,
    taskAnswers: Record<string, string>,
    activeDurationSeconds: number
  ): Promise<PhaseCompletionResult> {
    const res = await apiRequest<ApiResult<PhaseCompletionResult>>(
      `/adaptivelearning/plan/phase/${phaseId}/complete`,
      {
        method: "POST",
        body: JSON.stringify({ taskAnswers, activeDurationSeconds }),
      }
    );
    if (!res.data) {
      throw new Error(res.message || "Failed to complete phase.");
    }
    return res.data;
  },

  /**
   * Retrieve the learner's historical plans, mastery records, and recommended revisions.
   */
  async getLearningHistory(): Promise<LearningPlanHistory> {
    const res = await apiRequest<ApiResult<LearningPlanHistory>>(
      "/adaptivelearning/plan/history",
      {
        method: "GET",
      }
    );
    return (
      res.data || {
        recentPlans: [],
        masteredObjectivesCount: 0,
        totalActiveMinutes: 0,
        masteryBreakdown: [],
        recommendedRevisions: [],
      }
    );
  },

  /**
   * Option 3: Translate phrases into natural and formal English with grammar explanations.
   */
  async translatePhrase(
    text: string,
    sourceLanguage = "Auto",
    targetStyle = "Conversational"
  ): Promise<import("@/types").TranslateResult> {
    const res = await apiRequest<ApiResult<import("@/types").TranslateResult>>(
      "/adaptivelearning/translate",
      {
        method: "POST",
        body: JSON.stringify({ text, sourceLanguage, targetStyle }),
      }
    );
    if (!res.data) {
      throw new Error(res.message || "Failed to translate phrase.");
    }
    return res.data;
  },

  /**
   * Option 4: Analyze pasted document text for summary, vocabulary, and discussion questions.
   */
  async analyzeDocumentText(
    documentTitle: string,
    documentText: string
  ): Promise<import("@/types").DocumentAnalysisResult> {
    const res = await apiRequest<ApiResult<import("@/types").DocumentAnalysisResult>>(
      "/adaptivelearning/document/analyze",
      {
        method: "POST",
        body: JSON.stringify({ documentTitle, documentText }),
      }
    );
    if (!res.data) {
      throw new Error(res.message || "Failed to analyze document.");
    }
    return res.data;
  },

  /**
   * Option 4: Upload a document/text file for Emma discussion.
   */
  async uploadDocumentFile(
    file: File
  ): Promise<import("@/types").DocumentAnalysisResult> {
    const formData = new FormData();
    formData.append("file", file, file.name);

    const res = await apiRequest<ApiResult<import("@/types").DocumentAnalysisResult>>(
      "/adaptivelearning/document/upload",
      {
        method: "POST",
        body: formData,
      }
    );
    if (!res.data) {
      throw new Error(res.message || "Failed to process uploaded file.");
    }
    return res.data;
  },
};
