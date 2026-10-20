"use client";

import React from "react";
import { CheckCircle2 } from "lucide-react";
import { CEFRLevel } from "@/types";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/Card";

export interface LevelSelectorProps {
  currentLevel: CEFRLevel;
  onSelectLevel: (level: CEFRLevel) => void;
}

export function LevelSelector({ currentLevel, onSelectLevel }: LevelSelectorProps) {
  const levels: { level: CEFRLevel; name: string; description: string }[] = [
    {
      level: "A1",
      name: "Beginner",
      description: "Simple sentences, basic introductions, and fundamental everyday vocabulary.",
    },
    {
      level: "A2",
      name: "Elementary",
      description: "Routine tasks, asking for directions, shopping, and familiar subjects.",
    },
    {
      level: "B1",
      name: "Intermediate",
      description: "Workplace conversations, travel problem-solving, and expressing opinions.",
    },
    {
      level: "B2",
      name: "Upper Intermediate",
      description: "Spontaneous discussions, technical nuances, and complex reasoning.",
    },
    {
      level: "C1",
      name: "Advanced",
      description: "Fluent, structured communication across professional and social contexts.",
    },
    {
      level: "C2",
      name: "Proficient",
      description: "Native-like precision, subtle emotional nuances, and effortless expression.",
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base font-bold text-white">English Proficiency Level (CEFR)</CardTitle>
        <CardDescription>
          Your selected level adjusts the vocabulary complexity and sentence pacing of the AI tutor during speaking sessions.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {levels.map((item) => {
            const isSelected = currentLevel === item.level;
            return (
              <button
                key={item.level}
                type="button"
                onClick={() => onSelectLevel(item.level)}
                className={`p-4 rounded-2xl border text-left transition-all flex flex-col justify-between select-none ${
                  isSelected
                    ? "border-purple-500 bg-purple-950/40 ring-2 ring-purple-500 text-white shadow-lg shadow-purple-950/30"
                    : "border-slate-800 hover:border-slate-700 bg-slate-850 text-slate-300"
                }`}
              >
                <div>
                  <div className="flex items-center justify-between mb-1.5">
                    <span className="font-extrabold text-sm text-white">
                      {item.level} — {item.name}
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
