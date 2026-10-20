"use client";

import React from "react";
import {
  MessageCircle,
  Briefcase,
  Compass,
  ShoppingBag,
  UtensilsCrossed,
  Building2,
  UserCheck,
  Clock,
  CheckCircle2,
  Sparkles,
} from "lucide-react";
import { Topic } from "@/types";
import { Badge } from "@/components/ui/Badge";

const iconMap: Record<string, React.ComponentType<{ className?: string }>> = {
  MessageCircle,
  Briefcase,
  Compass,
  ShoppingBag,
  UtensilsCrossed,
  Building2,
  UserCheck,
};

export interface TopicCardProps {
  topic: Topic;
  isSelected: boolean;
  onSelect: (topic: Topic) => void;
}

export function TopicCard({ topic, isSelected, onSelect }: TopicCardProps) {
  const Icon = iconMap[topic.iconKey] || MessageCircle;

  return (
    <div
      onClick={() => onSelect(topic)}
      className={`relative p-5 sm:p-6 rounded-3xl border transition-all duration-200 cursor-pointer flex flex-col justify-between select-none ${
        isSelected
          ? "bg-slate-850 border-purple-500 ring-2 ring-purple-500 shadow-xl shadow-purple-950/50 transform -translate-y-1"
          : "bg-slate-900/90 border-slate-800 hover:border-slate-700 hover:bg-slate-850/80 hover:-translate-y-0.5"
      }`}
    >
      {/* Top Header */}
      <div className="space-y-4">
        <div className="flex items-start justify-between">
          <div
            className={`w-12 h-12 rounded-2xl flex items-center justify-center transition-colors ${
              isSelected
                ? "bg-gradient-to-tr from-purple-600 to-indigo-600 text-white shadow-md shadow-purple-600/30"
                : "bg-slate-800 text-purple-400 border border-slate-700"
            }`}
          >
            <Icon className="w-6 h-6" />
          </div>

          <div className="flex items-center space-x-2">
            {topic.popular && (
              <span className="inline-flex items-center space-x-1 px-2 py-0.5 rounded-lg bg-amber-950/40 text-amber-300 border border-amber-800/40 text-[10px] font-bold">
                <Sparkles className="w-3 h-3 text-amber-400" />
                <span>Popular</span>
              </span>
            )}
            <Badge cefrLevel={topic.difficulty} size="sm" />
          </div>
        </div>

        <div>
          <h3 className="text-base font-bold text-white tracking-tight">
            {topic.title}
          </h3>
          <p className="text-xs text-slate-400 mt-1.5 leading-relaxed line-clamp-2">
            {topic.description}
          </p>
        </div>
      </div>

      {/* Footer Info */}
      <div className="pt-4 mt-4 border-t border-slate-800 flex items-center justify-between text-xs">
        <div className="flex items-center space-x-1.5 text-slate-400">
          <Clock className="w-3.5 h-3.5" />
          <span>~{topic.estimatedMinutes} mins</span>
        </div>

        <div className="flex items-center space-x-1.5">
          {isSelected ? (
            <span className="flex items-center space-x-1 font-bold text-purple-400 text-xs">
              <CheckCircle2 className="w-4 h-4 text-purple-400" />
              <span>Selected</span>
            </span>
          ) : (
            <span className="text-slate-500 font-medium text-xs">
              Click to select
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
