"use client";

import React, { useState, useEffect, useRef, useCallback, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { AppShell } from "@/components/layout/AppShell";
import { TutorHeader } from "@/components/practice/TutorHeader";
import { ConversationFeed } from "@/components/practice/ConversationFeed";
import { TextConversationInput, ConversationState } from "@/components/practice/TextConversationInput";
import { InteractiveMicControl, SpeechPracticeState } from "@/components/practice/InteractiveMicControl";
import { conversationService, ConversationSessionDto } from "@/services/conversationService";
import { useAuth } from "@/context/AuthContext";
import { mockTopics } from "@/lib/mock";
import { ConversationMessage } from "@/types";
import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { CheckCircle2, Home, Sparkles, Mic, MessageSquare, Loader2, AlertCircle, RefreshCw, Bot } from "lucide-react";

function PracticeContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user } = useAuth();

  const sessionIdParam = searchParams.get("session");
  const topicIdParam = searchParams.get("topic") || searchParams.get("topicId");
  const objectiveParam = searchParams.get("objective");

  const [inputMode, setInputMode] = useState<"speech" | "text">("speech");
  const [session, setSession] = useState<ConversationSessionDto | null>(null);
  const [messages, setMessages] = useState<ConversationMessage[]>([]);
  const [textState, setTextState] = useState<ConversationState>("ready");
  const [speechState, setSpeechState] = useState<SpeechPracticeState>("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [lastUserMessage, setLastUserMessage] = useState<string>("");
  const [showEndModal, setShowEndModal] = useState<boolean>(false);
  const [isLoadingSession, setIsLoadingSession] = useState<boolean>(true);

  const feedBottomRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    feedBottomRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, textState, speechState]);

  const mapErrorMessage = (rawError: string): string => {
    if (rawError.includes("OPENAI_API_KEY") || rawError.includes("AI conversation is currently unavailable")) {
      return "Emma AI is currently waiting for local OPENAI_API_KEY configuration. Configure your API key for live voice turns or use test mode.";
    }
    if (rawError.includes("validation") || rawError.includes("TopicId")) {
      return "Some practice topic information was missing. A valid topic has been selected.";
    }
    if (rawError.includes("401") || rawError.includes("Unauthorized")) {
      return "Your authentication session has expired. Please log in again.";
    }
    if (rawError.includes("Network") || rawError.includes("Failed to fetch")) {
      return "Unable to reach the backend learning service. Please verify that the API server is active.";
    }
    return rawError;
  };

  const loadSession = useCallback(async () => {
    setIsLoadingSession(true);
    setErrorMessage(null);

    try {
      if (sessionIdParam) {
        const loadedSession = await conversationService.getConversation(sessionIdParam);
        setSession(loadedSession);
        if (loadedSession.messages && loadedSession.messages.length > 0) {
          setMessages(
            loadedSession.messages.map((m) => ({
              id: m.id,
              role: m.role as "user" | "assistant",
              content: m.messageText,
              timestamp: new Date(m.createdAt).toLocaleTimeString([], {
                hour: "2-digit",
                minute: "2-digit",
              }),
              audioDurationSec: 5,
            }))
          );
        }
        if (loadedSession.status === "Completed") {
          setTextState("completed");
        }
      } else {
        const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
        const legacyTopicMap: Record<string, string> = {
          "topic-1": "11111111-1111-1111-1111-111111111111",
          "topic-2": "22222222-2222-2222-2222-222222222222",
          "topic-3": "33333333-3333-3333-3333-333333333333",
          "topic-4": "44444444-4444-4444-4444-444444444444",
          "topic-5": "55555555-5555-5555-5555-555555555555",
          "topic-6": "66666666-6666-6666-6666-666666666666",
          "topic-7": "77777777-7777-7777-7777-777777777777",
        };

        let targetTopicId = "22222222-2222-2222-2222-222222222222";
        if (topicIdParam) {
          if (GUID_REGEX.test(topicIdParam)) {
            targetTopicId = topicIdParam;
          } else if (legacyTopicMap[topicIdParam]) {
            targetTopicId = legacyTopicMap[topicIdParam];
          }
        }

        const newSession = await conversationService.startConversation(
          targetTopicId,
          undefined,
          objectiveParam || undefined
        );
        setSession(newSession);
        if (newSession.messages && newSession.messages.length > 0) {
          setMessages(
            newSession.messages.map((m) => ({
              id: m.id,
              role: m.role as "user" | "assistant",
              content: m.messageText,
              timestamp: new Date(m.createdAt).toLocaleTimeString([], {
                hour: "2-digit",
                minute: "2-digit",
              }),
              audioDurationSec: 5,
            }))
          );
        }
      }
    } catch (err) {
      const msg = mapErrorMessage((err as Error).message || "Failed to initialize conversation session.");
      setErrorMessage(msg);
    } finally {
      setIsLoadingSession(false);
    }
  }, [sessionIdParam, topicIdParam, objectiveParam]);

  useEffect(() => {
    loadSession();
  }, [loadSession]);

  const handleSendMessage = async (text: string) => {
    if (!session?.id) {
      setErrorMessage("No active conversation session found. Please reload.");
      return;
    }

    setLastUserMessage(text);
    setTextState("sending");
    setErrorMessage(null);

    const tempUserMsg: ConversationMessage = {
      id: `user-${Date.now()}`,
      role: "user",
      content: text,
      timestamp: new Date().toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
      }),
      audioDurationSec: 3,
    };
    setMessages((prev) => [...prev, tempUserMsg]);

    try {
      setTextState("ai_responding");
      const resp = await conversationService.sendMessage(session.id, text);

      const aiMsg: ConversationMessage = {
        id: resp.aiMessage.id,
        role: "assistant",
        content: resp.aiMessage.messageText,
        timestamp: new Date(resp.aiMessage.createdAt).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
        audioDurationSec: 5,
        pedagogicalTip: resp.pedagogicalTip,
      };

      setMessages((prev) => [...prev, aiMsg]);
      setTextState("ready");
    } catch (err) {
      setTextState("error");
      const msg = mapErrorMessage((err as Error).message || "Failed to send message to Emma.");
      setErrorMessage(msg);
    }
  };

  const speakEmmaResponse = (text: string, onEnd?: () => void) => {
    if (typeof window !== "undefined" && "speechSynthesis" in window) {
      try {
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = "en-US";
        utterance.rate = 1.0;
        const voices = window.speechSynthesis.getVoices();
        const preferred =
          voices.find(
            (v) =>
              v.lang.startsWith("en") &&
              (v.name.includes("Natural") ||
                v.name.includes("Samantha") ||
                v.name.includes("Google US") ||
                v.name.includes("Female") ||
                v.name.includes("Zira"))
          ) || voices.find((v) => v.lang.startsWith("en"));
        if (preferred) {
          utterance.voice = preferred;
        }
        utterance.onend = () => onEnd?.();
        utterance.onerror = () => onEnd?.();
        window.speechSynthesis.speak(utterance);
        return;
      } catch {
        // Speech synthesis fallback
      }
    }
    onEnd?.();
  };

  const handleSendSpeechBlob = async (audioBlob: Blob, liveTranscript?: string) => {
    let activeSession = session;
    if (!activeSession?.id) {
      try {
        activeSession = await conversationService.startConversation("22222222-2222-2222-2222-222222222222");
        setSession(activeSession);
      } catch {
        setErrorMessage("Please select a topic or log in to practice.");
        return;
      }
    }

    setSpeechState("processing");
    setErrorMessage(null);

    try {
      const speechResp = await conversationService.sendSpeech(activeSession.id, audioBlob);

      const recognized =
        liveTranscript?.trim() ||
        speechResp.recognizedText ||
        speechResp.userMessage?.messageText ||
        "Spoken turn";

      const userMsg: ConversationMessage = {
        id: speechResp.userMessage?.id || `user-speech-${Date.now()}`,
        role: "user",
        content: recognized,
        timestamp: new Date(speechResp.userMessage?.createdAt || Date.now()).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
        audioDurationSec: 4,
      };

      const aiMsg: ConversationMessage = {
        id: speechResp.aiMessage?.id || `ai-speech-${Date.now()}`,
        role: "assistant",
        content: speechResp.aiMessage?.messageText || "",
        timestamp: new Date(speechResp.aiMessage?.createdAt || Date.now()).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
        audioBase64: speechResp.audioBase64,
        audioDurationSec: 6,
        pedagogicalTip: speechResp.pedagogicalTip,
      };

      setMessages((prev) => [...prev, userMsg, aiMsg]);

      const isBase64Dummy =
        !speechResp.audioBase64 ||
        speechResp.audioBase64.startsWith("//uQZAAAA") ||
        speechResp.audioBase64.length < 200;

      if (speechResp.audioBase64 && !isBase64Dummy) {
        setSpeechState("ai_responding");
        const audioUrl = `data:audio/mpeg;base64,${speechResp.audioBase64}`;
        const audio = new Audio(audioUrl);
        audio.onended = () => {
          setSpeechState("idle");
        };
        audio.onerror = () => {
          speakEmmaResponse(aiMsg.content, () => setSpeechState("idle"));
        };
        audio.play().catch(() => {
          speakEmmaResponse(aiMsg.content, () => setSpeechState("idle"));
        });
      } else {
        setSpeechState("ai_responding");
        speakEmmaResponse(aiMsg.content, () => setSpeechState("idle"));
      }
    } catch (err) {
      setSpeechState("error");
      const msg = mapErrorMessage((err as Error).message || "Speech processing encountered an error.");
      setErrorMessage(msg);
    }
  };

  const handleEndSession = async () => {
    if (!session?.id) {
      router.push("/dashboard");
      return;
    }

    try {
      await conversationService.endConversation(session.id);
      router.push(`/practice/result?session=${session.id}`);
    } catch {
      setShowEndModal(true);
    }
  };

  const isBusy =
    textState === "sending" ||
    textState === "ai_responding" ||
    speechState === "processing" ||
    speechState === "ai_responding";

  const isInputDisabled = isLoadingSession || (!session?.id && !errorMessage) || textState === "completed";

  return (
    <AppShell>
      <div className="space-y-4 sm:space-y-5 max-w-3xl mx-auto pb-6">
        {/* Emma AI Coach Header Banner */}
        <TutorHeader
          topicTitle={session?.topicName || "Job Interview Practice"}
          englishLevel={session?.englishLevel || user?.level || "B1"}
          onEndSession={handleEndSession}
          messageCount={messages.length}
        />

        {/* User-Friendly Notification Banner */}
        {errorMessage && (
          <div className="bg-purple-950/60 border border-purple-800/60 rounded-3xl p-4 sm:p-5 flex items-start justify-between gap-3 text-purple-200 shadow-xl animate-in fade-in">
            <div className="flex items-start gap-3">
              <AlertCircle className="w-5 h-5 text-purple-400 flex-shrink-0 mt-0.5" />
              <div className="space-y-1">
                <p className="text-xs font-bold uppercase tracking-wider text-purple-300">
                  Practice Notice
                </p>
                <p className="text-xs sm:text-sm font-medium leading-relaxed">
                  {errorMessage}
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-2 flex-shrink-0">
              <button
                type="button"
                onClick={loadSession}
                className="inline-flex items-center space-x-1 text-xs font-bold text-white bg-purple-600 hover:bg-purple-500 px-3 py-1.5 rounded-xl transition-colors shadow-md"
              >
                <RefreshCw className="w-3.5 h-3.5" />
                <span>Retry</span>
              </button>
              <button
                type="button"
                onClick={() => setErrorMessage(null)}
                className="text-xs font-bold text-slate-400 hover:text-white p-1"
                aria-label="Dismiss notice"
              >
                ✕
              </button>
            </div>
          </div>
        )}

        {/* Active Curriculum Objective Banner if launched from Study Plan */}
        {objectiveParam && (
          <div className="p-3.5 bg-gradient-to-r from-indigo-950/70 via-purple-950/60 to-slate-900 border border-indigo-500/30 rounded-2xl flex items-center justify-between gap-3 text-xs shadow-md">
            <div className="flex items-center space-x-2.5">
              <span className="px-2.5 py-0.5 rounded-full bg-indigo-500/30 text-indigo-300 font-bold uppercase text-[10px] tracking-wider border border-indigo-500/40">
                Phase 4 Speaking
              </span>
              <span className="text-white font-semibold">{objectiveParam}</span>
            </div>
            <span className="text-[11px] text-indigo-300 hidden sm:inline font-medium">
              Emma is coaching your usage of this rule
            </span>
          </div>
        )}

        {/* Main Conversational Workspace */}
        <div className="bg-slate-900/90 rounded-3xl border border-slate-800 p-4 sm:p-6 min-h-[340px] max-h-[480px] overflow-y-auto flex flex-col justify-between shadow-xl">
          {isLoadingSession ? (
            <div className="flex-1 flex flex-col items-center justify-center py-12 space-y-3 text-center">
              <div className="w-12 h-12 rounded-2xl bg-purple-950/60 border border-purple-800/50 text-purple-400 flex items-center justify-center ring-4 ring-purple-500/20 animate-pulse">
                <Loader2 className="w-6 h-6 animate-spin" />
              </div>
              <p className="text-sm font-bold text-white">Connecting with Emma...</p>
              <p className="text-xs text-slate-400 max-w-xs leading-relaxed">
                Loading your practice scenario and preparing tailored conversation context
              </p>
            </div>
          ) : messages.length === 0 ? (
            /* Warm Empty Onboarding State */
            <div className="flex-1 flex flex-col items-center justify-center py-8 sm:py-10 space-y-4 text-center">
              <div className="relative">
                <div className="w-16 h-16 rounded-3xl bg-gradient-to-br from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-xl ring-4 ring-purple-500/20">
                  <Bot className="w-8 h-8" />
                </div>
                <span className="absolute -bottom-1 -right-1 w-4 h-4 rounded-full bg-emerald-400 ring-2 ring-slate-900 animate-pulse" />
              </div>

              <div className="space-y-1.5 max-w-md">
                <h3 className="text-base sm:text-lg font-extrabold text-white">
                  Ready when you are
                </h3>
                <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
                  Let&apos;s practice speaking about{" "}
                  <span className="font-bold text-purple-300">
                    {session?.topicName || "Job Interview Practice"}
                  </span>
                  . Start with a short answer and Emma will guide you naturally.
                </p>
              </div>

              <div className="flex flex-wrap items-center justify-center gap-2.5 pt-2">
                <button
                  type="button"
                  onClick={() => setInputMode("speech")}
                  className="inline-flex items-center space-x-2 px-5 py-2.5 bg-purple-600 hover:bg-purple-500 text-white rounded-2xl text-xs font-bold shadow-lg shadow-purple-950/50 transition-all hover:scale-102"
                >
                  <Mic className="w-4 h-4" />
                  <span>Start Speaking</span>
                </button>
                <button
                  type="button"
                  onClick={() => setInputMode("text")}
                  className="inline-flex items-center space-x-2 px-5 py-2.5 bg-slate-800 border border-slate-700 hover:bg-slate-750 text-slate-200 rounded-2xl text-xs font-bold transition-all"
                >
                  <MessageSquare className="w-4 h-4" />
                  <span>Use Keyboard</span>
                </button>
              </div>
            </div>
          ) : (
            <ConversationFeed messages={messages} isAiResponding={isBusy} />
          )}
          <div ref={feedBottomRef} />
        </div>

        {/* Input Mode Selector Bar */}
        <div className="flex items-center justify-between bg-slate-900/90 border border-slate-800 rounded-2xl px-4 py-2 shadow-lg">
          <span className="text-xs font-semibold text-slate-400">Practice Mode:</span>
          <div className="flex items-center space-x-1.5">
            <button
              type="button"
              onClick={() => setInputMode("speech")}
              className={`inline-flex items-center space-x-1.5 px-3 py-1.5 rounded-xl text-xs font-bold transition-all ${
                inputMode === "speech"
                  ? "bg-purple-600 text-white shadow-md shadow-purple-950/40"
                  : "bg-slate-800 text-slate-400 hover:bg-slate-750 hover:text-slate-200"
              }`}
            >
              <Mic className="w-3.5 h-3.5" />
              <span>Voice</span>
            </button>
            <button
              type="button"
              onClick={() => setInputMode("text")}
              className={`inline-flex items-center space-x-1.5 px-3 py-1.5 rounded-xl text-xs font-bold transition-all ${
                inputMode === "text"
                  ? "bg-purple-600 text-white shadow-md shadow-purple-950/40"
                  : "bg-slate-800 text-slate-400 hover:bg-slate-750 hover:text-slate-200"
              }`}
            >
              <MessageSquare className="w-3.5 h-3.5" />
              <span>Text</span>
            </button>
          </div>
        </div>

        {/* Dynamic Controls based on selected mode */}
        {inputMode === "speech" ? (
          <InteractiveMicControl
            speechState={speechState}
            onSendSpeechBlob={handleSendSpeechBlob}
            onSwitchToKeyboard={() => setInputMode("text")}
            onRetry={loadSession}
            errorMessage={errorMessage}
            onClearError={() => setErrorMessage(null)}
            disabled={isInputDisabled}
          />
        ) : (
          <TextConversationInput
            state={textState}
            errorMessage={errorMessage}
            onSendMessage={handleSendMessage}
            onRetryLastMessage={() => lastUserMessage && handleSendMessage(lastUserMessage)}
            disabled={isInputDisabled}
          />
        )}
      </div>

      {/* Exit & Completion Fallback Modal */}
      <Modal
        isOpen={showEndModal}
        onClose={() => setShowEndModal(false)}
        title="Speaking Session Finished"
        description="Great work practicing your spoken English with Emma!"
      >
        <div className="space-y-4 text-center py-4">
          <div className="w-14 h-14 rounded-3xl bg-purple-950/60 border border-purple-800/40 text-purple-400 flex items-center justify-center mx-auto">
            <CheckCircle2 className="w-8 h-8" />
          </div>
          <p className="text-xs sm:text-sm text-slate-300 max-w-sm mx-auto">
            Your dialogue turns have been securely saved to your learning history.
          </p>
          <div className="flex justify-center gap-3 pt-2">
            <Button
              variant="primary"
              size="md"
              className="bg-purple-600 hover:bg-purple-500"
              onClick={() => router.push("/dashboard")}
              leftIcon={<Home className="w-4 h-4" />}
            >
              Return to Dashboard
            </Button>
          </div>
        </div>
      </Modal>
    </AppShell>
  );
}

export default function PracticePage() {
  return (
    <Suspense
      fallback={
        <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center space-y-3">
          <Loader2 className="w-8 h-8 animate-spin text-purple-500" />
          <p className="text-xs font-semibold text-slate-400">Loading Practice Room...</p>
        </div>
      }
    >
      <PracticeContent />
    </Suspense>
  );
}
