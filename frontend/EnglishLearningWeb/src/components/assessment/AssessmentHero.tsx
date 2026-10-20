import React from "react";
import Link from "next/link";
import { CheckCircle2, RotateCcw, LayoutDashboard, History, Sparkles } from "lucide-react";
import { AssessmentResult } from "@/types";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";

export function AssessmentHero({
  assessment,
}: {
  assessment: AssessmentResult;
}) {
  return (
    <div className="bg-gradient-to-br from-purple-950/80 via-slate-900 to-slate-900 border border-purple-800/50 text-white rounded-3xl p-6 sm:p-8 shadow-2xl relative overflow-hidden space-y-6">
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-6 relative z-10">
        {/* Left Score Card */}
        <div className="flex items-center space-x-5">
          <div className="w-24 h-24 sm:w-28 sm:h-28 rounded-3xl bg-purple-600/30 border border-purple-400/40 flex flex-col items-center justify-center text-center flex-shrink-0 backdrop-blur-md shadow-lg shadow-purple-950/50">
            <span className="text-3xl sm:text-4xl font-black text-white tracking-tight">
              {assessment.overallScore}
            </span>
            <span className="text-[11px] font-bold text-purple-300 uppercase tracking-wider">
              Overall Score
            </span>
          </div>

          <div className="space-y-1.5">
            <div className="flex items-center space-x-2">
              <span className="text-xs font-bold uppercase tracking-wider text-emerald-400 flex items-center gap-1">
                <CheckCircle2 className="w-3.5 h-3.5" />
                Session Assessment Finalized
              </span>
              <Badge cefrLevel={assessment.englishLevel} size="sm" />
            </div>
            <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
              {assessment.topicTitle}
            </h1>
            <p className="text-xs text-slate-300">
              Completed {assessment.completedAt} • Competence: <span className="font-bold text-white">Effective Speaker ({assessment.englishLevel})</span>
            </p>
          </div>
        </div>

        {/* Right Navigation CTAs */}
        <div className="flex flex-wrap items-center gap-2.5">
          <Link href="/topics">
            <Button
              variant="primary"
              size="md"
              className="bg-purple-600 hover:bg-purple-500 shadow-md shadow-purple-950/50"
              leftIcon={<RotateCcw className="w-4 h-4" />}
            >
              Practice Another Topic
            </Button>
          </Link>
          <Link href="/history">
            <Button
              variant="outline"
              size="md"
              className="bg-slate-800 text-slate-200 border-slate-700 hover:bg-slate-750"
              leftIcon={<History className="w-4 h-4" />}
            >
              History
            </Button>
          </Link>
        </div>
      </div>

      {/* Emma Evaluation Summary */}
      {assessment.fluencyFeedback?.summary && (
        <div className="relative z-10 pt-4 border-t border-slate-800 text-xs sm:text-sm text-slate-300 leading-relaxed bg-slate-900/50 p-4 rounded-2xl border border-slate-800/80">
          <p className="font-bold text-purple-300 text-xs uppercase tracking-wider mb-1">
            Emma&apos;s Assessment Summary:
          </p>
          {assessment.fluencyFeedback.summary}
        </div>
      )}
    </div>
  );
}
