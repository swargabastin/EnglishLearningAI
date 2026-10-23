"use client";

import React, { useState } from "react";
import { PronunciationAssessment } from "@/types";
import { Sparkles, CheckCircle2, AlertCircle, ChevronDown, ChevronUp, Volume2 } from "lucide-react";

export interface PronunciationScoreCardProps {
  pronunciation: PronunciationAssessment;
  compact?: boolean;
}

export function PronunciationScoreCard({ pronunciation, compact = false }: PronunciationScoreCardProps) {
  const [isExpanded, setIsExpanded] = useState<boolean>(!compact);
  const [selectedWordIndex, setSelectedWordIndex] = useState<number | null>(null);

  const getScoreBadgeClass = (score: number) => {
    if (score >= 80) return "bg-emerald-950/80 text-emerald-300 border-emerald-800/60";
    if (score >= 60) return "bg-amber-950/80 text-amber-300 border-amber-800/60";
    return "bg-rose-950/80 text-rose-300 border-rose-800/60";
  };

  const getWordBgClass = (status: string) => {
    if (status === "Good") return "border-emerald-800/50 hover:bg-emerald-950/40 text-emerald-200";
    if (status === "Acceptable") return "border-amber-800/50 hover:bg-amber-950/40 text-amber-200";
    return "border-rose-800/50 hover:bg-rose-950/40 text-rose-200";
  };

  return (
    <div className="mt-2.5 p-3.5 bg-slate-900/90 border border-slate-800 rounded-2xl shadow-md text-left space-y-3 transition-all">
      {/* Header Bar */}
      <div className="flex items-center justify-between gap-2">
        <div className="flex items-center space-x-2">
          <span className="w-2 h-2 rounded-full bg-emerald-400" />
          <span className="text-[11px] font-extrabold uppercase tracking-wider text-slate-300">
            Pronunciation & Phonemes
          </span>
          <span
            className={`px-2 py-0.5 rounded-full border text-[10px] font-bold ${getScoreBadgeClass(
              pronunciation.overallScore
            )}`}
          >
            {Math.round(pronunciation.overallScore)}%
          </span>
        </div>

        <button
          type="button"
          onClick={() => setIsExpanded(!isExpanded)}
          className="text-slate-400 hover:text-white p-1 rounded-lg transition-colors flex items-center text-[10px] gap-1 font-bold"
        >
          <span>{isExpanded ? "Hide" : "Details"}</span>
          {isExpanded ? <ChevronUp className="w-3.5 h-3.5" /> : <ChevronDown className="w-3.5 h-3.5" />}
        </button>
      </div>

      {isExpanded && (
        <div className="space-y-3 pt-1 border-t border-slate-800/80 text-xs">
          {/* Sub-metrics: Accuracy, Fluency, Completeness */}
          <div className="grid grid-cols-3 gap-2 text-center">
            <div className="p-2 bg-slate-850 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 font-medium block">Accuracy</span>
              <span className="text-xs font-bold text-emerald-400">
                {Math.round(pronunciation.accuracyScore)}%
              </span>
            </div>
            <div className="p-2 bg-slate-850 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 font-medium block">Fluency</span>
              <span className="text-xs font-bold text-indigo-400">
                {Math.round(pronunciation.fluencyScore)}%
              </span>
            </div>
            <div className="p-2 bg-slate-850 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 font-medium block">Completeness</span>
              <span className="text-xs font-bold text-purple-400">
                {Math.round(pronunciation.completenessScore)}%
              </span>
            </div>
          </div>

          {/* Interactive Word Chips */}
          <div className="space-y-1.5">
            <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">
              Spoken Word Breakdown (Tap to inspect phonemes):
            </p>
            <div className="flex flex-wrap gap-1.5">
              {pronunciation.words.map((w, idx) => {
                const isSelected = selectedWordIndex === idx;
                return (
                  <button
                    key={idx}
                    type="button"
                    onClick={() => setSelectedWordIndex(isSelected ? null : idx)}
                    className={`px-2.5 py-1 rounded-xl border text-xs font-semibold transition-all ${getWordBgClass(
                      w.status
                    )} ${
                      isSelected
                        ? "bg-slate-800 ring-1 ring-indigo-500 shadow-sm"
                        : "bg-slate-850"
                    }`}
                  >
                    <span>{w.word}</span>
                    <span className="ml-1 text-[10px] opacity-75">
                      ({Math.round(w.score)}%)
                    </span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Selected Word Phoneme Inspector */}
          {selectedWordIndex !== null && pronunciation.words[selectedWordIndex] && (
            <div className="p-3 bg-slate-850 rounded-xl border border-slate-750 space-y-2 animate-in fade-in duration-150">
              <div className="flex items-center justify-between text-[11px]">
                <span className="font-bold text-slate-200">
                  Phonemes for &ldquo;{pronunciation.words[selectedWordIndex].word}&rdquo;:
                </span>
                <span className="text-slate-400 text-[10px]">
                  Rating: {pronunciation.words[selectedWordIndex].status}
                </span>
              </div>
              <div className="flex flex-wrap gap-1.5">
                {pronunciation.words[selectedWordIndex].phonemes.map((ph, pIdx) => (
                  <div
                    key={pIdx}
                    className="flex flex-col items-center px-2 py-1 rounded-lg border text-center min-w-[36px]"
                    style={{
                      borderColor: `${ph.colorHex}66`,
                      backgroundColor: `${ph.colorHex}18`,
                    }}
                  >
                    <span className="font-mono text-xs font-bold" style={{ color: ph.colorHex }}>
                      /{ph.phoneme}/
                    </span>
                    <span className="text-[9px] font-bold text-slate-300">
                      {Math.round(ph.score)}%
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Coaching Tip */}
          {pronunciation.feedback && (
            <div className="p-2.5 bg-indigo-950/40 border border-indigo-800/40 rounded-xl flex items-start gap-2 text-[11px] text-indigo-200">
              <Sparkles className="w-3.5 h-3.5 text-indigo-400 mt-0.5 shrink-0" />
              <span>{pronunciation.feedback}</span>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
