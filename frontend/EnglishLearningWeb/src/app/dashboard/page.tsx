"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { AppShell } from "@/components/layout/AppShell";
import { progressService, DashboardResponseDto } from "@/services/progressService";
import { useAuth } from "@/context/AuthContext";
import { useLanguage } from "@/context/LanguageContext";
import {
  Mic,
  MessageSquare,
  Languages,
  BookOpen,
  Mail,
  FileText,
  Sparkles,
  Flame,
  ArrowRight,
  Loader2,
  Bot,
} from "lucide-react";
import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { TranslateModal } from "@/components/translate/TranslateModal";
import { DocumentUploadModal } from "@/components/document/DocumentUploadModal";

export default function DashboardPage() {
  const router = useRouter();
  const { user: authUser, isLoading: isAuthLoading } = useAuth();
  const { t, language } = useLanguage();

  const [dashboardData, setDashboardData] = useState<DashboardResponseDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [modalFeature, setModalFeature] = useState<{ title: string; desc: string } | null>(null);
  const [isTranslateOpen, setIsTranslateOpen] = useState<boolean>(false);
  const [isDocumentOpen, setIsDocumentOpen] = useState<boolean>(false);

  useEffect(() => {
    let isMounted = true;

    async function loadDashboard() {
      try {
        const data = await progressService.getDashboard();
        if (isMounted) {
          setDashboardData(data);
        }
      } catch {
        // Fallback safely to auth user
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadDashboard();

    return () => {
      isMounted = false;
    };
  }, []);

  const stats = dashboardData?.stats;
  const recentSessions = dashboardData?.recentSessions || [];

  if (isAuthLoading || isLoading) {
    return (
      <AppShell>
        <div className="flex flex-col items-center justify-center min-h-[60vh] space-y-4">
          <Loader2 className="w-8 h-8 text-purple-400 animate-spin" />
          <p className="text-sm text-slate-400">Loading your learning dashboard...</p>
        </div>
      </AppShell>
    );
  }

  const user = authUser || {
    id: "guest",
    name: "Learner",
    email: "learner@fluentai.app",
    level: "B1" as const,
    learningGoal: "Speaking Fluency" as const,
    dailyPracticeTargetMinutes: 15,
    todayPracticeMinutes: stats?.totalPracticeMinutes ? Math.min(stats.totalPracticeMinutes, 15) : 0,
    streakDays: 1,
    createdAt: new Date().toISOString(),
    preferences: {
      tutorVoice: "alloy" as const,
      playbackSpeed: 1.0,
      autoPlayAudio: true,
      showLiveTranscript: true,
    },
  };

  const todayMinutes = user.todayPracticeMinutes ?? (stats?.totalPracticeMinutes ? Math.min(stats.totalPracticeMinutes, 15) : 0);
  const targetMinutes = user.dailyPracticeTargetMinutes ?? 15;
  const progressPercent = Math.min(100, Math.round((todayMinutes / targetMinutes) * 100));

  return (
    <AppShell>
      <div className="space-y-6 sm:space-y-8 max-w-4xl mx-auto">
        {/* Emma AI Greeting Header */}
        <div className="flex items-center justify-between gap-4 p-4 sm:p-6 bg-gradient-to-br from-slate-900 via-slate-850 to-purple-950/40 border border-slate-800/80 rounded-3xl shadow-xl">
          <div className="space-y-1 sm:space-y-1.5">
            <h1 className="text-xl sm:text-2xl font-extrabold tracking-tight text-white">
              {t.greeting(user.name.split(" ")[0])}
            </h1>
            <p className="text-xs sm:text-sm text-slate-300 font-medium">
              {t.subheading}
            </p>
          </div>

          <div className="relative flex-shrink-0">
            <div className="w-14 h-14 sm:w-16 sm:h-16 rounded-3xl bg-gradient-to-tr from-purple-600 via-indigo-600 to-blue-500 flex items-center justify-center text-white shadow-xl shadow-purple-600/30 ring-2 ring-purple-400/40 animate-glow-pulse">
              <Bot className="w-8 h-8 sm:w-9 sm:h-9" />
            </div>
            <span className="absolute -bottom-1 -right-1 w-4 h-4 rounded-full bg-emerald-400 ring-2 ring-slate-900 animate-pulse" />
          </div>
        </div>

        {/* Primary Action: Talk to Emma */}
        <Link
          href="/practice"
          className="group block relative overflow-hidden p-6 sm:p-8 bg-gradient-to-r from-purple-700 via-indigo-700 to-blue-700 rounded-3xl text-white shadow-2xl shadow-purple-900/40 border border-purple-400/30 transition-all hover:scale-[1.01] active:scale-[0.99] focus-ring"
        >
          {/* Subtle Ambient Background */}
          <div className="absolute top-0 right-0 -mt-10 -mr-10 w-48 h-48 bg-white/10 rounded-full blur-2xl pointer-events-none group-hover:scale-110 transition-transform duration-500" />
          <div className="absolute bottom-0 left-0 -mb-10 -ml-10 w-48 h-48 bg-purple-400/20 rounded-full blur-2xl pointer-events-none" />

          <div className="relative z-10 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div className="flex items-center space-x-4 sm:space-x-5">
              <div className="w-16 h-16 sm:w-18 sm:h-18 rounded-2xl bg-white/15 backdrop-blur-md flex items-center justify-center text-white shadow-inner ring-2 ring-white/30 group-hover:scale-105 transition-transform">
                <Mic className="w-8 h-8 sm:w-9 sm:h-9" />
              </div>
              <div className="space-y-1">
                <div className="flex items-center space-x-2">
                  <h2 className="text-xl sm:text-2xl font-black tracking-tight">
                    {t.talkToEmmaCta}
                  </h2>
                  <span className="px-2 py-0.5 rounded-full bg-white/20 text-[10px] font-bold uppercase tracking-wider">
                    Live AI
                  </span>
                </div>
                <p className="text-xs sm:text-sm text-purple-100 font-medium">
                  {t.talkToEmmaSub}
                </p>
              </div>
            </div>

            <div className="flex items-center space-x-2 text-sm font-bold bg-white text-purple-950 px-5 py-3 rounded-2xl shadow-lg group-hover:bg-purple-50 transition-colors self-start sm:self-auto">
              <span>{t.startSpeaking}</span>
              <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
            </div>
          </div>
        </Link>

        {/* Adaptive 4-Phase Learning Path Banner */}
        <Link
          href="/plan"
          className="group block p-5 sm:p-6 bg-gradient-to-br from-slate-900 via-indigo-950/40 to-slate-900 border border-indigo-500/30 hover:border-indigo-400/60 rounded-3xl shadow-xl transition-all hover:scale-[1.01]"
        >
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div className="space-y-1.5">
              <div className="flex items-center space-x-2">
                <span className="px-2.5 py-0.5 rounded-full bg-indigo-500/20 text-indigo-300 border border-indigo-500/30 text-[11px] font-bold tracking-wide uppercase">
                  Adaptive Path
                </span>
                <span className="text-xs text-slate-400 font-medium">Mandatory 4-Phase System</span>
              </div>
              <h3 className="text-lg sm:text-xl font-bold text-white group-hover:text-indigo-200 transition-colors">
                Personalized Daily English Plan
              </h3>
              <p className="text-xs sm:text-sm text-slate-300">
                1. Grammar Study → 2. Sentence Making → 3. Practice → 4. AI Speaking with Emma
              </p>
            </div>
            <div className="flex items-center space-x-2 text-xs sm:text-sm font-bold bg-indigo-600 hover:bg-indigo-500 text-white px-4 py-2.5 rounded-2xl shadow-md transition-all self-stretch sm:self-auto justify-center">
              <span>Start Daily Plan</span>
              <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
            </div>
          </div>
        </Link>

        {/* Quick Action Grid (2 Columns) */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 sm:gap-5">
          {/* Card 1: Conversation Practice */}
          <Link
            href="/practice"
            className="p-5 bg-slate-900/90 hover:bg-slate-850 border border-slate-800 hover:border-purple-500/40 rounded-3xl shadow-md transition-all group focus-ring space-y-3"
          >
            <div className="w-12 h-12 rounded-2xl bg-purple-950/60 border border-purple-700/40 text-purple-400 flex items-center justify-center group-hover:scale-105 transition-transform">
              <MessageSquare className="w-6 h-6" />
            </div>
            <div className="space-y-1">
              <h3 className="text-base font-bold text-white group-hover:text-purple-300 transition-colors">
                {t.quickPracticeTitle}
              </h3>
              <p className="text-xs text-slate-400 leading-relaxed">
                {t.quickPracticeSub}
              </p>
            </div>
          </Link>

          {/* Card 2: Translate to English */}
          <button
            type="button"
            onClick={() => setIsTranslateOpen(true)}
            className="text-left p-5 bg-slate-900/90 hover:bg-slate-850 border border-slate-800 hover:border-cyan-500/40 rounded-3xl shadow-md transition-all group focus-ring space-y-3"
          >
            <div className="w-12 h-12 rounded-2xl bg-cyan-950/60 border border-cyan-700/40 text-cyan-400 flex items-center justify-center group-hover:scale-105 transition-transform">
              <Languages className="w-6 h-6" />
            </div>
            <div className="space-y-1">
              <div className="flex items-center justify-between">
                <h3 className="text-base font-bold text-white group-hover:text-cyan-300 transition-colors">
                  {t.quickTranslateTitle}
                </h3>
                <span className="text-[10px] font-bold text-cyan-300 bg-cyan-950/80 border border-cyan-600/50 px-2 py-0.5 rounded-full">
                  Live AI
                </span>
              </div>
              <p className="text-xs text-slate-400 leading-relaxed">
                {t.quickTranslateSub}
              </p>
            </div>
          </button>
        </div>

        {/* Full-Width Action Cards */}
        <div className="space-y-3">
          {/* Learn a New Word */}
          <Link
            href="/vocabulary"
            className="flex items-center justify-between p-4 sm:p-5 bg-slate-900/80 hover:bg-slate-850 border border-slate-800 hover:border-purple-500/30 rounded-2xl transition-all group focus-ring"
          >
            <div className="flex items-center space-x-3.5">
              <div className="w-11 h-11 rounded-xl bg-emerald-950/50 border border-emerald-800/40 text-emerald-400 flex items-center justify-center">
                <BookOpen className="w-5 h-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-white group-hover:text-emerald-300 transition-colors">
                  {t.quickVocabTitle}
                </h4>
                <p className="text-xs text-slate-400">{t.quickVocabSub}</p>
              </div>
            </div>
            <ArrowRight className="w-4 h-4 text-slate-500 group-hover:text-white group-hover:translate-x-1 transition-all" />
          </Link>

          {/* Email / Message Writer */}
          <button
            type="button"
            onClick={() => router.push("/practice")}
            className="w-full flex items-center justify-between p-4 sm:p-5 bg-slate-900/80 hover:bg-slate-850 border border-slate-800 hover:border-blue-500/30 rounded-2xl transition-all group focus-ring text-left"
          >
            <div className="flex items-center space-x-3.5">
              <div className="w-11 h-11 rounded-xl bg-blue-950/50 border border-blue-800/40 text-blue-400 flex items-center justify-center">
                <Mail className="w-5 h-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-white group-hover:text-blue-300 transition-colors">
                  {t.quickEmailTitle}
                </h4>
                <p className="text-xs text-slate-400">{t.quickEmailSub}</p>
              </div>
            </div>
            <ArrowRight className="w-4 h-4 text-slate-500 group-hover:text-white group-hover:translate-x-1 transition-all" />
          </button>

          {/* Photo / PDF Upload */}
          <button
            type="button"
            onClick={() => setIsDocumentOpen(true)}
            className="w-full flex items-center justify-between p-4 sm:p-5 bg-slate-900/80 hover:bg-slate-850 border border-slate-800 hover:border-amber-500/30 rounded-2xl transition-all group focus-ring text-left"
          >
            <div className="flex items-center space-x-3.5">
              <div className="w-11 h-11 rounded-xl bg-amber-950/50 border border-amber-800/40 text-amber-400 flex items-center justify-center">
                <FileText className="w-5 h-5" />
              </div>
              <div>
                <div className="flex items-center space-x-2">
                  <h4 className="text-sm font-bold text-white group-hover:text-amber-300 transition-colors">
                    {t.quickUploadTitle}
                  </h4>
                  <span className="text-[10px] font-bold text-amber-300 bg-amber-950/80 border border-amber-600/50 px-2 py-0.5 rounded-full">
                    Active Tool
                  </span>
                </div>
                <p className="text-xs text-slate-400">{t.quickUploadSub}</p>
              </div>
            </div>
            <ArrowRight className="w-4 h-4 text-slate-500 group-hover:text-white group-hover:translate-x-1 transition-all" />
          </button>
        </div>

        {/* Learning Snapshot Card */}
        <div className="p-5 sm:p-6 bg-slate-900/90 border border-slate-800 rounded-3xl shadow-lg space-y-5">
          <div className="flex items-center justify-between">
            <h3 className="text-base font-extrabold text-white">
              {t.todaysProgress}
            </h3>
            <span className="text-xs font-bold text-purple-400 bg-purple-950/50 border border-purple-800/40 px-2.5 py-1 rounded-xl">
              {t.level(user.level)}
            </span>
          </div>

          {/* Progress Bar & Stats */}
          <div className="space-y-2">
            <div className="flex items-center justify-between text-xs font-semibold">
              <span className="text-slate-400">{t.dailyGoal}</span>
              <span className="text-white">
                {todayMinutes} / {targetMinutes} min ({progressPercent}%)
              </span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-2.5 overflow-hidden">
              <div
                className="bg-gradient-to-r from-purple-500 to-indigo-500 h-full rounded-full transition-all duration-500"
                style={{ width: `${progressPercent}%` }}
              />
            </div>
          </div>

          {/* Quick Metrics Grid */}
          <div className="grid grid-cols-3 gap-3 pt-2 text-center">
            <div className="p-3 bg-slate-850 rounded-2xl border border-slate-800">
              <p className="text-[11px] text-slate-400">Streak</p>
              <p className="text-base sm:text-lg font-black text-amber-400 flex items-center justify-center space-x-1">
                <Flame className="w-4 h-4 fill-amber-400" />
                <span>{user.streakDays}d</span>
              </p>
            </div>
            <div className="p-3 bg-slate-850 rounded-2xl border border-slate-800">
              <p className="text-[11px] text-slate-400">Fluency</p>
              <p className="text-base sm:text-lg font-black text-purple-400">
                {stats?.averageOverallScore ? `${Math.round(stats.averageOverallScore)}%` : "78%"}
              </p>
            </div>
            <div className="p-3 bg-slate-850 rounded-2xl border border-slate-800">
              <p className="text-[11px] text-slate-400">Completed</p>
              <p className="text-base sm:text-lg font-black text-emerald-400">
                {stats?.totalCompletedSessions ?? recentSessions.length ?? 3}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Feature Preview Modal */}
      <Modal
        isOpen={!!modalFeature}
        onClose={() => setModalFeature(null)}
        title={modalFeature?.title || "Feature Preview"}
      >
        <div className="space-y-4 text-center py-3">
          <div className="w-12 h-12 rounded-2xl bg-purple-950/60 border border-purple-700/40 text-purple-400 flex items-center justify-center mx-auto">
            <Sparkles className="w-6 h-6" />
          </div>
          <p className="text-xs sm:text-sm text-slate-300 leading-relaxed max-w-sm mx-auto">
            {modalFeature?.desc}
          </p>
          <div className="pt-2">
            <Button
              variant="primary"
              size="md"
              className="bg-purple-600 hover:bg-purple-500"
              onClick={() => setModalFeature(null)}
            >
              Got it
            </Button>
          </div>
        </div>
      </Modal>

      {/* Option 3: Bilingual Translation to English Modal */}
      <TranslateModal
        isOpen={isTranslateOpen}
        onClose={() => setIsTranslateOpen(false)}
      />

      {/* Option 4: Document / PDF English Practice Modal */}
      <DocumentUploadModal
        isOpen={isDocumentOpen}
        onClose={() => setIsDocumentOpen(false)}
      />
    </AppShell>
  );
}
