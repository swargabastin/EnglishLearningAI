import { apiRequest } from "./apiClient";
import { ApiResult, CEFRLevel } from "@/types";

export interface ConversationMessageDto {
  id: string;
  sessionId: string;
  role: "user" | "assistant" | "system";
  messageText: string;
  audioStoragePath?: string;
  sequenceNumber: number;
  createdAt: string;
}

export interface ConversationSessionDto {
  id: string;
  userId: string;
  topicId: string;
  topicName: string;
  englishLevel: string;
  startedAt: string;
  endedAt?: string;
  durationSeconds: number;
  overallScore?: number;
  status: "Active" | "Completed" | "Abandoned";
  messages?: ConversationMessageDto[];
}

export interface SendMessageResponseDto {
  session: ConversationSessionDto;
  userMessage: ConversationMessageDto;
  aiMessage: ConversationMessageDto;
  pedagogicalTip?: string;
}

export interface SpeechConversationResponseDto {
  session: ConversationSessionDto;
  userMessage: ConversationMessageDto;
  aiMessage: ConversationMessageDto;
  recognizedText: string;
  audioBase64?: string;
  audioContentType: string;
  pedagogicalTip?: string;
}

export interface TopicDto {
  id: string;
  name: string;
  description: string;
  difficulty: string;
  iconKey: string;
  isActive: boolean;
  sortOrder: number;
}

export const conversationService = {
  async startConversation(
    topicId: string,
    englishLevel?: CEFRLevel | string,
    targetObjective?: string
  ): Promise<ConversationSessionDto> {
    const response = await apiRequest<ApiResult<ConversationSessionDto>>("/conversations", {
      method: "POST",
      body: JSON.stringify({
        topicId,
        englishLevel,
        targetObjective,
      }),
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to start conversation session.");
    }

    return response.data;
  },

  async sendMessage(sessionId: string, message: string): Promise<SendMessageResponseDto> {
    const response = await apiRequest<ApiResult<SendMessageResponseDto>>(`/conversations/${sessionId}/messages`, {
      method: "POST",
      body: JSON.stringify({
        message,
      }),
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to send message to AI tutor.");
    }

    return response.data;
  },

  async sendSpeech(
    sessionId: string,
    audioBlob: Blob,
    clientTranscript?: string
  ): Promise<SpeechConversationResponseDto> {
    const formData = new FormData();
    const extension = audioBlob.type.includes("mp4") ? "mp4" : audioBlob.type.includes("wav") ? "wav" : "webm";
    formData.append("audio", audioBlob, `recording_${Date.now()}.${extension}`);
    if (clientTranscript && clientTranscript.trim()) {
      formData.append("clientTranscript", clientTranscript.trim());
    }

    const response = await apiRequest<ApiResult<SpeechConversationResponseDto>>(`/conversations/${sessionId}/speech`, {
      method: "POST",
      body: formData,
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to process audio speech input.");
    }

    return response.data;
  },

  async getConversation(sessionId: string): Promise<ConversationSessionDto> {
    const response = await apiRequest<ApiResult<ConversationSessionDto>>(`/conversations/${sessionId}`, {
      method: "GET",
    });

    if (!response.data) {
      throw new Error("Unable to retrieve conversation session.");
    }

    return response.data;
  },

  async endConversation(sessionId: string): Promise<ConversationSessionDto> {
    const response = await apiRequest<ApiResult<ConversationSessionDto>>(`/conversations/${sessionId}/end`, {
      method: "POST",
    });

    if (!response.data) {
      throw new Error(response.message || "Failed to end conversation session.");
    }

    return response.data;
  },

  async getActiveTopics(): Promise<TopicDto[]> {
    const response = await apiRequest<ApiResult<TopicDto[]>>("/topics", {
      method: "GET",
    });

    return response.data || [];
  },
};
