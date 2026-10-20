import { apiRequest } from "./apiClient";
import { ApiResult, VocabularyItem } from "@/types";

export interface AddVocabularyPayload {
  word: string;
  meaning: string;
  example: string;
  contextSessionId?: string;
}

export const vocabularyService = {
  async getVocabulary(): Promise<VocabularyItem[]> {
    const response = await apiRequest<ApiResult<VocabularyItem[]>>("/vocabulary", {
      method: "GET",
    });

    return response.data || [];
  },

  async addVocabulary(payload: AddVocabularyPayload): Promise<VocabularyItem> {
    const response = await apiRequest<ApiResult<VocabularyItem>>("/vocabulary", {
      method: "POST",
      body: JSON.stringify(payload),
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to save vocabulary word.");
    }

    return response.data;
  },

  async updateMastery(id: string, masteryLevel: number): Promise<VocabularyItem> {
    const response = await apiRequest<ApiResult<VocabularyItem>>(`/vocabulary/${id}/mastery`, {
      method: "PUT",
      body: JSON.stringify({ masteryLevel }),
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to update mastery level.");
    }

    return response.data;
  },

  async deleteVocabulary(id: string): Promise<boolean> {
    const response = await apiRequest<ApiResult<boolean>>(`/vocabulary/${id}`, {
      method: "DELETE",
    });

    return response.data ?? true;
  },
};
