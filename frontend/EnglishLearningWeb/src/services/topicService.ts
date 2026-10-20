import { apiRequest } from "./apiClient";
import { ApiResult, Topic } from "@/types";
import { mockTopics } from "@/lib/mock";

export interface BackendTopicDto {
  id: string;
  name: string;
  description: string;
  difficulty: string;
  iconKey: string;
  isActive: boolean;
  sortOrder: number;
}

export const topicService = {
  async getTopics(): Promise<Topic[]> {
    try {
      const response = await apiRequest<ApiResult<BackendTopicDto[]>>("/topics", {
        method: "GET",
      });

      if (response.data && response.data.length > 0) {
        return response.data.map((bt) => {
          const matchMock = mockTopics.find((mt) => mt.id === bt.id);
          return {
            id: bt.id,
            title: bt.name,
            description: bt.description,
            difficulty: bt.difficulty as Topic["difficulty"],
            category: matchMock?.category || "General",
            iconKey: bt.iconKey || matchMock?.iconKey || "MessageCircle",
            estimatedMinutes: matchMock?.estimatedMinutes || 10,
            popular: matchMock?.popular || false,
            promptGuidance: matchMock?.promptGuidance,
          };
        });
      }
    } catch {
      // Fallback gracefully to seeded topics
    }

    return mockTopics;
  },

  async getTopicById(id: string): Promise<Topic | undefined> {
    const all = await this.getTopics();
    return all.find((t) => t.id === id) || mockTopics.find((t) => t.id === id);
  },
};
