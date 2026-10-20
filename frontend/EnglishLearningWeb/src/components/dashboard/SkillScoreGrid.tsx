"use client";

import React from "react";
import { Mic, BookOpen, Sparkles, Activity } from "lucide-react";
import { Card } from "@/components/ui/Card";
import { ProgressBar } from "@/components/ui/ProgressBar";
import { ProgressStatsDto } from "@/services/progressService";

export interface SkillScoreGridProps {
  stats?: ProgressStatsDto;
}

export function SkillScoreGrid({ stats }: SkillScoreGridProps) {
  const overall = Math.round(stats?.averageOverallScore || 0);
  const grammar = Math.round(stats?.grammarAverage || 0);
  const vocab = Math.round(stats?.vocabularyAverage || 0);
  const fluency = Math.round(stats?.fluencyAverage || 0);

  const skills = [
    {
      name: "Overall",
      score: overall,
      label: stats?.totalCompletedSessions ? `${stats.totalCompletedSessions} assessed sessions` : "No sessions yet",
      icon: Mic,
      color: "blue",
      desc: "Overall speaking performance",
    },
    {
      name: "Grammar",
      score: grammar,
      label: grammar > 0 ? `${grammar}% mastery` : "Pending evaluation",
      icon: BookOpen,
      color: "emerald",
      desc: "Tenses & sentence syntax",
    },
    {
      name: "Vocabulary",
      score: vocab,
      label: vocab > 0 ? `${vocab}% lexical range` : "Pending evaluation",
      icon: Sparkles,
      color: "blue",
      desc: "Topic & vocabulary variety",
    },
    {
      name: "Fluency",
      score: fluency,
      label: fluency > 0 ? `${fluency}% flow & speed` : "Pending evaluation",
      icon: Activity,
      color: "emerald",
      desc: "Speaking rate & cohesion",
    },
  ];

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="text-base font-bold text-slate-900 tracking-tight">
          Current Skill Competencies
        </h3>
        <span className="text-xs text-slate-500">
          {stats?.totalCompletedSessions
            ? `Calculated from ${stats.totalCompletedSessions} completed sessions`
            : "Complete a practice session to see skill scores"}
        </span>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {skills.map((skill) => {
          const Icon = skill.icon;
          return (
            <Card
              key={skill.name}
              className="p-5 hover:border-slate-300 transition-colors"
            >
              <div className="flex items-start justify-between">
                <div className="p-2.5 rounded-xl bg-slate-50 border border-slate-100 text-slate-700">
                  <Icon className="w-5 h-5" />
                </div>
                <div className="text-right">
                  <span className="text-2xl font-extrabold text-slate-900">
                    {skill.score}%
                  </span>
                  <span className="block text-[11px] font-medium text-emerald-600">
                    {skill.label}
                  </span>
                </div>
              </div>

              <div className="mt-4 space-y-1.5">
                <p className="text-sm font-bold text-slate-800">{skill.name}</p>
                <p className="text-xs text-slate-500">{skill.desc}</p>
                <ProgressBar
                  value={skill.score}
                  max={100}
                  variant={skill.color as "blue" | "emerald"}
                  size="sm"
                  className="pt-2"
                />
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
