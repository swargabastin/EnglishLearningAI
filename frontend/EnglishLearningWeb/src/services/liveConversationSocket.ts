import * as signalR from "@microsoft/signalr";
import { SpeechConversationResponseDto } from "./conversationService";

export type LiveSocketState = "disconnected" | "connecting" | "connected" | "error";

export interface LiveSocketCallbacks {
  onAiThinking?: () => void;
  onAudioProcessing?: () => void;
  onLiveTurnComplete?: (result: SpeechConversationResponseDto) => void;
  onInterruptionAcknowledged?: () => void;
  onError?: (errorMessage: string) => void;
  onStateChange?: (state: LiveSocketState) => void;
}

class LiveConversationSocketService {
  private hubConnection: signalR.HubConnection | null = null;
  private currentSessionId: string | null = null;
  private callbacks: LiveSocketCallbacks = {};

  public async connect(
    sessionId: string,
    callbacks: LiveSocketCallbacks = {}
  ): Promise<boolean> {
    this.callbacks = callbacks;
    this.currentSessionId = sessionId;

    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke("JoinSession", sessionId).catch(() => {});
      return true;
    }

    const apiBase =
      process.env.NEXT_PUBLIC_API_URL ||
      process.env.NEXT_PUBLIC_API_BASE_URL ||
      "http://localhost:5288";

    const hubUrl = `${apiBase.replace(/\/api\/v1\/?$/, "")}/hubs/conversation`;

    try {
      this.callbacks.onStateChange?.("connecting");

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => {
            if (typeof window !== "undefined") {
              return localStorage.getItem("el_access_token") || "";
            }
            return "";
          },
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect([0, 1000, 3000, 5000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      this.hubConnection.on("OnAiThinking", () => {
        this.callbacks.onAiThinking?.();
      });

      this.hubConnection.on("OnAudioProcessing", () => {
        this.callbacks.onAudioProcessing?.();
      });

      this.hubConnection.on("OnLiveTurnComplete", (result: SpeechConversationResponseDto) => {
        this.callbacks.onLiveTurnComplete?.(result);
      });

      this.hubConnection.on("OnInterruptionAcknowledged", () => {
        this.callbacks.onInterruptionAcknowledged?.();
      });

      this.hubConnection.on("OnError", (err: string) => {
        this.callbacks.onError?.(err);
      });

      this.hubConnection.onclose(() => {
        this.callbacks.onStateChange?.("disconnected");
      });

      this.hubConnection.onreconnected(() => {
        this.callbacks.onStateChange?.("connected");
        if (this.currentSessionId) {
          this.hubConnection?.invoke("JoinSession", this.currentSessionId).catch(() => {});
        }
      });

      await this.hubConnection.start();
      await this.hubConnection.invoke("JoinSession", sessionId);
      this.callbacks.onStateChange?.("connected");
      return true;
    } catch (err) {
      console.warn("SignalR live connection error (falling back to REST):", err);
      this.callbacks.onStateChange?.("error");
      return false;
    }
  }

  public async sendLiveAudioTurn(base64Audio: string, clientTranscript?: string): Promise<boolean> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected || !this.currentSessionId) {
      return false;
    }

    try {
      await this.hubConnection.invoke("SendLiveAudioTurn", this.currentSessionId, base64Audio, clientTranscript || null);
      return true;
    } catch (err) {
      console.warn("Failed to send live audio turn over WebSocket:", err);
      return false;
    }
  }

  public async interruptAi(): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected && this.currentSessionId) {
      await this.hubConnection.invoke("InterruptAi", this.currentSessionId).catch(() => {});
    }
  }

  public disconnect(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().catch(() => {});
      this.hubConnection = null;
    }
    this.currentSessionId = null;
    this.callbacks.onStateChange?.("disconnected");
  }
}

export const liveConversationSocket = new LiveConversationSocketService();
