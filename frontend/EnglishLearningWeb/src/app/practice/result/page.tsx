"use client";

import React, { useEffect, useState, useCallback, Suspense } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { AppShell } from "@/components/layout/AppShell";
import { AssessmentHero } from "@/components/assessment/AssessmentHero";
import { ScoreBreakdown } from "@/components/assessment/ScoreBreakdown";
import { StrengthsAndWeaknesses } from "@/components/assessment/StrengthsAndWeaknesses";
import { GrammarCorrectionsList } from "@/components/assessment/GrammarCorrectionsList";
import { VocabularySuggestionsList } from "@/components/assessment/VocabularySuggestionsList";
import { RecommendedActionCard } from "@/components/assessment/RecommendedActionCard";
import { assessmentService, AssessmentResponseDto } from "@/services/assessmentService";
import { mockAssessment } from "@/lib/mock";
import { AssessmentResult, CEFRLevel } from "@/types";
import { Loader2, AlertCircle, RefreshCw, ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/Button";

function ResultContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const sessionId = searchParams.get("session") || searchParams.get("assessmentId");

  const [assessmentData, setAssessmentData] = useState<AssessmentResponseDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const fetchAssessment = useCallback(async () => {
    if (!sessionId) {
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setErrorMessage(null);

    try {
      // First try generating/retrieving assessment
      const result = await assessmentService.generateAssessment(sessionId);
      setAssessmentData(result);
    } catch (err) {
      try {
        // If generate gave an error (e.g. already generated), try GET
        const existing = await assessmentService.getAssessment(sessionId);
        setAssessmentData(existing);
      } catch {
        setErrorMessage((err as Error).message || "Failed to load conversation assessment.");
      }
    } finally {
      setIsLoading(false);
    }
  }, [sessionId]);

  useEffect(() => {
    fetchAssessment();
  }, [fetchAssessment]);

  // Convert AssessmentResponseDto to UI AssessmentResult
  const currentAssessment: AssessmentResult = assessmentData
    ? {
        sessionId: assessmentData.sessionId,
        topicTitle: assessmentData.topicName,
        englishLevel: (assessmentData.englishLevel as CEFRLevel) || "B1",
        completedAt: new Date(assessmentData.createdAt).toLocaleDateString([], {
          month: "short",
          day: "numeric",
          year: "numeric",
        }),
        overallScore: Math.round(assessmentData.overallScore),
        grammarScore: Math.round(assessmentData.grammarScore),
        vocabularyScore: Math.round(assessmentData.vocabularyScore),
        fluencyScore: Math.round(assessmentData.fluencyScore),
        pronunciationScore: assessmentData.pronunciationScore ? Math.round(assessmentData.pronunciationScore) : undefined,
        strengths:
          assessmentData.strengths && assessmentData.strengths.length > 0
            ? assessmentData.strengths
            : ["Clear conversational turn-taking", "Good contextual response"],
        areasToImprove:
          assessmentData.areasForImprovement && assessmentData.areasForImprovement.length > 0
            ? assessmentData.areasForImprovement
            : ["Expand descriptive vocabulary", "Practice complex sentence transitions"],
        grammarCorrections: (assessmentData.grammarCorrections || []).map((gc, index) => ({
          id: `gc-${index}`,
          original: gc.original,
          corrected: gc.corrected,
          explanation: gc.explanation,
          ruleCategory: gc.ruleCategory || "Grammar",
        })),
        vocabularySuggestions: (assessmentData.vocabularySuggestions || []).map((vs, index) => ({
          id: `vs-${index}`,
          originalWord: vs.originalWord,
          suggestedAlternative: vs.suggestedAlternative,
          context: vs.context,
          cefrLevel: (vs.cefrLevel as CEFRLevel) || "B2",
        })),
        fluencyFeedback: {
          wordCount: 45,
          speakingRateWpm: 120,
          pauseCount: 2,
          summary: assessmentData.feedback || "Smooth conversational pace with coherent thoughts.",
        },
        recommendedPractice: {
          title: assessmentData.recommendations?.[0] || "Practice daily English dialogues",
          description: assessmentData.recommendations?.[1] || "Focus on complex sentence variation and transition words.",
          actionTopicId: "11111111-1111-1111-1111-111111111111",
        },
      }
    : mockAssessment;

  if (isLoading) {
    return (
      <AppShell>
        <div className="min-h-[500px] flex flex-col items-center justify-center space-y-4 max-w-lg mx-auto text-center px-4">
          <div className="w-16 h-16 rounded-3xl bg-purple-950/60 border border-purple-800/50 text-purple-400 flex items-center justify-center shadow-xl animate-pulse">
            <Loader2 className="w-8 h-8 animate-spin" />
          </div>
          <div className="space-y-1">
            <h2 className="text-xl font-extrabold text-white">
              Analyzing Your Conversation...
            </h2>
            <p className="text-xs sm:text-sm text-slate-400">
              Emma is evaluating your grammar precision, lexical variety, fluency flow, and communicative competence.
            </p>
          </div>
        </div>
      </AppShell>
    );
  }

  if (errorMessage && !assessmentData) {
    return (
      <AppShell>
        <div className="min-h-[450px] flex flex-col items-center justify-center space-y-4 max-w-md mx-auto text-center px-4">
          <div className="w-14 h-14 rounded-3xl bg-red-950/60 border border-red-800/60 text-red-400 flex items-center justify-center">
            <AlertCircle className="w-7 h-7" />
          </div>
          <div className="space-y-1">
            <h2 className="text-lg font-bold text-white">
              Assessment Unavailable
            </h2>
            <p className="text-xs text-slate-400">{errorMessage}</p>
          </div>
          <div className="flex items-center gap-3 pt-2">
            <Button
              variant="outline"
              size="md"
              onClick={() => router.push("/dashboard")}
              leftIcon={<ArrowLeft className="w-4 h-4" />}
            >
              Dashboard
            </Button>
            <Button
              variant="primary"
              size="md"
              className="bg-purple-600 hover:bg-purple-500"
              onClick={fetchAssessment}
              leftIcon={<RefreshCw className="w-4 h-4" />}
            >
              Retry Evaluation
            </Button>
          </div>
        </div>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <div className="space-y-8 max-w-5xl mx-auto">
        {/* Assessment Hero Header */}
        <AssessmentHero assessment={currentAssessment} />

        {/* 4-Category Score Grid */}
        <ScoreBreakdown assessment={currentAssessment} />

        {/* Strengths & Weaknesses */}
        <StrengthsAndWeaknesses
          strengths={currentAssessment.strengths}
          areasToImprove={currentAssessment.areasToImprove}
        />

        {/* Detailed Grammar Corrections */}
        <GrammarCorrectionsList corrections={currentAssessment.grammarCorrections} />

        {/* Vocabulary Improvements */}
        <VocabularySuggestionsList suggestions={currentAssessment.vocabularySuggestions} />

        {/* Next Recommendation Action Card */}
        <RecommendedActionCard recommendedPractice={currentAssessment.recommendedPractice} />
      </div>
    </AppShell>
  );
}

export default function PracticeResultPage() {
  return (
    <Suspense
      fallback={
        <div className="min-h-screen bg-slate-950 flex items-center justify-center text-sm font-semibold text-slate-400">
          Loading conversation assessment...
        </div>
      }
    >
      <ResultContent />
    </Suspense>
  );
}
