import { apiRequest } from "./apiClient";
import { ApiResult, CEFRLevel } from "@/types";

export interface GrammarCorrectionDto {
  original: string;
  corrected: string;
  explanation: string;
  ruleCategory?: string;
}

export interface VocabularySuggestionDto {
  originalWord: string;
  suggestedAlternative: string;
  context: string;
  cefrLevel?: string;
}

export interface AssessmentResponseDto {
  id: string;
  sessionId: string;
  topicName: string;
  englishLevel: CEFRLevel | string;
  overallScore: number;
  grammarScore: number;
  vocabularyScore: number;
  fluencyScore: number;
  pronunciationScore?: number;
  feedback: string;
  strengths: string[];
  areasForImprovement: string[];
  grammarCorrections: GrammarCorrectionDto[];
  vocabularySuggestions: VocabularySuggestionDto[];
  recommendations: string[];
  createdAt: string;
}

export const assessmentService = {
  async generateAssessment(sessionId: string): Promise<AssessmentResponseDto> {
    const response = await apiRequest<ApiResult<AssessmentResponseDto>>(`/conversations/${sessionId}/assessment`, {
      method: "POST",
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to generate AI conversation assessment.");
    }

    return response.data;
  },

  async getAssessment(sessionId: string): Promise<AssessmentResponseDto> {
    const response = await apiRequest<ApiResult<AssessmentResponseDto>>(`/conversations/${sessionId}/assessment`, {
      method: "GET",
    });

    if (!response.data) {
      throw new Error(response.message || "Unable to retrieve assessment for session.");
    }

    return response.data;
  },
};
