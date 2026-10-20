"use client";

import React from "react";
import { CheckCircle2, Target } from "lucide-react";
import { LearningGoal } from "@/types";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/Card";

export interface GoalSelectorProps {
  currentGoal: LearningGoal;
  onSelectGoal: (goal: LearningGoal) => void;
}

export function GoalSelector({ currentGoal, onSelectGoal }: GoalSelectorProps) {
  const goals: { goal: LearningGoal; description: string }[] = [
    {
      goal: "Speaking Fluency",
      description: "Build spoken confidence, reduce hesitation, and maintain continuous conversational rhythm.",
    },
    {
      goal: "Workplace English",
      description: "Master professional terminology, meeting contributions, and cross-functional discussions.",
    },
    {
      goal: "Job Interviews",
      description: "Structure crisp answers for behavioral interview questions using STAR phrasing.",
    },
    {
      goal: "Grammar & Accuracy",
      description: "Sharpen verb tenses, prepositions, articles, and complex sentence structures.",
    },
    {
      goal: "Travel & Daily Life",
      description: "Practical dialogues for navigation, dining, shopping, airport logistics, and hotels.",
    },
    {
      goal: "Vocabulary Expansion",
      description: "Replace repetitive words with rich, idiomatic, and contextually varied language.",
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base font-bold text-white">Primary Learning Goal</CardTitle>
        <CardDescription>
          Your selected learning focus influences Emma&apos;s follow-up questions and practice scenario recommendations.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {goals.map((item) => {
            const isSelected = currentGoal === item.goal;
            return (
              <button
                key={item.goal}
                type="button"
                onClick={() => onSelectGoal(item.goal)}
                className={`p-4 rounded-2xl border text-left transition-all flex flex-col justify-between select-none ${
                  isSelected
                    ? "border-purple-500 bg-purple-950/40 ring-2 ring-purple-500 text-white shadow-lg shadow-purple-950/30"
                    : "border-slate-800 hover:border-slate-700 bg-slate-850 text-slate-300"
                }`}
              >
                <div>
                  <div className="flex items-center justify-between mb-1.5">
                    <span className="font-extrabold text-sm text-white">
                      {item.goal}
                    </span>
                    {isSelected && (
                      <CheckCircle2 className="w-4 h-4 text-purple-400" />
                    )}
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {item.description}
                  </p>
                </div>
              </button>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
