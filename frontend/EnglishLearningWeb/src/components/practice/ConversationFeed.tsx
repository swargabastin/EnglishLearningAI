"use client";

import React, { useState, useRef, useEffect } from "react";
import { Sparkles, Play, Pause, Mic, Bot, User as UserIcon, BookmarkPlus, Check } from "lucide-react";
import { ConversationMessage } from "@/types";
import { vocabularyService } from "@/services/vocabularyService";

export interface ConversationFeedProps {
  messages: ConversationMessage[];
  isAiResponding?: boolean;
}

export function ConversationFeed({
  messages,
  isAiResponding = false,
}: ConversationFeedProps) {
  const [playingMessageId, setPlayingMessageId] = useState<string | null>(null);
  const [playbackProgress, setPlaybackProgress] = useState<Record<string, number>>({});
  const [savedTipIds, setSavedTipIds] = useState<Record<string, boolean>>({});
  const audioPlayerRef = useRef<HTMLAudioElement | null>(null);

  useEffect(() => {
    return () => {
      if (audioPlayerRef.current) {
        audioPlayerRef.current.pause();
        audioPlayerRef.current = null;
      }
      if (typeof window !== "undefined" && "speechSynthesis" in window) {
        window.speechSynthesis.cancel();
      }
    };
  }, []);

  const handleSaveTipToVocabulary = async (tipText: string, messageId: string) => {
    try {
      // Extract word or short phrase if in quotes, or save the tip
      const quoteMatch = tipText.match(/"([^"]+)"/);
      const word = quoteMatch ? quoteMatch[1] : tipText.split("—")[0].trim().slice(0, 40);
      const meaning = tipText;

      await vocabularyService.addVocabulary({
        word,
        meaning,
        example: `From practice conversation with Emma: "${word}"`,
      });

      setSavedTipIds((prev) => ({ ...prev, [messageId]: true }));
    } catch {
      // Non-blocking
    }
  };

  const togglePlayAudio = (msg: ConversationMessage) => {
    if (playingMessageId === msg.id) {
      if (audioPlayerRef.current) {
        audioPlayerRef.current.pause();
      }
      if (typeof window !== "undefined" && "speechSynthesis" in window) {
        window.speechSynthesis.cancel();
      }
      setPlayingMessageId(null);
      return;
    }

    if (audioPlayerRef.current) {
      audioPlayerRef.current.pause();
      audioPlayerRef.current = null;
    }

    if (typeof window !== "undefined" && "speechSynthesis" in window) {
      window.speechSynthesis.cancel();
    }

    const isBase64Dummy = !msg.audioBase64 || msg.audioBase64.startsWith("//uQZAAAA") || msg.audioBase64.length < 200;

    const playWithSpeechSynthesis = () => {
      if (typeof window !== "undefined" && "speechSynthesis" in window) {
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(msg.content);
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
        setPlayingMessageId(msg.id);
        utterance.onend = () => {
          setPlayingMessageId(null);
          setPlaybackProgress((prev) => ({ ...prev, [msg.id]: 0 }));
        };
        utterance.onerror = () => {
          setPlayingMessageId(null);
        };
        window.speechSynthesis.speak(utterance);
      } else {
        setPlayingMessageId(null);
      }
    };

    if (msg.audioBase64 && !isBase64Dummy) {
      const audioUrl = `data:audio/mpeg;base64,${msg.audioBase64}`;
      const audio = new Audio(audioUrl);
      audioPlayerRef.current = audio;

      audio.ontimeupdate = () => {
        if (audio.duration) {
          setPlaybackProgress((prev) => ({
            ...prev,
            [msg.id]: (audio.currentTime / audio.duration) * 100,
          }));
        }
      };

      audio.onended = () => {
        setPlayingMessageId(null);
        setPlaybackProgress((prev) => ({ ...prev, [msg.id]: 0 }));
      };

      audio.onerror = () => {
        playWithSpeechSynthesis();
      };

      setPlayingMessageId(msg.id);
      audio.play().catch(() => {
        playWithSpeechSynthesis();
      });
    } else {
      playWithSpeechSynthesis();
    }
  };

  return (
    <div className="space-y-4 sm:space-y-5 py-2 max-w-2xl mx-auto w-full">
      {messages.map((msg) => {
        const isAi = msg.role === "assistant";
        const isPlaying = playingMessageId === msg.id;
        const isSaved = savedTipIds[msg.id];

        return (
          <div
            key={msg.id}
            className={`flex items-start gap-3 animate-in fade-in slide-in-from-bottom-2 duration-200 ${
              isAi ? "justify-start" : "justify-end flex-row-reverse"
            }`}
          >
            {/* Avatar */}
            <div className="flex-shrink-0 mt-0.5">
              {isAi ? (
                <div
                  className={`w-9 h-9 sm:w-10 sm:h-10 rounded-2xl bg-gradient-to-br from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-md ${
                    isPlaying ? "ring-4 ring-purple-500/40 animate-pulse" : "ring-2 ring-purple-500/20"
                  }`}
                >
                  <Bot className="w-5 h-5" />
                </div>
              ) : (
                <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-2xl bg-slate-800 flex items-center justify-center text-white shadow-md ring-2 ring-slate-700">
                  <UserIcon className="w-5 h-5 text-slate-300" />
                </div>
              )}
            </div>

            {/* Message Bubble */}
            <div
              className={`max-w-[85%] sm:max-w-md md:max-w-lg rounded-3xl p-4 sm:p-5 shadow-md space-y-2.5 transition-all ${
                isAi
                  ? "bg-slate-900 border border-slate-800 text-slate-100 rounded-tl-sm"
                  : "bg-gradient-to-r from-purple-600 to-indigo-600 text-white rounded-tr-sm shadow-purple-950/50"
              }`}
            >
              {/* Speaker Label & Timestamp Header */}
              <div className="flex items-center justify-between gap-4 text-[11px]">
                <div className="flex items-center gap-1.5 font-bold tracking-wide">
                  {isAi ? (
                    <>
                      <span className="text-purple-400">Emma</span>
                      <span className="text-slate-400 font-normal">· AI Coach</span>
                    </>
                  ) : (
                    <>
                      <Mic className="w-3 h-3 text-purple-200" />
                      <span className="text-purple-100 font-semibold">You</span>
                      <span className="text-purple-200 font-normal">· Learner</span>
                    </>
                  )}
                </div>
                <span
                  className={
                    isAi
                      ? "text-slate-400 font-mono text-[10px]"
                      : "text-purple-200 font-mono text-[10px]"
                  }
                >
                  {msg.timestamp}
                </span>
              </div>

              {/* Message Content */}
              {!isAi ? (
                <div className="space-y-1">
                  <p className="text-[11px] font-semibold text-purple-200 tracking-wide uppercase">
                    You said:
                  </p>
                  <p className="text-sm sm:text-base leading-relaxed tracking-normal font-medium text-white">
                    &ldquo;{msg.content}&rdquo;
                  </p>
                </div>
              ) : (
                <p className="text-sm sm:text-base leading-relaxed tracking-normal text-slate-200">
                  {msg.content}
                </p>
              )}

              {/* Real-time Pedagogical & Grammar Tip Card */}
              {isAi && msg.pedagogicalTip && (
                <div className="pt-2 mt-2 border-t border-purple-900/40 bg-purple-950/40 -mx-1 p-3 rounded-2xl border border-purple-800/40 space-y-1.5">
                  <div className="flex items-center justify-between text-xs font-bold text-amber-300">
                    <div className="flex items-center gap-1.5">
                      <Sparkles className="w-3.5 h-3.5 text-amber-400" />
                      <span className="tracking-wide">Grammar & Phrasing Tip</span>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleSaveTipToVocabulary(msg.pedagogicalTip!, msg.id)}
                      disabled={isSaved}
                      className={`text-[10px] font-semibold px-2 py-0.5 rounded-lg transition-colors inline-flex items-center gap-1 ${
                        isSaved
                          ? "bg-emerald-950/60 text-emerald-400 border border-emerald-800/50"
                          : "bg-purple-900/60 hover:bg-purple-800/80 text-purple-200 hover:text-white"
                      }`}
                      title="Save word or phrase to your personal Vocabulary Notebook"
                    >
                      {isSaved ? (
                        <>
                          <Check className="w-3 h-3 text-emerald-400" />
                          <span>Saved!</span>
                        </>
                      ) : (
                        <>
                          <BookmarkPlus className="w-3 h-3" />
                          <span>+ Save to Vocab</span>
                        </>
                      )}
                    </button>
                  </div>
                  <p className="text-xs text-slate-300 leading-relaxed font-normal">
                    {msg.pedagogicalTip}
                  </p>
                </div>
              )}

              {/* Inline TTS Audio Player for AI Turns */}
              {isAi && (
                <div className="pt-2 border-t border-slate-800 flex items-center justify-between gap-3">
                  <div className="flex items-center space-x-2">
                    <button
                      type="button"
                      onClick={() => togglePlayAudio(msg)}
                      className="p-1.5 rounded-xl bg-purple-950/60 hover:bg-purple-900/80 text-purple-300 border border-purple-800/50 transition-colors flex items-center justify-center shadow-xs"
                      title={isPlaying ? "Pause audio" : "Listen to Emma speak this turn"}
                    >
                      {isPlaying ? (
                        <Pause className="w-3.5 h-3.5" />
                      ) : (
                        <Play className="w-3.5 h-3.5 fill-purple-300 text-purple-300 ml-0.5" />
                      )}
                    </button>
                    <span className="text-[11px] font-semibold text-slate-400">
                      {isPlaying ? "Emma is speaking..." : "Listen to Emma"}
                    </span>
                  </div>

                  {/* Progress Wave Indicator */}
                  {isPlaying && (
                    <div className="flex items-center space-x-1">
                      <span className="w-1 h-3 bg-purple-400 rounded-full animate-sound-wave" />
                      <span className="w-1 h-5 bg-purple-400 rounded-full animate-sound-wave" style={{ animationDelay: "0.2s" }} />
                      <span className="w-1 h-4 bg-purple-400 rounded-full animate-sound-wave" style={{ animationDelay: "0.4s" }} />
                      <span className="w-1 h-2 bg-purple-400 rounded-full animate-sound-wave" style={{ animationDelay: "0.1s" }} />
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        );
      })}

      {/* AI Generating Indicator Bubble */}
      {isAiResponding && (
        <div className="flex items-start gap-3 animate-in fade-in duration-200">
          <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-2xl bg-gradient-to-br from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-md ring-2 ring-purple-500/20">
            <Bot className="w-5 h-5" />
          </div>
          <div className="bg-slate-900 border border-slate-800 rounded-3xl rounded-tl-sm p-4 text-slate-300 flex items-center space-x-3 shadow-md">
            <div className="flex items-center space-x-1.5">
              <span className="w-2 h-2 rounded-full bg-purple-400 animate-bounce" />
              <span className="w-2 h-2 rounded-full bg-purple-400 animate-bounce" style={{ animationDelay: "0.2s" }} />
              <span className="w-2 h-2 rounded-full bg-purple-400 animate-bounce" style={{ animationDelay: "0.4s" }} />
            </div>
            <span className="text-xs font-semibold text-purple-300">Emma is typing & speaking...</span>
          </div>
        </div>
      )}
    </div>
  );
}
