import React from "react";
import { BookOpen, Sparkles, Activity, Mic2 } from "lucide-react";
import { AssessmentResult } from "@/types";
import { Card } from "@/components/ui/Card";
import { ProgressBar } from "@/components/ui/ProgressBar";

export function ScoreBreakdown({
  assessment,
}: {
  assessment: AssessmentResult;
}) {
  const categories = [
    {
      title: "Grammar",
      score: assessment.grammarScore,
      icon: BookOpen,
      desc: "Tense consistency, sentence structure, prepositions",
    },
    {
      title: "Vocabulary",
      score: assessment.vocabularyScore,
      icon: Sparkles,
      desc: "Topic suitability, idiomatic breadth, precise terms",
    },
    {
      title: "Fluency",
      score: assessment.fluencyScore,
      icon: Activity,
      desc: "Speaking cadence, pause frequency, coherence",
    },
    {
      title: "Pronunciation",
      score: assessment.pronunciationScore || 80,
      icon: Mic2,
      desc: "Word clarity, syllable stress, natural pacing",
    },
  ];

  return (
    <div className="space-y-3">
      <h3 className="text-base font-extrabold text-white tracking-tight">
        Score Category Breakdown
      </h3>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {categories.map((cat) => {
          const Icon = cat.icon;
          return (
            <Card key={cat.title} className="p-5 bg-slate-900 border-slate-800">
              <div className="flex items-center justify-between">
                <div className="p-2 rounded-xl bg-purple-950/60 border border-purple-800/40 text-purple-400">
                  <Icon className="w-5 h-5" />
                </div>
                <span className="text-2xl font-black text-white">
                  {Math.round(cat.score)}
                  <span className="text-xs font-normal text-slate-500">/100</span>
                </span>
              </div>

              <div className="mt-3 space-y-1.5">
                <p className="text-sm font-bold text-slate-200">{cat.title}</p>
                <div className="w-full bg-slate-800 rounded-full h-1.5 overflow-hidden">
                  <div
                    className="bg-gradient-to-r from-purple-500 to-indigo-500 h-full rounded-full"
                    style={{ width: `${Math.min(100, cat.score)}%` }}
                  />
                </div>
                <p className="text-[11px] text-slate-400 pt-1 leading-snug">{cat.desc}</p>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
