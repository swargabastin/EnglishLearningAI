"use client";

import React, { useState, useRef, useEffect } from "react";
import { Send, Sparkles, AlertCircle, RefreshCw, MessageSquare } from "lucide-react";
import { Button } from "@/components/ui/Button";

export type ConversationState = "ready" | "sending" | "ai_responding" | "error" | "completed";

export interface TextConversationInputProps {
  state: ConversationState;
  errorMessage?: string | null;
  onSendMessage: (message: string) => Promise<void>;
  onRetryLastMessage?: () => void;
  disabled?: boolean;
}

export function TextConversationInput({
  state,
  errorMessage,
  onSendMessage,
  onRetryLastMessage,
  disabled = false,
}: TextConversationInputProps) {
  const [message, setMessage] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const isBusy = state === "sending" || state === "ai_responding";
  const isCompleted = state === "completed";

  useEffect(() => {
    if (state === "ready" && inputRef.current) {
      inputRef.current.focus();
    }
  }, [state]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!message.trim() || isBusy || isCompleted || disabled) return;

    const textToSend = message.trim();
    setMessage("");
    await onSendMessage(textToSend);
  };

  return (
    <div className="bg-slate-900/90 border border-slate-800 rounded-3xl p-4 sm:p-5 shadow-xl max-w-2xl mx-auto w-full space-y-3">
      {/* Header Info */}
      <div className="flex items-center justify-between text-xs px-1">
        <div className="flex items-center space-x-1.5 text-purple-300 font-semibold">
          <span className="w-2 h-2 rounded-full bg-purple-400 animate-pulse" />
          <span>Interactive Text Mode</span>
        </div>
        <div className="flex items-center space-x-1 text-slate-400 text-[11px]">
          <span>Type complete English sentences & press Enter</span>
        </div>
      </div>

      {/* Error Alert with Retry */}
      {state === "error" && (
        <div className="p-3 bg-red-950/60 border border-red-800/60 rounded-2xl flex items-center justify-between text-xs text-red-300 animate-in fade-in">
          <div className="flex items-center space-x-2">
            <AlertCircle className="w-4 h-4 text-red-400 flex-shrink-0" />
            <span>{errorMessage || "Failed to receive response from Emma."}</span>
          </div>
          {onRetryLastMessage && (
            <button
              type="button"
              onClick={onRetryLastMessage}
              className="inline-flex items-center space-x-1 font-bold text-red-300 hover:text-white bg-slate-800 border border-slate-700 px-2.5 py-1 rounded-xl transition-colors"
            >
              <RefreshCw className="w-3 h-3" />
              <span>Retry</span>
            </button>
          )}
        </div>
      )}

      {/* Form Input */}
      <form onSubmit={handleSubmit} className="flex items-center space-x-2">
        <input
          ref={inputRef}
          type="text"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          placeholder={
            isCompleted
              ? "Session completed. View assessment."
              : isBusy
              ? "Emma is crafting feedback..."
              : "Type your response to Emma in English..."
          }
          disabled={isBusy || isCompleted || disabled}
          className="flex-1 px-4 py-3 bg-slate-850 border border-slate-700 text-white placeholder:text-slate-500 rounded-2xl text-sm focus:outline-none focus:ring-2 focus:ring-purple-500/40 focus:border-purple-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
        />

        <Button
          type="submit"
          variant="primary"
          size="md"
          className="bg-purple-600 hover:bg-purple-500 rounded-2xl px-5"
          disabled={!message.trim() || isBusy || isCompleted || disabled}
          isLoading={isBusy}
          rightIcon={!isBusy ? <Send className="w-4 h-4" /> : undefined}
        >
          Send
        </Button>
      </form>
    </div>
  );
}
