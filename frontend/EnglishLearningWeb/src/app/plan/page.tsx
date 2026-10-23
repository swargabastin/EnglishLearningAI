"use client";

import React, { useEffect, useState, useRef, useCallback } from "react";
import Link from "next/link";
import { AppShell } from "@/components/layout/AppShell";
import { adaptiveLearningService } from "@/services/adaptiveLearningService";
import {
  LearnerAssessmentResult,
  DailyStudyPlan,
  StudyPlanPhase,
  LearningPlanHistory,
  AssessmentQuestion,
  PhaseTask,
  LearningObjectiveItem,
} from "@/types";
import {
  Sparkles,
  BookOpen,
  PenTool,
  CheckCircle2,
  Clock,
  ArrowRight,
  RotateCcw,
  AlertCircle,
  HelpCircle,
  Volume2,
  Mic,
  MicOff,
  ChevronRight,
  TrendingUp,
  Award,
  Layers,
  Calendar,
  Lock,
  Play,
  Pause,
  Send,
  Loader2,
  ShieldCheck,
  RefreshCw,
} from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";

export default function StudyPlanPage() {
  // Navigation / Tab state
  const [activeTab, setActiveTab] = useState<"plan" | "curriculum" | "assessment" | "history">("plan");

  // Core Data States
  const [assessment, setAssessment] = useState<LearnerAssessmentResult | null>(null);
  const [todayPlan, setTodayPlan] = useState<DailyStudyPlan | null>(null);
  const [history, setHistory] = useState<LearningPlanHistory | null>(null);
  const [objectives, setObjectives] = useState<LearningObjectiveItem[]>([]);
  const [tenseFilter, setTenseFilter] = useState<string>("All");
  const [isSwitchingTense, setIsSwitchingTense] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Active Phase Execution State
  const [selectedPhaseIndex, setSelectedPhaseIndex] = useState<number>(0);
  const [activeTimerSeconds, setActiveTimerSeconds] = useState<number>(0);
  const [isTimerRunning, setIsTimerRunning] = useState<boolean>(false);
  const [phaseTaskAnswers, setPhaseTaskAnswers] = useState<Record<string, string>>({});
  const [isSubmittingPhase, setIsSubmittingPhase] = useState<boolean>(false);
  const [phaseFeedback, setPhaseFeedback] = useState<{
    score: number;
    isMastered: boolean;
    feedback: string;
  } | null>(null);

  // Diagnostic Assessment Modal State
  const [isAssessmentModalOpen, setIsAssessmentModalOpen] = useState<boolean>(false);
  const [diagnosticQuestions, setDiagnosticQuestions] = useState<AssessmentQuestion[]>([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState<number>(0);
  const [assessmentAnswers, setAssessmentAnswers] = useState<Record<string, number>>({});
  const [isRecordingSpeech, setIsRecordingSpeech] = useState<boolean>(false);
  const [speechRecorded, setSpeechRecorded] = useState<boolean>(false);
  const [isSubmittingAssessment, setIsSubmittingAssessment] = useState<boolean>(false);
  const [enableSpeechAssessment, setEnableSpeechAssessment] = useState<boolean>(false);

  // Fetch initial data
  const loadData = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);
    try {
      // 1. Fetch latest assessment
      const latestAssessment = await adaptiveLearningService.getLatestAssessment();
      setAssessment(latestAssessment);

      // 2. Fetch today's plan
      const plan = await adaptiveLearningService.getTodayPlan(false);
      setTodayPlan(plan);

      // Find first uncompleted phase or first phase
      const uncompletedIndex = plan.phases.findIndex(
        (p) => p.status !== "Completed"
      );
      if (uncompletedIndex >= 0) {
        setSelectedPhaseIndex(uncompletedIndex);
      }

      // 3. Fetch history
      const historyData = await adaptiveLearningService.getLearningHistory();
      setHistory(historyData);

      // 4. Fetch all curriculum objectives / tenses
      const allObjectives = await adaptiveLearningService.getAllObjectives();
      setObjectives(allObjectives);
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || "Failed to load study plan.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Active Focus Duration Timer (Strictly tracks active tab and active phase)
  useEffect(() => {
    let interval: NodeJS.Timeout | null = null;
    if (isTimerRunning) {
      interval = setInterval(() => {
        setActiveTimerSeconds((prev) => prev + 1);
      }, 1000);
    }
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [isTimerRunning]);

  // Sync active duration to backend periodically (every 60 seconds)
  useEffect(() => {
    if (activeTimerSeconds > 0 && activeTimerSeconds % 60 === 0 && todayPlan) {
      const currentPhase = todayPlan.phases[selectedPhaseIndex];
      if (currentPhase && currentPhase.status !== "Completed") {
        adaptiveLearningService
          .updatePhaseProgress(currentPhase.id, 60)
          .catch(() => {});
      }
    }
  }, [activeTimerSeconds, selectedPhaseIndex, todayPlan]);

  // Handlers for Phase runner
  const handleSelectPhase = (index: number) => {
    const phase = todayPlan?.phases[index];
    if (!phase) return;
    if (phase.isLocked) {
      alert("This phase is locked. Please complete the previous mandatory phase first.");
      return;
    }
    setSelectedPhaseIndex(index);
    setPhaseFeedback(null);
    setIsTimerRunning(true);
  };

  const handleTaskAnswerChange = (taskId: string, answer: string) => {
    setPhaseTaskAnswers((prev) => ({
      ...prev,
      [taskId]: answer,
    }));
  };

  const handleCompleteCurrentPhase = async () => {
    if (!todayPlan) return;
    const currentPhase = todayPlan.phases[selectedPhaseIndex];
    if (!currentPhase) return;

    setIsSubmittingPhase(true);
    setIsTimerRunning(false);

    try {
      const result = await adaptiveLearningService.completePhase(
        currentPhase.id,
        phaseTaskAnswers,
        activeTimerSeconds
      );

      setPhaseFeedback({
        score: result.score,
        isMastered: result.isMastered,
        feedback: result.feedback,
      });

      // Refresh today's plan to show updated phase unlock & completion states
      const updatedPlan = await adaptiveLearningService.getTodayPlan(false);
      setTodayPlan(updatedPlan);

      // Auto-advance to next phase if unlocked
      if (result.nextPhaseUnlocked && selectedPhaseIndex < updatedPlan.phases.length - 1) {
        setSelectedPhaseIndex(selectedPhaseIndex + 1);
        setActiveTimerSeconds(0);
        setIsTimerRunning(true);
      }
    } catch (err: unknown) {
      alert((err as Error).message || "Failed to submit phase completion.");
    } finally {
      setIsSubmittingPhase(false);
    }
  };

  const handleSwitchTense = async (objectiveCode: string) => {
    setIsSwitchingTense(objectiveCode);
    setErrorMessage(null);
    try {
      const newPlan = await adaptiveLearningService.getTodayPlan(true, objectiveCode);
      setTodayPlan(newPlan);
      setSelectedPhaseIndex(0);
      setActiveTimerSeconds(0);
      setIsTimerRunning(true);
      setPhaseFeedback(null);
      setActiveTab("plan");
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || "Failed to switch tense.");
    } finally {
      setIsSwitchingTense(null);
    }
  };

  // Handlers for Diagnostic Level Assessment
  const handleOpenDiagnostic = async () => {
    try {
      setIsLoading(true);
      const questions = await adaptiveLearningService.getAssessmentQuestions();
      setDiagnosticQuestions(questions);
      setCurrentQuestionIndex(0);
      setAssessmentAnswers({});
      setIsAssessmentModalOpen(true);
    } catch (err: unknown) {
      alert((err as Error).message || "Failed to load diagnostic questions.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleAnswerDiagnostic = (optionIndex: number) => {
    const currentQ = diagnosticQuestions[currentQuestionIndex];
    if (!currentQ) return;
    setAssessmentAnswers((prev) => ({
      ...prev,
      [currentQ.id]: optionIndex,
    }));

    if (currentQuestionIndex < diagnosticQuestions.length - 1) {
      setCurrentQuestionIndex((prev) => prev + 1);
    }
  };

  const handleSubmitDiagnostic = async () => {
    setIsSubmittingAssessment(true);
    try {
      const answersPayload = Object.entries(assessmentAnswers).map(
        ([questionId, selectedOptionIndex]) => ({
          questionId,
          selectedOptionIndex,
          timeSpentSeconds: 15,
        })
      );

      const result = await adaptiveLearningService.submitAssessment({
        answers: answersPayload,
        speechAudioBase64: null,
      });

      setAssessment(result);
      setIsAssessmentModalOpen(false);

      // Reload plan to adapt to validated level
      await loadData();
    } catch (err: unknown) {
      alert((err as Error).message || "Failed to submit assessment.");
    } finally {
      setIsSubmittingAssessment(false);
    }
  };

  // Helper formatting
  const formatSeconds = (sec: number) => {
    const m = Math.floor(sec / 60);
    const s = sec % 60;
    return `${m}:${s < 10 ? "0" : ""}${s}`;
  };

  return (
    <AppShell>
      <div className="space-y-6 max-w-5xl mx-auto pb-12">
        {/* Top Header & Tab Navigation */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-5 sm:p-6 bg-gradient-to-br from-slate-900 via-slate-850 to-indigo-950/40 border border-slate-800 rounded-3xl shadow-xl">
          <div className="space-y-1">
            <div className="flex items-center space-x-2">
              <span className="px-2.5 py-0.5 rounded-full bg-indigo-500/20 text-indigo-300 border border-indigo-500/30 text-[11px] font-bold uppercase tracking-wider">
                Adaptive System
              </span>
              {assessment && (
                <span className="px-2.5 py-0.5 rounded-full bg-purple-500/20 text-purple-300 border border-purple-500/30 text-[11px] font-bold">
                  {assessment.category} Level ({assessment.cefrLevel})
                </span>
              )}
            </div>
            <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
              Adaptive Learning Path
            </h1>
            <p className="text-xs sm:text-sm text-slate-300">
              Validated curriculum progression across 4 mandatory daily learning phases.
            </p>
          </div>

          {/* Tab Switcher */}
          <div className="flex flex-wrap items-center bg-slate-900/90 p-1.5 rounded-2xl border border-slate-800 self-start sm:self-auto gap-1">
            <button
              onClick={() => setActiveTab("plan")}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all ${
                activeTab === "plan"
                  ? "bg-indigo-600 text-white shadow-md"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              Today&apos;s Plan
            </button>
            <button
              onClick={() => setActiveTab("curriculum")}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all ${
                activeTab === "curriculum"
                  ? "bg-indigo-600 text-white shadow-md"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              All 12 Tenses
            </button>
            <button
              onClick={() => setActiveTab("assessment")}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all ${
                activeTab === "assessment"
                  ? "bg-indigo-600 text-white shadow-md"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              Level & Skills
            </button>
            <button
              onClick={() => setActiveTab("history")}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all ${
                activeTab === "history"
                  ? "bg-indigo-600 text-white shadow-md"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              Mastery & History
            </button>
          </div>
        </div>

        {/* Loading / Error Banners */}
        {isLoading && (
          <div className="p-8 text-center bg-slate-900 border border-slate-800 rounded-3xl flex flex-col items-center justify-center space-y-3">
            <Loader2 className="w-8 h-8 text-indigo-400 animate-spin" />
            <p className="text-sm font-semibold text-slate-300">
              Loading your personalized curriculum...
            </p>
          </div>
        )}

        {errorMessage && (
          <div className="p-4 bg-rose-950/40 border border-rose-800/60 rounded-2xl flex items-center justify-between text-rose-200 text-xs sm:text-sm">
            <div className="flex items-center space-x-2">
              <AlertCircle className="w-4 h-4 text-rose-400" />
              <span>{errorMessage}</span>
            </div>
            <button
              onClick={loadData}
              className="text-xs font-bold text-rose-300 underline hover:text-white"
            >
              Retry
            </button>
          </div>
        )}

        {/* TAB 1: TODAY'S 4-PHASE STUDY PLAN */}
        {!isLoading && activeTab === "plan" && todayPlan && (
          <div className="space-y-6">
            {/* Daily Objective & Duration Summary Card */}
            <div className="p-5 sm:p-6 bg-slate-900 border border-slate-800 rounded-3xl shadow-lg space-y-4">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                <div className="space-y-1">
                  <div className="flex items-center space-x-2 flex-wrap gap-y-1">
                    <span className="text-xs font-extrabold text-indigo-400">
                      TODAY&apos;S LEARNING OBJECTIVE
                    </span>
                    <span className="text-[11px] font-bold text-slate-400">
                      [{todayPlan.objectiveCode}]
                    </span>
                    <span className="px-2 py-0.5 rounded-md bg-slate-800 text-[10px] font-bold text-slate-300">
                      {todayPlan.difficulty} · {todayPlan.cefrLevel}
                    </span>
                  </div>
                  <h2 className="text-xl sm:text-2xl font-bold text-white">
                    {todayPlan.objectiveTitle}
                  </h2>
                </div>

                <div className="flex items-center space-x-3 flex-wrap gap-y-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setActiveTab("curriculum")}
                    className="border-indigo-500/40 text-indigo-300 hover:bg-indigo-950/50 hover:text-white text-xs"
                  >
                    <BookOpen className="w-3.5 h-3.5 mr-1.5" />
                    Browse All 12 Tenses
                  </Button>

                  {/* Target vs Actual Active Learning Time */}
                  <div className="flex items-center space-x-4 bg-slate-850 p-2.5 sm:p-3 rounded-2xl border border-slate-800">
                    <div className="space-y-0.5 text-right">
                      <p className="text-[11px] text-slate-400 font-medium">Active Learning</p>
                      <p className="text-sm font-extrabold text-white">
                        {todayPlan.actualMinutes} / {todayPlan.targetMinutes} min
                      </p>
                    </div>
                    <div className="w-10 h-10 rounded-xl bg-indigo-950 border border-indigo-700/50 flex items-center justify-center text-indigo-400">
                      <Clock className="w-5 h-5" />
                    </div>
                  </div>
                </div>
              </div>

              {/* Four Mandatory Phases Stepper */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-3 pt-2">
                {todayPlan.phases.map((phase, idx) => {
                  const isSelected = selectedPhaseIndex === idx;
                  const isCompleted = phase.status === "Completed";
                  const isLocked = phase.isLocked;

                  return (
                    <button
                      key={phase.id}
                      type="button"
                      onClick={() => handleSelectPhase(idx)}
                      disabled={isLocked}
                      className={`relative p-3.5 rounded-2xl border text-left transition-all ${
                        isSelected
                          ? "bg-indigo-950/60 border-indigo-500 shadow-md ring-1 ring-indigo-500/40"
                          : isCompleted
                          ? "bg-slate-900/90 border-emerald-800/40 hover:bg-slate-850"
                          : isLocked
                          ? "bg-slate-950/60 border-slate-800/60 opacity-60 cursor-not-allowed"
                          : "bg-slate-900 border-slate-800 hover:border-slate-700"
                      }`}
                    >
                      <div className="flex items-center justify-between mb-1.5">
                        <span className="text-[10px] font-extrabold text-slate-400">
                          PHASE {phase.phaseOrder}
                        </span>
                        {isCompleted ? (
                          <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                        ) : isLocked ? (
                          <Lock className="w-3.5 h-3.5 text-slate-500" />
                        ) : (
                          <span className="w-2 h-2 rounded-full bg-indigo-400 animate-pulse" />
                        )}
                      </div>
                      <p className="text-xs font-bold text-white truncate">
                        {phase.phaseOrder === 1 && "1. Grammar Study"}
                        {phase.phaseOrder === 2 && "2. Sentence Making"}
                        {phase.phaseOrder === 3 && "3. Practice"}
                        {phase.phaseOrder === 4 && "4. AI Speaking"}
                      </p>
                      <div className="flex items-center justify-between text-[11px] text-slate-400 mt-1">
                        <span>{phase.plannedDurationMinutes} min</span>
                        {phase.score !== null && phase.score !== undefined && (
                          <span className="font-bold text-emerald-400">{Math.round(phase.score)}%</span>
                        )}
                      </div>
                    </button>
                  );
                })}
              </div>
            </div>

            {/* Active Phase Runner Workspace */}
            {todayPlan.phases[selectedPhaseIndex] && (
              <div className="p-5 sm:p-7 bg-slate-900 border border-slate-800 rounded-3xl shadow-xl space-y-6">
                {/* Phase Header with Live Timer */}
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-4 border-b border-slate-800">
                  <div className="space-y-0.5">
                    <span className="text-xs font-extrabold text-indigo-400 uppercase tracking-wide">
                      Phase {todayPlan.phases[selectedPhaseIndex].phaseOrder} of 4 ·{" "}
                      {todayPlan.phases[selectedPhaseIndex].phaseType}
                    </span>
                    <h3 className="text-xl font-black text-white">
                      {todayPlan.phases[selectedPhaseIndex].objectiveTitle}
                    </h3>
                  </div>

                  {/* Anti-Idle Active Timer */}
                  <div className="flex items-center space-x-2 self-start sm:self-auto">
                    <div className="flex items-center space-x-2 bg-slate-850 px-3.5 py-1.5 rounded-xl border border-slate-800 text-xs font-mono font-bold text-slate-200">
                      <Clock className="w-3.5 h-3.5 text-indigo-400" />
                      <span>{formatSeconds(activeTimerSeconds)}</span>
                    </div>
                    <button
                      type="button"
                      onClick={() => setIsTimerRunning(!isTimerRunning)}
                      className={`p-2 rounded-xl text-xs font-bold border transition-all ${
                        isTimerRunning
                          ? "bg-amber-950/40 text-amber-300 border-amber-800/40 hover:bg-amber-900/50"
                          : "bg-indigo-600 text-white border-indigo-500 hover:bg-indigo-500"
                      }`}
                      title={isTimerRunning ? "Pause active timer" : "Resume active timer"}
                    >
                      {isTimerRunning ? <Pause className="w-3.5 h-3.5" /> : <Play className="w-3.5 h-3.5" />}
                    </button>
                  </div>
                </div>

                {/* Feedback Banner if Submitted */}
                {phaseFeedback && (
                  <div
                    className={`p-4 rounded-2xl border text-xs sm:text-sm space-y-1 ${
                      phaseFeedback.isMastered
                        ? "bg-emerald-950/40 border-emerald-800/60 text-emerald-200"
                        : "bg-amber-950/40 border-amber-800/60 text-amber-200"
                    }`}
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-2 font-bold">
                        <Award className="w-4 h-4 text-emerald-400" />
                        <span>
                          Score: {Math.round(phaseFeedback.score)}% ·{" "}
                          {phaseFeedback.isMastered ? "Mastered (≥75%)" : "Completed (Needs Review)"}
                        </span>
                      </div>
                    </div>
                    <p className="text-xs text-slate-300">{phaseFeedback.feedback}</p>
                  </div>
                )}

                {/* PHASE 1 SPECIFIC: LESSON CONTENT CARDS */}
                {todayPlan.phases[selectedPhaseIndex].phaseOrder === 1 &&
                  todayPlan.phases[selectedPhaseIndex].lessonContent && (
                    <div className="space-y-4">
                      {/* Formula & Rule Summary */}
                      <div className="p-4 sm:p-5 bg-indigo-950/30 border border-indigo-500/30 rounded-2xl space-y-2">
                        <span className="text-[11px] font-extrabold text-indigo-300 uppercase tracking-wide">
                          Grammar Formula
                        </span>
                        <p className="text-base sm:text-lg font-mono font-bold text-white bg-slate-900/80 p-3 rounded-xl border border-indigo-500/20">
                          {todayPlan.phases[selectedPhaseIndex].lessonContent?.formula}
                        </p>
                        <p className="text-xs sm:text-sm text-slate-300 leading-relaxed pt-1">
                          {todayPlan.phases[selectedPhaseIndex].lessonContent?.summary}
                        </p>
                      </div>

                      {/* Key Rules & Common Mistakes Grid */}
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="p-4 bg-slate-850 rounded-2xl border border-slate-800 space-y-2">
                          <p className="text-xs font-bold text-emerald-400 uppercase tracking-wide">
                            Key Rules
                          </p>
                          <ul className="space-y-1.5 text-xs text-slate-300 list-disc list-inside">
                            {todayPlan.phases[selectedPhaseIndex].lessonContent?.keyRules.map(
                              (rule, i) => (
                                <li key={i}>{rule}</li>
                              )
                            )}
                          </ul>
                        </div>

                        <div className="p-4 bg-slate-850 rounded-2xl border border-slate-800 space-y-2">
                          <p className="text-xs font-bold text-rose-400 uppercase tracking-wide">
                            Common Mistakes to Avoid
                          </p>
                          <ul className="space-y-1.5 text-xs text-slate-300 list-disc list-inside">
                            {todayPlan.phases[selectedPhaseIndex].lessonContent?.commonMistakes.map(
                              (mistake, i) => (
                                <li key={i}>{mistake}</li>
                              )
                            )}
                          </ul>
                        </div>
                      </div>

                      {/* Examples */}
                      <div className="p-4 bg-slate-850 rounded-2xl border border-slate-800 space-y-2">
                        <p className="text-xs font-bold text-purple-400 uppercase tracking-wide">
                          Clear Examples
                        </p>
                        <div className="space-y-1.5">
                          {todayPlan.phases[selectedPhaseIndex].lessonContent?.examples.map(
                            (ex, i) => (
                              <p
                                key={i}
                                className="text-xs sm:text-sm text-slate-200 bg-slate-900/60 p-2.5 rounded-xl border border-slate-800"
                              >
                                &quot;{ex}&quot;
                              </p>
                            )
                          )}
                        </div>
                      </div>
                    </div>
                  )}

                {/* PHASE 4 SPECIFIC: AI SPEAKING WITH EMMA DEEP LINK */}
                {todayPlan.phases[selectedPhaseIndex].phaseOrder === 4 && (
                  <div className="p-6 bg-gradient-to-r from-purple-900/40 via-indigo-900/40 to-slate-900 border border-purple-500/30 rounded-3xl space-y-4 text-center">
                    <div className="w-14 h-14 rounded-2xl bg-purple-600/30 border border-purple-400/30 flex items-center justify-center text-purple-300 mx-auto">
                      <Mic className="w-7 h-7" />
                    </div>
                    <div className="space-y-1 max-w-md mx-auto">
                      <h4 className="text-lg font-bold text-white">
                        Live Speaking with Emma
                      </h4>
                      <p className="text-xs sm:text-sm text-slate-300">
                        Put today&apos;s objective into spoken conversation with Emma. Emma will
                        listen, reply, and offer real-time speech guidance!
                      </p>
                    </div>

                    <div className="pt-2">
                      <Link
                        href={`/practice?topicId=general-1&objective=${encodeURIComponent(
                          todayPlan.objectiveTitle
                        )}`}
                        className="inline-flex items-center space-x-2 text-sm font-bold bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white px-6 py-3 rounded-2xl shadow-lg transition-all"
                      >
                        <Mic className="w-4 h-4" />
                        <span>Launch Emma Speaking Room</span>
                        <ArrowRight className="w-4 h-4" />
                      </Link>
                    </div>
                  </div>
                )}

                {/* INTERACTIVE PHASE TASKS (Multiple Choice, Sentence Builder, Practice) */}
                {todayPlan.phases[selectedPhaseIndex].tasks.length > 0 && (
                  <div className="space-y-4 pt-2">
                    <h4 className="text-sm font-bold text-slate-200">
                      Phase Tasks & Verification
                    </h4>

                    {todayPlan.phases[selectedPhaseIndex].tasks.map((task, tIdx) => {
                      const currentAnswer = phaseTaskAnswers[task.id] || "";

                      return (
                        <div
                          key={task.id}
                          className="p-4 sm:p-5 bg-slate-850 border border-slate-800 rounded-2xl space-y-3"
                        >
                          <div className="space-y-1">
                            <span className="text-[10px] font-extrabold text-indigo-400 uppercase tracking-wide">
                              Task {tIdx + 1}: {task.instruction}
                            </span>
                            <p className="text-sm sm:text-base font-semibold text-white">
                              {task.question}
                            </p>
                          </div>

                          {/* Multiple Choice Options */}
                          {task.options && task.options.length > 0 ? (
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 pt-1">
                              {task.options.map((opt, oIdx) => {
                                const isSelected = currentAnswer === opt;
                                return (
                                  <button
                                    key={oIdx}
                                    type="button"
                                    onClick={() => handleTaskAnswerChange(task.id, opt)}
                                    className={`p-3 rounded-xl text-xs sm:text-sm font-medium text-left border transition-all ${
                                      isSelected
                                        ? "bg-indigo-600 text-white border-indigo-400 font-bold"
                                        : "bg-slate-900 border-slate-700/60 text-slate-300 hover:bg-slate-800 hover:text-white"
                                    }`}
                                  >
                                    {opt}
                                  </button>
                                );
                              })}
                            </div>
                          ) : (
                            /* Sentence builder / text input */
                            <div className="space-y-2 pt-1">
                              <input
                                type="text"
                                value={currentAnswer}
                                onChange={(e) =>
                                  handleTaskAnswerChange(task.id, e.target.value)
                                }
                                placeholder="Type your sentence here..."
                                className="w-full px-4 py-2.5 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                              />
                            </div>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}

                {/* Submit Phase Button */}
                <div className="pt-4 flex justify-end">
                  <Button
                    variant="primary"
                    size="md"
                    disabled={isSubmittingPhase}
                    onClick={handleCompleteCurrentPhase}
                    className="bg-indigo-600 hover:bg-indigo-500 px-6 py-2.5"
                  >
                    {isSubmittingPhase ? (
                      <div className="flex items-center space-x-2">
                        <Loader2 className="w-4 h-4 animate-spin" />
                        <span>Verifying Tasks...</span>
                      </div>
                    ) : (
                      <div className="flex items-center space-x-2">
                        <span>Complete Phase & Unlock Next</span>
                        <ArrowRight className="w-4 h-4" />
                      </div>
                    )}
                  </Button>
                </div>
              </div>
            )}
          </div>
        )}

        {/* TAB: ALL 12 ENGLISH TENSES & CURRICULUM */}
        {!isLoading && activeTab === "curriculum" && (
          <div className="space-y-6">
            <div className="p-6 sm:p-8 bg-gradient-to-br from-slate-900 via-indigo-950/30 to-slate-900 border border-slate-800 rounded-3xl shadow-xl flex flex-col sm:flex-row sm:items-center justify-between gap-4">
              <div className="space-y-1">
                <span className="text-xs font-extrabold text-indigo-400 uppercase tracking-wide">
                  Complete Curriculum Catalog
                </span>
                <h2 className="text-2xl sm:text-3xl font-black text-white">
                  All 12 English Tenses
                </h2>
                <p className="text-xs sm:text-sm text-slate-300 max-w-xl">
                  Select any tense to instantly tailor today&apos;s 4-phase structured daily plan (Rules, Sentence Making, Quiz, and AI Speaking Practice).
                </p>
              </div>

              {/* Tense Filter Pills */}
              <div className="flex flex-wrap gap-1.5 self-start sm:self-auto">
                {["All", "Present", "Past", "Future", "Mastered"].map((filter) => (
                  <button
                    key={filter}
                    onClick={() => setTenseFilter(filter)}
                    className={`px-3 py-1.5 rounded-xl text-xs font-bold transition-all ${
                      tenseFilter === filter
                        ? "bg-indigo-600 text-white shadow-md"
                        : "bg-slate-850 text-slate-400 hover:text-white border border-slate-800"
                    }`}
                  >
                    {filter}
                  </button>
                ))}
              </div>
            </div>

            {/* Tenses Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {objectives
                .filter((obj) => {
                  if (tenseFilter === "Present") return obj.code.includes("PRES");
                  if (tenseFilter === "Past") return obj.code.includes("PAST");
                  if (tenseFilter === "Future") return obj.code.includes("FUTURE");
                  if (tenseFilter === "Mastered") return obj.isMastered;
                  return true;
                })
                .map((obj) => {
                  const isCurrentPlanTense = todayPlan?.objectiveCode === obj.code;
                  const isSwitchingThis = isSwitchingTense === obj.code;

                  return (
                    <div
                      key={obj.id}
                      className={`p-5 rounded-3xl border transition-all flex flex-col justify-between space-y-4 ${
                        isCurrentPlanTense
                          ? "bg-indigo-950/40 border-indigo-500/80 shadow-lg ring-1 ring-indigo-500/30"
                          : "bg-slate-900 border-slate-800 hover:border-slate-700 hover:bg-slate-850/60"
                      }`}
                    >
                      <div className="space-y-3">
                        <div className="flex items-center justify-between gap-2">
                          <span className="font-mono text-[10px] font-bold text-slate-400">
                            {obj.code}
                          </span>
                          <div className="flex items-center space-x-1.5">
                            <span className="px-2 py-0.5 rounded-md bg-slate-800 text-[10px] font-bold text-indigo-300">
                              {obj.cefrLevel} · {obj.difficulty}
                            </span>
                            {obj.isMastered ? (
                              <span className="px-2 py-0.5 rounded-md bg-emerald-950/80 border border-emerald-800/50 text-[10px] font-bold text-emerald-400 flex items-center gap-1">
                                <CheckCircle2 className="w-3 h-3" />
                                {obj.masteryScore ? `${Math.round(obj.masteryScore)}%` : "Mastered"}
                              </span>
                            ) : null}
                          </div>
                        </div>

                        <div>
                          <h3 className="text-base font-bold text-white leading-snug">
                            {obj.title}
                          </h3>
                          <p className="text-xs text-slate-300 mt-1 line-clamp-3 leading-relaxed">
                            {obj.description}
                          </p>
                        </div>
                      </div>

                      <div className="pt-3 border-t border-slate-800/80 flex items-center justify-between">
                        <span className="text-[11px] text-slate-400 font-medium">
                          ~{obj.suggestedDurationMinutes} min lesson
                        </span>

                        {isCurrentPlanTense ? (
                          <span className="px-3 py-1 rounded-xl bg-indigo-900/60 border border-indigo-500/40 text-[11px] font-bold text-indigo-200 flex items-center gap-1">
                            <Sparkles className="w-3 h-3 text-indigo-400" />
                            Assigned Today
                          </span>
                        ) : (
                          <Button
                            variant="primary"
                            size="sm"
                            disabled={!!isSwitchingTense}
                            onClick={() => handleSwitchTense(obj.code)}
                            className="bg-indigo-600 hover:bg-indigo-500 text-xs px-3 py-1.5"
                          >
                            {isSwitchingThis ? (
                              <Loader2 className="w-3.5 h-3.5 animate-spin mr-1" />
                            ) : (
                              <Play className="w-3 h-3 mr-1" />
                            )}
                            Study Today
                          </Button>
                        )}
                      </div>
                    </div>
                  );
                })}
            </div>
          </div>
        )}

        {/* TAB 2: LEVEL VALIDATION & SKILL SCORES */}
        {!isLoading && activeTab === "assessment" && (
          <div className="space-y-6">
            {assessment ? (
              <div className="p-6 sm:p-8 bg-slate-900 border border-slate-800 rounded-3xl shadow-xl space-y-6">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-6 border-b border-slate-800">
                  <div className="space-y-1">
                    <span className="text-xs font-extrabold text-indigo-400 uppercase tracking-wide">
                      Validated Diagnostic Level
                    </span>
                    <h2 className="text-2xl sm:text-3xl font-black text-white">
                      {assessment.category} English ({assessment.cefrLevel})
                    </h2>
                    <p className="text-xs text-slate-400">
                      *Informal diagnostic reference based on CEFR criteria. Not an official certification.
                    </p>
                  </div>

                  <div className="flex items-center space-x-3">
                    <div className="text-center p-3 bg-slate-850 rounded-2xl border border-slate-800">
                      <p className="text-[11px] text-slate-400">Overall Score</p>
                      <p className="text-xl font-black text-indigo-400">
                        {Math.round(assessment.overallScore)}%
                      </p>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={handleOpenDiagnostic}
                      className="border-slate-700 text-slate-300 hover:text-white"
                    >
                      <RotateCcw className="w-3.5 h-3.5 mr-1.5" />
                      Retake
                    </Button>
                  </div>
                </div>

                {/* 5-Skill Score Breakdown */}
                <div className="space-y-3">
                  <h3 className="text-sm font-bold text-slate-200">
                    Individual Skill Competencies
                  </h3>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    {/* Grammar */}
                    <div className="p-3.5 bg-slate-850 rounded-2xl border border-slate-800 space-y-1.5">
                      <div className="flex justify-between text-xs font-semibold">
                        <span className="text-slate-300">Grammar</span>
                        <span className="text-indigo-400 font-bold">
                          {Math.round(assessment.grammarScore)}%
                        </span>
                      </div>
                      <div className="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
                        <div
                          className="bg-indigo-500 h-full rounded-full"
                          style={{ width: `${Math.round(assessment.grammarScore)}%` }}
                        />
                      </div>
                    </div>

                    {/* Vocabulary */}
                    <div className="p-3.5 bg-slate-850 rounded-2xl border border-slate-800 space-y-1.5">
                      <div className="flex justify-between text-xs font-semibold">
                        <span className="text-slate-300">Vocabulary</span>
                        <span className="text-emerald-400 font-bold">
                          {Math.round(assessment.vocabularyScore)}%
                        </span>
                      </div>
                      <div className="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
                        <div
                          className="bg-emerald-500 h-full rounded-full"
                          style={{ width: `${Math.round(assessment.vocabularyScore)}%` }}
                        />
                      </div>
                    </div>

                    {/* Sentence Construction */}
                    <div className="p-3.5 bg-slate-850 rounded-2xl border border-slate-800 space-y-1.5">
                      <div className="flex justify-between text-xs font-semibold">
                        <span className="text-slate-300">Sentence Construction</span>
                        <span className="text-purple-400 font-bold">
                          {Math.round(assessment.sentenceScore)}%
                        </span>
                      </div>
                      <div className="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
                        <div
                          className="bg-purple-500 h-full rounded-full"
                          style={{ width: `${Math.round(assessment.sentenceScore)}%` }}
                        />
                      </div>
                    </div>

                    {/* Reading */}
                    <div className="p-3.5 bg-slate-850 rounded-2xl border border-slate-800 space-y-1.5">
                      <div className="flex justify-between text-xs font-semibold">
                        <span className="text-slate-300">Reading Comprehension</span>
                        <span className="text-cyan-400 font-bold">
                          {Math.round(assessment.readingScore)}%
                        </span>
                      </div>
                      <div className="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
                        <div
                          className="bg-cyan-500 h-full rounded-full"
                          style={{ width: `${Math.round(assessment.readingScore)}%` }}
                        />
                      </div>
                    </div>

                    {/* Speaking (With unassessed handling) */}
                    <div className="p-3.5 bg-slate-850 rounded-2xl border border-slate-800 space-y-1.5 sm:col-span-2">
                      <div className="flex justify-between text-xs font-semibold">
                        <span className="text-slate-300">Speaking & Fluency</span>
                        {assessment.isSpeakingAssessed && typeof assessment.speakingScore === "number" ? (
                          <span className="text-amber-400 font-bold">
                            {Math.round(assessment.speakingScore)}%
                          </span>
                        ) : (
                          <span className="text-slate-400 font-medium italic">
                            Unassessed (No mic input required)
                          </span>
                        )}
                      </div>
                      <div className="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
                        <div
                          className="bg-amber-500 h-full rounded-full"
                          style={{
                            width: `${
                              assessment.isSpeakingAssessed && typeof assessment.speakingScore === "number"
                                ? Math.round(assessment.speakingScore)
                                : 0
                            }%`,
                          }}
                        />
                      </div>
                    </div>
                  </div>
                </div>

                {/* Weaknesses & Recommendations */}
                {assessment.weaknesses.length > 0 && (
                  <div className="p-4 bg-slate-850 rounded-2xl border border-slate-800 space-y-2">
                    <p className="text-xs font-bold text-slate-300 uppercase tracking-wide">
                      Target Areas for Focused Practice
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {assessment.weaknesses.map((w, i) => (
                        <span
                          key={i}
                          className="px-3 py-1 rounded-xl bg-indigo-950/60 border border-indigo-700/40 text-xs font-medium text-indigo-300"
                        >
                          {w}
                        </span>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            ) : (
              /* If learner has not taken assessment yet */
              <div className="p-8 sm:p-12 text-center bg-slate-900 border border-slate-800 rounded-3xl space-y-5">
                <div className="w-16 h-16 rounded-3xl bg-indigo-950/60 border border-indigo-500/30 text-indigo-400 flex items-center justify-center mx-auto">
                  <ShieldCheck className="w-8 h-8" />
                </div>
                <div className="space-y-1.5 max-w-md mx-auto">
                  <h3 className="text-xl sm:text-2xl font-bold text-white">
                    Validate Your English Level
                  </h3>
                  <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
                    Complete our 5-minute diagnostic assessment covering Grammar, Vocabulary,
                    Sentence Construction, and Reading to tailor your 4-phase daily plan!
                  </p>
                </div>
                <Button
                  variant="primary"
                  size="md"
                  onClick={handleOpenDiagnostic}
                  className="bg-indigo-600 hover:bg-indigo-500 px-6 py-2.5"
                >
                  Start Diagnostic Assessment
                </Button>
              </div>
            )}
          </div>
        )}

        {/* TAB 3: LEARNING HISTORY & MASTERY REVISION */}
        {!isLoading && activeTab === "history" && history && (
          <div className="space-y-6">
            {/* Stats Hero */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="p-5 bg-slate-900 border border-slate-800 rounded-3xl space-y-1">
                <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wide">
                  Mastered Objectives (≥75%)
                </span>
                <p className="text-3xl font-black text-emerald-400">
                  {history.masteredObjectivesCount}
                </p>
                <p className="text-xs text-slate-400">Curriculum concepts verified</p>
              </div>

              <div className="p-5 bg-slate-900 border border-slate-800 rounded-3xl space-y-1">
                <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wide">
                  Total Active Learning Time
                </span>
                <p className="text-3xl font-black text-indigo-400">
                  {history.totalActiveMinutes} min
                </p>
                <p className="text-xs text-slate-400">Excludes idle and paused time</p>
              </div>
            </div>

            {/* Scheduled Revision Recommendations */}
            {history.recommendedRevisions.length > 0 && (
              <div className="p-5 bg-slate-900 border border-slate-800 rounded-3xl space-y-3">
                <h3 className="text-sm font-bold text-amber-300 flex items-center space-x-2">
                  <RotateCcw className="w-4 h-4" />
                  <span>Scheduled Anti-Repetition Revisions</span>
                </h3>
                <div className="space-y-2">
                  {history.recommendedRevisions.map((rev, idx) => (
                    <div
                      key={idx}
                      className="p-3 bg-slate-850 rounded-2xl border border-slate-800 text-xs sm:text-sm text-slate-200 flex items-center justify-between"
                    >
                      <span>{rev}</span>
                      <span className="px-2 py-0.5 rounded-full bg-amber-950/60 border border-amber-800/40 text-[10px] font-bold text-amber-400">
                        Due Revision
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Recent Daily Plans History */}
            <div className="p-5 sm:p-6 bg-slate-900 border border-slate-800 rounded-3xl space-y-4">
              <h3 className="text-base font-bold text-white">Recent Daily Plans</h3>

              {history.recentPlans.length === 0 ? (
                <p className="text-xs text-slate-400">No previous plans recorded yet.</p>
              ) : (
                <div className="space-y-2.5">
                  {history.recentPlans.map((plan) => (
                    <div
                      key={plan.id}
                      className="p-4 bg-slate-850 rounded-2xl border border-slate-800 flex flex-col sm:flex-row sm:items-center justify-between gap-3"
                    >
                      <div className="space-y-0.5">
                        <div className="flex items-center space-x-2 text-xs">
                          <span className="font-mono text-slate-400">{plan.planDate}</span>
                          <span className="px-2 py-0.5 rounded-md bg-slate-800 text-[10px] font-bold text-indigo-300">
                            {plan.objectiveCode}
                          </span>
                        </div>
                        <p className="text-sm font-bold text-white">{plan.objectiveTitle}</p>
                      </div>

                      <div className="flex items-center space-x-3 text-xs">
                        <span className="text-slate-400">{plan.actualMinutes} min practiced</span>
                        <span
                          className={`px-2.5 py-1 rounded-xl text-xs font-bold border ${
                            plan.status === "Completed"
                              ? "bg-emerald-950/50 text-emerald-300 border-emerald-800/40"
                              : "bg-amber-950/50 text-amber-300 border-amber-800/40"
                          }`}
                        >
                          {plan.status}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}

        {/* DIAGNOSTIC LEVEL ASSESSMENT MODAL */}
        <Modal
          isOpen={isAssessmentModalOpen}
          onClose={() => setIsAssessmentModalOpen(false)}
          title="Diagnostic English Level Assessment"
        >
          <div className="space-y-5 py-2">
            {diagnosticQuestions.length > 0 && diagnosticQuestions[currentQuestionIndex] ? (
              <div className="space-y-4">
                {/* Progress bar */}
                <div className="space-y-1.5">
                  <div className="flex justify-between text-xs text-slate-400">
                    <span>
                      Question {currentQuestionIndex + 1} of {diagnosticQuestions.length}
                    </span>
                    <span className="font-bold text-indigo-400">
                      {diagnosticQuestions[currentQuestionIndex].skillCategory}
                    </span>
                  </div>
                  <div className="w-full bg-slate-800 rounded-full h-1.5">
                    <div
                      className="bg-indigo-500 h-full rounded-full transition-all"
                      style={{
                        width: `${
                          ((currentQuestionIndex + 1) / diagnosticQuestions.length) * 100
                        }%`,
                      }}
                    />
                  </div>
                </div>

                {/* Optional Passage */}
                {diagnosticQuestions[currentQuestionIndex].passage && (
                  <div className="p-3 bg-slate-900 border border-slate-800 rounded-xl text-xs text-slate-300 italic">
                    &quot;{diagnosticQuestions[currentQuestionIndex].passage}&quot;
                  </div>
                )}

                {/* Question Prompt */}
                <p className="text-sm sm:text-base font-bold text-white">
                  {diagnosticQuestions[currentQuestionIndex].prompt}
                </p>

                {/* Options */}
                <div className="space-y-2 pt-2">
                  {diagnosticQuestions[currentQuestionIndex].options.map((opt, oIdx) => {
                    const currentQ = diagnosticQuestions[currentQuestionIndex];
                    const isSelected = assessmentAnswers[currentQ.id] === oIdx;

                    return (
                      <button
                        key={oIdx}
                        type="button"
                        onClick={() => handleAnswerDiagnostic(oIdx)}
                        className={`w-full p-3 rounded-xl text-xs sm:text-sm text-left border transition-all ${
                          isSelected
                            ? "bg-indigo-600 text-white border-indigo-400 font-bold"
                            : "bg-slate-850 border-slate-750 text-slate-300 hover:bg-slate-800 hover:text-white"
                        }`}
                      >
                        {opt}
                      </button>
                    );
                  })}
                </div>

                {/* Optional Speaking Recording Section */}
                <div className="pt-2 border-t border-slate-800">
                  <div className="flex items-center justify-between text-xs text-slate-400">
                    <span className="flex items-center space-x-1.5">
                      <Mic className="w-3.5 h-3.5 text-indigo-400" />
                      <span>Optional Microphone Check</span>
                    </span>
                    <span className="text-[11px] text-slate-500 italic">
                      Skipping will not penalize your score
                    </span>
                  </div>
                </div>

                {/* Navigation / Submit buttons */}
                <div className="flex items-center justify-between pt-4 border-t border-slate-800">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={currentQuestionIndex === 0}
                    onClick={() => setCurrentQuestionIndex((prev) => prev - 1)}
                  >
                    Previous
                  </Button>

                  {currentQuestionIndex === diagnosticQuestions.length - 1 ? (
                    <Button
                      variant="primary"
                      size="sm"
                      disabled={isSubmittingAssessment}
                      onClick={handleSubmitDiagnostic}
                      className="bg-indigo-600 hover:bg-indigo-500"
                    >
                      {isSubmittingAssessment ? (
                        <div className="flex items-center space-x-1.5">
                          <Loader2 className="w-3.5 h-3.5 animate-spin" />
                          <span>Scoring...</span>
                        </div>
                      ) : (
                        "Submit Assessment"
                      )}
                    </Button>
                  ) : (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setCurrentQuestionIndex((prev) => prev + 1)}
                    >
                      Next
                    </Button>
                  )}
                </div>
              </div>
            ) : null}
          </div>
        </Modal>
      </div>
    </AppShell>
  );
}
