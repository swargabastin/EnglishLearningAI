import { apiRequest } from "./apiClient";
import { ApiResult, CEFRLevel } from "@/types";
import { AssessmentResponseDto } from "./assessmentService";
import { ConversationMessageDto } from "./conversationService";

export interface UserProfileSummaryDto {
  id: string;
  name: string;
  email: string;
  englishLevel: CEFRLevel | string;
  learningGoal: string;
  totalPracticeMinutes: number;
  totalSessionsCompleted: number;
}

export interface ProgressStatsDto {
  totalCompletedSessions: number;
  totalPracticeMinutes: number;
  averageOverallScore: number;
  latestScore: number;
  grammarAverage: number;
  vocabularyAverage: number;
  fluencyAverage: number;
  assessedLevel: string;
}

export interface RecentSessionSummaryDto {
  sessionId: string;
  topicId: string;
  topicName: string;
  englishLevel: string;
  startedAt: string;
  durationMinutes: number;
  overallScore: number;
  hasAssessment: boolean;
}

export interface ProgressChartPointDto {
  date: string;
  overallScore: number;
  grammarScore: number;
  vocabularyScore: number;
  fluencyScore: number;
  topicName: string;
}

export interface DashboardResponseDto {
  user: UserProfileSummaryDto;
  stats: ProgressStatsDto;
  recentSessions: RecentSessionSummaryDto[];
  scoreProgression: ProgressChartPointDto[];
}

export interface HistoryItemDto {
  sessionId: string;
  topicId: string;
  topicName: string;
  englishLevel: string;
  startedAt: string;
  endedAt?: string;
  durationMinutes: number;
  overallScore: number;
  grammarScore: number;
  vocabularyScore: number;
  fluencyScore: number;
  messageCount: number;
  hasAssessment: boolean;
  assessmentId?: string;
}

export interface PaginatedListDto<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface SessionDetailDto {
  sessionId: string;
  topicId: string;
  topicName: string;
  topicDescription: string;
  englishLevel: string;
  status: string;
  startedAt: string;
  endedAt?: string;
  durationSeconds: number;
  overallScore?: number;
  messages: ConversationMessageDto[];
  assessment?: AssessmentResponseDto;
}

export const progressService = {
  async getDashboard(): Promise<DashboardResponseDto> {
    const response = await apiRequest<ApiResult<DashboardResponseDto>>("/progress/dashboard", {
      method: "GET",
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to load progress dashboard.");
    }

    return response.data;
  },

  async getHistory(page = 1, pageSize = 20): Promise<PaginatedListDto<HistoryItemDto>> {
    const response = await apiRequest<ApiResult<PaginatedListDto<HistoryItemDto>>>(
      `/progress/history?page=${page}&pageSize=${pageSize}`,
      {
        method: "GET",
      }
    );

    if (!response.data) {
      throw new Error(response.message || "Failed to load practice history.");
    }

    return response.data;
  },

  async getSessionDetail(sessionId: string): Promise<SessionDetailDto> {
    const response = await apiRequest<ApiResult<SessionDetailDto>>(`/progress/sessions/${sessionId}`, {
      method: "GET",
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to retrieve session detail.");
    }

    return response.data;
  },
};
