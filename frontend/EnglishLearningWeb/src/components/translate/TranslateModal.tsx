"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { adaptiveLearningService } from "@/services/adaptiveLearningService";
import { TranslateResult } from "@/types";
import {
  Languages,
  Sparkles,
  Volume2,
  Copy,
  Check,
  ArrowRight,
  Loader2,
  BookOpen,
  Mic,
  Lightbulb,
} from "lucide-react";

interface TranslateModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function TranslateModal({ isOpen, onClose }: TranslateModalProps) {
  const router = useRouter();
  const [inputText, setInputText] = useState<string>("");
  const [sourceLang, setSourceLang] = useState<string>("Auto");
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [result, setResult] = useState<TranslateResult | null>(null);
  const [copiedType, setCopiedType] = useState<"natural" | "formal" | null>(null);

  const samplePhrases = [
    { label: "എനിക്ക് ഒരു കാപ്പി വേണം", lang: "Malayalam" },
    { label: "ഞാൻ നാളെ വരാം", lang: "Malayalam" },
    { label: "എന്താണ് ചെയ്യുന്നത്?", lang: "Malayalam" },
    { label: "मुझे मदद चाहिए", lang: "Hindi" },
  ];

  const handleTranslate = async (textToUse?: string) => {
    const text = (textToUse || inputText).trim();
    if (!text) return;

    setIsLoading(true);
    try {
      const data = await adaptiveLearningService.translatePhrase(text, sourceLang);
      setResult(data);
    } catch (err: unknown) {
      alert((err as Error).message || "Failed to translate phrase.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleCopy = (text: string, type: "natural" | "formal") => {
    if (typeof navigator !== "undefined" && navigator.clipboard) {
      navigator.clipboard.writeText(text);
      setCopiedType(type);
      setTimeout(() => setCopiedType(null), 2000);
    }
  };

  const handleSpeak = (text: string) => {
    if (typeof window !== "undefined" && "speechSynthesis" in window) {
      window.speechSynthesis.cancel();
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = "en-US";
      utterance.rate = 0.95;
      window.speechSynthesis.speak(utterance);
    }
  };

  const handlePracticeWithEmma = () => {
    if (!result) return;
    onClose();
    router.push(
      `/practice?topicId=22222222-2222-2222-2222-222222222222&objective=${encodeURIComponent(
        result.naturalEnglish
      )}`
    );
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Translate to Natural English">
      <div className="space-y-5 py-1 max-w-lg mx-auto">
        {/* Source Language & Input */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-xs text-slate-400">
            <span className="font-semibold text-slate-300">
              Enter phrase in Malayalam, Hindi, or any language:
            </span>
            <select
              value={sourceLang}
              onChange={(e) => setSourceLang(e.target.value)}
              className="bg-slate-900 border border-slate-700 rounded-lg px-2 py-1 text-xs text-slate-300 focus:outline-none focus:ring-1 focus:ring-cyan-500"
            >
              <option value="Auto">Auto-Detect</option>
              <option value="Malayalam">Malayalam (മലയാളം)</option>
              <option value="Hindi">Hindi (हिंदी)</option>
              <option value="Tamil">Tamil (தமிழ்)</option>
            </select>
          </div>

          <textarea
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
            placeholder="Type or paste your sentence here... (e.g. എനിക്ക് ഒരു കാപ്പി വേണം)"
            rows={3}
            className="w-full px-4 py-3 bg-slate-900 border border-slate-700/80 rounded-2xl text-sm text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-cyan-500 transition-all resize-none"
          />

          {/* Quick Examples */}
          <div className="flex flex-wrap gap-1.5 pt-1">
            <span className="text-[11px] text-slate-400 mr-1 self-center">Try:</span>
            {samplePhrases.map((s, idx) => (
              <button
                key={idx}
                type="button"
                onClick={() => {
                  setInputText(s.label);
                  setSourceLang(s.lang);
                  handleTranslate(s.label);
                }}
                className="px-2.5 py-1 rounded-xl bg-slate-800 hover:bg-slate-750 border border-slate-700 text-[11px] font-medium text-slate-300 transition-all"
              >
                {s.label}
              </button>
            ))}
          </div>
        </div>

        {/* Action Button */}
        <Button
          variant="primary"
          size="md"
          disabled={isLoading || !inputText.trim()}
          onClick={() => handleTranslate()}
          className="w-full bg-cyan-600 hover:bg-cyan-500 text-white font-bold py-2.5 rounded-xl shadow-md"
        >
          {isLoading ? (
            <div className="flex items-center justify-center space-x-2">
              <Loader2 className="w-4 h-4 animate-spin" />
              <span>Translating & Analyzing Grammar...</span>
            </div>
          ) : (
            <div className="flex items-center justify-center space-x-2">
              <Sparkles className="w-4 h-4" />
              <span>Translate & Explain</span>
            </div>
          )}
        </Button>

        {/* Translation Results */}
        {result && (
          <div className="space-y-4 pt-2 border-t border-slate-800 animate-in fade-in duration-200">
            {/* 1. Natural Conversational English */}
            <div className="p-4 bg-cyan-950/40 border border-cyan-800/50 rounded-2xl space-y-2">
              <div className="flex items-center justify-between text-xs">
                <span className="font-extrabold text-cyan-300 uppercase tracking-wide">
                  🗣️ Spoken / Conversational English
                </span>
                <div className="flex items-center space-x-1.5">
                  <button
                    type="button"
                    onClick={() => handleSpeak(result.naturalEnglish)}
                    className="p-1.5 rounded-lg bg-cyan-900/60 hover:bg-cyan-850 text-cyan-300 transition-all"
                    title="Listen to pronunciation"
                  >
                    <Volume2 className="w-3.5 h-3.5" />
                  </button>
                  <button
                    type="button"
                    onClick={() => handleCopy(result.naturalEnglish, "natural")}
                    className="p-1.5 rounded-lg bg-cyan-900/60 hover:bg-cyan-850 text-cyan-300 transition-all"
                    title="Copy to clipboard"
                  >
                    {copiedType === "natural" ? (
                      <Check className="w-3.5 h-3.5 text-emerald-400" />
                    ) : (
                      <Copy className="w-3.5 h-3.5" />
                    )}
                  </button>
                </div>
              </div>
              <p className="text-base font-bold text-white">&quot;{result.naturalEnglish}&quot;</p>
            </div>

            {/* 2. Formal / Professional English */}
            <div className="p-4 bg-slate-850 border border-slate-750 rounded-2xl space-y-2">
              <div className="flex items-center justify-between text-xs">
                <span className="font-extrabold text-purple-300 uppercase tracking-wide">
                  💼 Formal / Workplace English
                </span>
                <div className="flex items-center space-x-1.5">
                  <button
                    type="button"
                    onClick={() => handleSpeak(result.formalEnglish)}
                    className="p-1.5 rounded-lg bg-purple-950/60 hover:bg-purple-900 text-purple-300 transition-all"
                    title="Listen to pronunciation"
                  >
                    <Volume2 className="w-3.5 h-3.5" />
                  </button>
                  <button
                    type="button"
                    onClick={() => handleCopy(result.formalEnglish, "formal")}
                    className="p-1.5 rounded-lg bg-purple-950/60 hover:bg-purple-900 text-purple-300 transition-all"
                    title="Copy to clipboard"
                  >
                    {copiedType === "formal" ? (
                      <Check className="w-3.5 h-3.5 text-emerald-400" />
                    ) : (
                      <Copy className="w-3.5 h-3.5" />
                    )}
                  </button>
                </div>
              </div>
              <p className="text-sm font-semibold text-slate-200">
                &quot;{result.formalEnglish}&quot;
              </p>
            </div>

            {/* 3. Grammar & Nuance Explanation */}
            <div className="p-3.5 bg-slate-850/80 border border-slate-800 rounded-2xl space-y-1.5">
              <div className="flex items-center space-x-1.5 text-xs font-bold text-amber-400">
                <Lightbulb className="w-3.5 h-3.5" />
                <span>Why this phrasing is natural</span>
              </div>
              <p className="text-xs text-slate-300 leading-relaxed">{result.explanation}</p>
            </div>

            {/* 4. Pronunciation Tip */}
            {result.pronunciationTip && (
              <div className="p-3 bg-slate-850/60 border border-slate-800 rounded-2xl space-y-1">
                <span className="text-[11px] font-bold text-indigo-300 uppercase tracking-wide">
                  Pronunciation & Rhythm
                </span>
                <p className="text-xs text-slate-300 italic">{result.pronunciationTip}</p>
              </div>
            )}

            {/* 5. Key Vocabulary Chips */}
            {result.keyVocabulary && result.keyVocabulary.length > 0 && (
              <div className="space-y-1.5">
                <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wide">
                  Key Vocabulary Breakdown
                </span>
                <div className="space-y-1">
                  {result.keyVocabulary.map((item, idx) => (
                    <div
                      key={idx}
                      className="p-2 bg-slate-850 rounded-xl border border-slate-800 text-xs flex items-center justify-between"
                    >
                      <span className="font-bold text-cyan-300">{item.word}</span>
                      <span className="text-slate-400">{item.meaning}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* CTA: Practice with Emma */}
            <div className="pt-2">
              <Button
                variant="primary"
                size="md"
                onClick={handlePracticeWithEmma}
                className="w-full bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white font-bold py-2.5 rounded-xl shadow-lg"
              >
                <div className="flex items-center justify-center space-x-2">
                  <Mic className="w-4 h-4" />
                  <span>Practice this phrase with Emma</span>
                  <ArrowRight className="w-4 h-4" />
                </div>
              </Button>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
}
