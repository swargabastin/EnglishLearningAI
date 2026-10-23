"use client";

import React from "react";
import { Mic, Square, Loader2, Sparkles, AlertCircle, RefreshCw, MessageSquare, Volume2 } from "lucide-react";
import { AudioWaveformMock } from "./AudioWaveformMock";
import { useAudioRecorder } from "@/hooks/useAudioRecorder";

export type SpeechPracticeState = "idle" | "listening" | "processing" | "ai_responding" | "error";

export interface InteractiveMicControlProps {
  speechState: SpeechPracticeState;
  onSendSpeechBlob: (audioBlob: Blob, liveTranscript?: string) => Promise<void>;
  onSwitchToKeyboard?: () => void;
  onRetry?: () => void;
  onBargeIn?: () => void;
  errorMessage?: string | null;
  onClearError?: () => void;
  disabled?: boolean;
}

export function InteractiveMicControl({
  speechState,
  onSendSpeechBlob,
  onSwitchToKeyboard,
  onRetry,
  onBargeIn,
  errorMessage: parentError,
  onClearError,
  disabled = false,
}: InteractiveMicControlProps) {
  const isAiResponding = speechState === "ai_responding";
  const isProcessing = speechState === "processing";

  const {
    state: recorderState,
    errorMessage: recorderError,
    recordingDurationSec,
    liveTranscript,
    isHandsFree,
    setIsHandsFree,
    startRecording,
    stopRecording,
    clearError,
    isSupported,
  } = useAudioRecorder({
    isAiSpeaking: isAiResponding,
    onBargeIn,
    onAutoSubmitTurn: async (blob, transcript) => {
      await onSendSpeechBlob(blob, transcript);
    },
  });

  const isRecording = recorderState === "recording";
  const hasError = speechState === "error" || Boolean(parentError || recorderError);
  const currentError = parentError || recorderError;

  // In hands-free mode: auto-rearm microphone once AI finishes speaking
  React.useEffect(() => {
    if (isHandsFree && !isAiResponding && !isProcessing && recorderState === "idle" && !hasError) {
      const timer = setTimeout(() => {
        startRecording().catch(() => {});
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [isHandsFree, isAiResponding, isProcessing, recorderState, hasError, startRecording]);

  const handleMicClick = async () => {
    if (disabled || isProcessing || isAiResponding) return;

    if (hasError) {
      clearError();
      onClearError?.();
      onRetry?.();
      return;
    }

    if (!isRecording) {
      await startRecording();
    } else {
      const result = await stopRecording();
      if (result.blob) {
        await onSendSpeechBlob(result.blob, result.transcript);
      }
    }
  };

  const formatDuration = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, "0")}:${secs.toString().padStart(2, "0")}`;
  };

  return (
    <div className="bg-slate-900/90 border border-slate-800 rounded-3xl p-5 sm:p-6 shadow-xl max-w-xl mx-auto w-full text-center space-y-4 transition-all">
      {/* Dynamic Status Header */}
      <div className="space-y-1">
        {!isRecording && !isProcessing && !isAiResponding && !hasError && (
          <div className="inline-flex items-center gap-1.5 px-3 py-1 bg-purple-950/60 text-purple-300 border border-purple-800/50 rounded-full text-xs font-bold uppercase tracking-wider">
            <span className="w-2 h-2 rounded-full bg-purple-400 animate-pulse" />
            <span>Ready to Speak</span>
          </div>
        )}

        {isRecording && (
          <div className="inline-flex items-center gap-2 px-3.5 py-1 bg-red-950/60 text-red-400 border border-red-800/60 rounded-full text-xs font-bold uppercase tracking-wider animate-pulse">
            <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
            <span>Listening... ({formatDuration(recordingDurationSec)})</span>
          </div>
        )}

        {isProcessing && (
          <div className="inline-flex items-center gap-2 px-3.5 py-1 bg-purple-950/60 text-purple-300 border border-purple-800/50 rounded-full text-xs font-bold uppercase tracking-wider">
            <Loader2 className="w-3.5 h-3.5 animate-spin text-purple-400" />
            <span>Understanding your speech...</span>
          </div>
        )}

        {isAiResponding && (
          <div className="inline-flex items-center gap-2 px-3.5 py-1 bg-indigo-950/60 text-indigo-300 border border-indigo-800/50 rounded-full text-xs font-bold uppercase tracking-wider">
            <Volume2 className="w-3.5 h-3.5 animate-pulse text-indigo-400" />
            <span>Emma is speaking...</span>
          </div>
        )}

        {hasError && (
          <div className="inline-flex items-center gap-2 px-3.5 py-1 bg-rose-950/60 text-rose-300 border border-rose-800/60 rounded-full text-xs font-bold uppercase tracking-wider">
            <AlertCircle className="w-3.5 h-3.5 text-rose-400" />
            <span>Voice Notice</span>
          </div>
        )}

        <div className="pt-0.5">
          {!isRecording && !isProcessing && !isAiResponding && !hasError && (
            <h3 className="text-sm sm:text-base font-bold text-slate-200">
              Tap the microphone to speak your response
            </h3>
          )}

          {isRecording && (
            <div className="space-y-1.5">
              <h3 className="text-xs font-semibold uppercase tracking-wider text-purple-400">
                Live Speech Recognition
              </h3>
              <div className="p-3 bg-purple-950/40 border border-purple-800/40 rounded-2xl min-h-[48px] flex items-center justify-center">
                {liveTranscript ? (
                  <p className="text-sm sm:text-base font-bold text-white tracking-wide animate-fade-in">
                    &ldquo;{liveTranscript}&rdquo;
                  </p>
                ) : (
                  <p className="text-xs sm:text-sm text-slate-400 italic">
                    Listening for your voice... speak naturally in English
                  </p>
                )}
              </div>
            </div>
          )}

          {isProcessing && (
            <div className="space-y-1">
              <h3 className="text-sm sm:text-base font-bold text-purple-300">
                Evaluating with Emma...
              </h3>
              {liveTranscript && (
                <p className="text-xs text-slate-400 truncate max-w-md mx-auto">
                  &ldquo;{liveTranscript}&rdquo;
                </p>
              )}
            </div>
          )}

          {isAiResponding && (
            <h3 className="text-sm sm:text-base font-bold text-indigo-300">
              Listen to Emma&apos;s spoken response
            </h3>
          )}

          {hasError && (
            <h3 className="text-xs sm:text-sm font-semibold text-rose-300">
              {typeof currentError === "string" && currentError.includes("OPENAI_API_KEY")
                ? "Configure OPENAI_API_KEY for live turns or speak clearly into the microphone."
                : typeof currentError === "string"
                ? currentError
                : "Speech processing encountered an issue"}
            </h3>
          )}
        </div>
      </div>

      {/* Decorative Waveform Indicator */}
      <AudioWaveformMock
        isActive={isRecording || isAiResponding}
        variant={isAiResponding ? "ai" : "user"}
      />

      {/* Primary Centerpiece Microphone Button */}
      <div className="flex flex-col items-center justify-center pt-0.5 gap-3">
        <div className="relative">
          {/* Animated Pulsing Ring when Recording */}
          {isRecording && (
            <span className="absolute -inset-3 rounded-full bg-red-500/20 animate-ping pointer-events-none" />
          )}

          <button
            type="button"
            onClick={handleMicClick}
            disabled={disabled || isProcessing || isAiResponding}
            aria-label={
              hasError
                ? "Retry voice input"
                : isRecording
                ? "Stop voice recording and send"
                : "Start speaking into microphone"
            }
            className={`w-20 h-20 sm:w-24 sm:h-24 rounded-full flex items-center justify-center transition-all duration-300 shadow-2xl relative z-10 ${
              hasError
                ? "bg-rose-950/80 border-2 border-rose-500 text-rose-300 hover:bg-rose-900"
                : isRecording
                ? "bg-red-600 hover:bg-red-500 text-white shadow-red-900/60 scale-105"
                : isProcessing || isAiResponding
                ? "bg-purple-950/60 border border-purple-800/60 text-purple-400 cursor-wait animate-pulse"
                : "bg-gradient-to-tr from-purple-700 via-indigo-600 to-purple-500 hover:from-purple-600 hover:to-indigo-500 text-white shadow-purple-950/60 hover:scale-105 active:scale-95 ring-4 ring-purple-500/20"
            }`}
          >
            {hasError ? (
              <RefreshCw className="w-8 h-8 text-rose-400" />
            ) : isRecording ? (
              <Square className="w-8 h-8 fill-current text-white animate-pulse" />
            ) : isProcessing ? (
              <Loader2 className="w-8 h-8 animate-spin text-purple-300" />
            ) : isAiResponding ? (
              <Sparkles className="w-8 h-8 text-indigo-300 animate-spin" />
            ) : (
              <Mic className="w-9 h-9 sm:w-10 sm:h-10 text-white" />
            )}
          </button>
        </div>

        {/* Action Button Label / Microcopy */}
        <p className="text-xs font-semibold text-slate-400 tracking-wide">
          {hasError
            ? "Tap to retry"
            : isHandsFree
            ? isRecording
              ? "Hands-free listening (pausing auto-sends)..."
              : isAiResponding
              ? "Emma is speaking (speak to interrupt)..."
              : "Hands-free ready (start speaking)..."
            : isRecording
            ? "Tap to Stop & Send"
            : isProcessing
            ? "Processing response..."
            : isAiResponding
            ? "Emma is responding..."
            : "Tap to Speak"}
        </p>

        {/* Full-Duplex Hands-Free Mode Toggle */}
        <div className="flex items-center justify-center gap-2 pt-1">
          <button
            type="button"
            onClick={() => setIsHandsFree(!isHandsFree)}
            className={`inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full text-xs font-bold transition-all ${
              isHandsFree
                ? "bg-emerald-950/70 border border-emerald-500/60 text-emerald-300 ring-1 ring-emerald-500/30 shadow-md"
                : "bg-slate-800/60 border border-slate-700/60 text-slate-400 hover:text-white"
            }`}
          >
            <span
              className={`w-2 h-2 rounded-full ${
                isHandsFree ? "bg-emerald-400 animate-pulse" : "bg-slate-500"
              }`}
            />
            <span>Live Hands-Free Duplex: {isHandsFree ? "ON" : "OFF"}</span>
          </button>
        </div>

        {/* Secondary Error Recovery Actions */}
        {hasError && (
          <div className="flex flex-col items-center gap-3 pt-1 w-full animate-in fade-in duration-200">
            <div className="flex items-center justify-center gap-2">
              <button
                type="button"
                onClick={() => {
                  clearError();
                  onClearError?.();
                  onRetry?.();
                }}
                className="inline-flex items-center space-x-1.5 px-3 py-1.5 bg-slate-800 hover:bg-slate-750 text-slate-300 rounded-xl text-xs font-bold transition-colors"
              >
                <RefreshCw className="w-3.5 h-3.5" />
                <span>Try Again</span>
              </button>
              {onSwitchToKeyboard && (
                <button
                  type="button"
                  onClick={onSwitchToKeyboard}
                  className="inline-flex items-center space-x-1.5 px-3 py-1.5 bg-purple-950/60 border border-purple-800/50 hover:bg-purple-900/50 text-purple-300 rounded-xl text-xs font-bold transition-colors"
                >
                  <MessageSquare className="w-3.5 h-3.5" />
                  <span>Use Keyboard</span>
                </button>
              )}
            </div>

            {/* Practical Troubleshooting Guide */}
            <div className="w-full max-w-md text-left bg-slate-950/70 border border-slate-800 rounded-2xl p-3.5 space-y-1.5 text-xs">
              <p className="font-bold text-slate-200 flex items-center gap-1.5 text-[11px]">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-400" />
                <span>How to resolve microphone detection:</span>
              </p>
              <ul className="text-slate-400 space-y-1 pl-4 list-disc text-[11px] leading-relaxed">
                <li><strong>USB Mic / Headset:</strong> Try unplugging and re-inserting firmly into a direct PC USB port.</li>
                <li><strong>3.5mm Headset:</strong> Open <em>Waves MaxxAudio Pro</em> or Realtek Audio on Windows and choose <em>Headset</em> (not just Headphones).</li>
                <li><strong>Browser Permission:</strong> Click the lock/site permissions icon beside the URL in your address bar and ensure <em>Microphone</em> is set to <em>Allow</em>.</li>
              </ul>
            </div>
          </div>
        )}

        {!isSupported && (
          <p className="text-xs text-rose-400 font-semibold max-w-xs">
            Browser does not support audio recording. Please use modern Chrome, Edge, or Safari.
          </p>
        )}
      </div>

      {/* Powered by Whisper Footer Notice */}
      <div className="pt-2 border-t border-slate-800/80">
        <p className="text-[11px] text-slate-400 font-medium tracking-wide">
          Powered by OpenAI Whisper Speech-to-Text
        </p>
      </div>
    </div>
  );
}
