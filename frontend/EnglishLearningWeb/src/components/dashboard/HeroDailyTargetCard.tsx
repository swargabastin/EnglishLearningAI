"use client";

import React from "react";
import Link from "next/link";
import { Mic, Target, Sparkles, ArrowRight } from "lucide-react";
import { UserProfileSummaryDto, ProgressStatsDto } from "@/services/progressService";
import { Badge } from "@/components/ui/Badge";
import { ProgressBar } from "@/components/ui/ProgressBar";
import { CEFRLevel } from "@/types";

export interface HeroDailyTargetCardProps {
  user?: UserProfileSummaryDto;
  stats?: ProgressStatsDto;
}

export function HeroDailyTargetCard({ user, stats }: HeroDailyTargetCardProps) {
  const targetMinutes = 15;
  const todayMinutes = stats?.totalPracticeMinutes || 0;
  const percentComplete = Math.min(100, Math.round((todayMinutes / targetMinutes) * 100));

  const userName = user?.name || "Learner";
  const userLevel = (user?.englishLevel as CEFRLevel) || "B1";
  const learningGoal = user?.learningGoal || "Speaking Fluency";

  return (
    <div className="bg-gradient-to-br from-slate-900 to-slate-800 text-white rounded-3xl p-6 sm:p-8 shadow-md relative overflow-hidden">
      {/* Background Decorative Circles */}
      <div className="absolute top-0 right-0 -mt-8 -mr-8 w-64 h-64 bg-blue-600/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-0 left-1/3 -mb-8 w-48 h-48 bg-indigo-600/10 rounded-full blur-2xl pointer-events-none" />

      <div className="relative z-10 flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
        {/* Left Info Column */}
        <div className="space-y-4 max-w-xl">
          <div className="flex items-center space-x-2.5">
            <span className="text-xs font-semibold uppercase tracking-wider text-blue-400 flex items-center gap-1.5">
              <Sparkles className="w-3.5 h-3.5" />
              Daily Practice Routine
            </span>
            <span className="text-slate-600">•</span>
            <Badge cefrLevel={userLevel} size="sm" />
          </div>

          <div>
            <h2 className="text-2xl sm:text-3xl font-extrabold tracking-tight text-white">
              Welcome back, {userName}!
            </h2>
            <p className="text-sm text-slate-300 mt-1">
              Focus Goal: <span className="text-white font-medium">{learningGoal}</span>.{" "}
              {stats?.totalCompletedSessions && stats.totalCompletedSessions > 0 ? (
                <>You have completed <strong className="text-white">{stats.totalCompletedSessions} practice sessions</strong> so far.</>
              ) : (
                <>Start your first spoken practice session with AI Tutor Emma today!</>
              )}
            </p>
          </div>

          {/* Practice Progress Bar */}
          <div className="space-y-2 bg-white/5 border border-white/10 rounded-2xl p-4 max-w-md">
            <div className="flex items-center justify-between text-xs text-slate-300">
              <span className="flex items-center space-x-1.5 font-medium">
                <Target className="w-4 h-4 text-blue-400" />
                <span>Practice Target</span>
              </span>
              <span className="font-bold text-white">
                {todayMinutes} / {targetMinutes} min ({percentComplete}%)
              </span>
            </div>
            <ProgressBar
              value={todayMinutes}
              max={targetMinutes}
              variant="blue"
              size="md"
            />
          </div>
        </div>

        {/* Right CTA Column */}
        <div className="flex flex-col sm:flex-row lg:flex-col items-start lg:items-end gap-3 flex-shrink-0">
          <Link
            href="/practice"
            className="inline-flex items-center justify-center space-x-2.5 px-6 py-4 rounded-2xl bg-blue-600 hover:bg-blue-500 text-white font-bold text-sm sm:text-base shadow-lg shadow-blue-600/30 active:scale-95 transition-all w-full sm:w-auto"
          >
            <Mic className="w-5 h-5 text-white" />
            <span>Start Speaking Practice</span>
            <ArrowRight className="w-4 h-4" />
          </Link>
          <Link
            href="/topics"
            className="text-xs text-slate-300 hover:text-white transition-colors flex items-center space-x-1 px-2 py-1"
          >
            <span>Explore all 7 topics</span>
            <ArrowRight className="w-3 h-3" />
          </Link>
        </div>
      </div>
    </div>
  );
}
