"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ArrowRight, Sparkles, Filter, Briefcase, Clock, Compass, Mic } from "lucide-react";
import { Topic } from "@/types";
import { mockTopics } from "@/lib/mock";
import { topicService } from "@/services/topicService";
import { TopicCard } from "./TopicCard";
import { Button } from "@/components/ui/Button";
import { useAuth } from "@/context/AuthContext";
import { useLanguage } from "@/context/LanguageContext";
import { Badge } from "@/components/ui/Badge";

export function TopicGrid() {
  const router = useRouter();
  const { user } = useAuth();
  const { t } = useLanguage();

  const [topics, setTopics] = useState<Topic[]>(mockTopics);
  const [selectedTopic, setSelectedTopic] = useState<Topic>(mockTopics[1]); // Default to Job Interview
  const [filterCategory, setFilterCategory] = useState<string>("All");
  const [isLoading, setIsLoading] = useState<boolean>(false);

  useEffect(() => {
    topicService.getTopics().then((res) => {
      if (res && res.length > 0) {
        setTopics(res);
        const matchSelected = res.find((t) => t.id === "22222222-2222-2222-2222-222222222222");
        if (matchSelected) {
          setSelectedTopic(matchSelected);
        }
      }
    });
  }, []);

  const filterOptions = [
    { label: "All Topics (7)", value: "All" },
    { label: "Speaking", value: "Speaking" },
    { label: "Professional", value: "Professional" },
    { label: "Situational", value: "Situational" },
    { label: "General", value: "General" },
  ];

  const filteredTopics = topics.filter((t) => {
    if (filterCategory === "All") return true;
    if (filterCategory === "Speaking") return true;
    return t.category === filterCategory;
  });

  const recommendedTopic =
    topics.find((t) => t.id === "22222222-2222-2222-2222-222222222222") || topics[0];

  const handleStartPractice = (topicId?: string) => {
    const targetId = topicId || selectedTopic.id;
    router.push(`/practice?topic=${targetId}`);
  };

  return (
    <div className="space-y-6 sm:space-y-8">
      {/* Featured / Recommended Topic Card */}
      <div className="p-6 sm:p-8 bg-gradient-to-br from-purple-950/70 via-slate-900 to-slate-900 border border-purple-800/50 rounded-3xl shadow-xl space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="flex items-center space-x-2">
            <span className="px-3 py-1 bg-purple-900/60 border border-purple-700/50 text-purple-300 rounded-full text-xs font-bold uppercase tracking-wider flex items-center space-x-1.5">
              <Sparkles className="w-3.5 h-3.5 text-purple-400" />
              <span>{t.recommendedForYou}</span>
            </span>
            <Badge cefrLevel={recommendedTopic.difficulty} size="sm" />
          </div>
          <div className="flex items-center space-x-2 text-xs text-slate-400">
            <Clock className="w-3.5 h-3.5" />
            <span>~{recommendedTopic.estimatedMinutes} min practice</span>
          </div>
        </div>

        <div className="space-y-1">
          <h2 className="text-xl sm:text-2xl font-extrabold text-white">
            {recommendedTopic.title}
          </h2>
          <p className="text-xs sm:text-sm text-slate-300 max-w-2xl leading-relaxed">
            {recommendedTopic.description}
          </p>
        </div>

        <div className="pt-2 flex items-center gap-3">
          <Button
            variant="primary"
            size="md"
            className="bg-purple-600 hover:bg-purple-500 shadow-lg shadow-purple-950/50"
            onClick={() => handleStartPractice(recommendedTopic.id)}
            leftIcon={<Mic className="w-4 h-4" />}
          >
            {t.startPractice}
          </Button>
          <button
            type="button"
            onClick={() => setSelectedTopic(recommendedTopic)}
            className="text-xs font-bold text-slate-400 hover:text-slate-200 px-3 py-2"
          >
            Select Scenario
          </button>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex items-center space-x-2 overflow-x-auto pb-1">
        <span className="text-xs font-semibold text-slate-400 flex items-center gap-1 mr-1">
          <Filter className="w-3.5 h-3.5" />
          Filter:
        </span>
        {filterOptions.map((opt) => (
          <button
            key={opt.value}
            onClick={() => setFilterCategory(opt.value)}
            className={`px-3.5 py-1.5 rounded-xl text-xs font-semibold transition-all whitespace-nowrap ${
              filterCategory === opt.value
                ? "bg-purple-600 text-white shadow-md shadow-purple-950/40"
                : "bg-slate-900 border border-slate-800 text-slate-400 hover:bg-slate-850 hover:text-slate-200"
            }`}
          >
            {opt.label}
          </button>
        ))}
      </div>

      {/* Topics Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-5">
        {filteredTopics.map((topic) => (
          <TopicCard
            key={topic.id}
            topic={topic}
            isSelected={selectedTopic.id === topic.id}
            onSelect={(t) => setSelectedTopic(t)}
          />
        ))}
      </div>

      {/* Selected Topic Sticky Action Bar */}
      <div className="sticky bottom-20 md:bottom-6 z-20 bg-slate-900/95 backdrop-blur-xl text-white rounded-2xl p-4 sm:p-5 shadow-2xl border border-slate-700/80 flex flex-col sm:flex-row items-center justify-between gap-4 animate-in slide-in-from-bottom-2 duration-200">
        <div className="flex items-center space-x-3 text-center sm:text-left">
          <div className="w-10 h-10 rounded-xl bg-purple-950/60 border border-purple-700/50 text-purple-400 flex items-center justify-center flex-shrink-0">
            <Compass className="w-5 h-5" />
          </div>
          <div>
            <div className="flex items-center space-x-2 justify-center sm:justify-start">
              <span className="text-xs text-slate-400">Selected Scenario:</span>
              <span className="text-sm font-bold text-white">{selectedTopic.title}</span>
            </div>
            <p className="text-[11px] text-slate-400">
              Level {selectedTopic.difficulty} · ~{selectedTopic.estimatedMinutes} mins speaking
            </p>
          </div>
        </div>

        <Button
          variant="primary"
          size="md"
          className="w-full sm:w-auto px-6 py-2.5 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white font-bold rounded-xl shadow-lg shadow-purple-900/40"
          onClick={() => handleStartPractice(selectedTopic.id)}
          rightIcon={<ArrowRight className="w-4 h-4" />}
        >
          {t.startPractice}
        </Button>
      </div>
    </div>
  );
}
