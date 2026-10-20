"use client";

import React, { useEffect, useState, useMemo } from "react";
import { AppShell } from "@/components/layout/AppShell";
import {
  BookOpen,
  Sparkles,
  Plus,
  Volume2,
  Trash2,
  CheckCircle2,
  Clock,
  RotateCw,
  Search,
  Filter,
  Layers,
  ArrowRight,
  ArrowLeft,
  X,
  Loader2,
  Bookmark,
} from "lucide-react";
import { vocabularyService } from "@/services/vocabularyService";
import { VocabularyItem } from "@/types";

const defaultStarterWords: VocabularyItem[] = [
  {
    id: "starter-1",
    word: "Articulate",
    meaning: "Able to express thoughts and ideas clearly and effectively (വ്യക്തമായി സംസാരിക്കാൻ കഴിയുന്ന)",
    example: "She gave an articulate presentation during the team meeting.",
    masteryLevel: 2,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: "starter-2",
    word: "Resilient",
    meaning: "Able to recover quickly from difficulties or setbacks (സ്ഥിരോത്സാഹമുള്ള)",
    example: "He remained resilient despite the challenging interview questions.",
    masteryLevel: 1,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: "starter-3",
    word: "Nuanced",
    meaning: "Characterized by subtle distinctions or shades of meaning (സൂക്ഷ്മമായ വ്യത്യാസങ്ങളുള്ള)",
    example: "Emma provided a nuanced explanation of professional email etiquette.",
    masteryLevel: 1,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: "starter-4",
    word: "Eloquent",
    meaning: "Fluent or persuasive in speaking or writing (ഹൃദ്യമായി സംസാരിക്കുന്ന)",
    example: "The speaker delivered an eloquent speech that inspired everyone.",
    masteryLevel: 3,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

export default function VocabularyPage() {
  const [items, setItems] = useState<VocabularyItem[]>(defaultStarterWords);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [filterMode, setFilterMode] = useState<"all" | "learning" | "mastered">("all");
  const [viewMode, setViewMode] = useState<"flashcards" | "list">("flashcards");

  // Flashcard state
  const [currentCardIndex, setCurrentCardIndex] = useState<number>(0);
  const [isFlipped, setIsFlipped] = useState<boolean>(false);

  // Add word modal state
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [newWord, setNewWord] = useState<string>("");
  const [newMeaning, setNewMeaning] = useState<string>("");
  const [newExample, setNewExample] = useState<string>("");
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  useEffect(() => {
    loadVocabulary();
  }, []);

  const loadVocabulary = async () => {
    setIsLoading(true);
    try {
      const data = await vocabularyService.getVocabulary();
      if (data && data.length > 0) {
        setItems(data);
      } else {
        setItems(defaultStarterWords);
      }
    } catch {
      setItems(defaultStarterWords);
    } finally {
      setIsLoading(false);
    }
  };

  const speakWord = (text: string) => {
    if (typeof window !== "undefined" && "speechSynthesis" in window) {
      window.speechSynthesis.cancel();
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = "en-US";
      utterance.rate = 0.9;
      const voices = window.speechSynthesis.getVoices();
      const natural = voices.find((v) => v.lang.startsWith("en") && (v.name.includes("Natural") || v.name.includes("Samantha"))) || voices.find((v) => v.lang.startsWith("en"));
      if (natural) utterance.voice = natural;
      window.speechSynthesis.speak(utterance);
    }
  };

  const filteredItems = useMemo(() => {
    return items.filter((item) => {
      const matchesSearch =
        item.word.toLowerCase().includes(searchQuery.toLowerCase()) ||
        item.meaning.toLowerCase().includes(searchQuery.toLowerCase());

      if (!matchesSearch) return false;

      if (filterMode === "learning") return item.masteryLevel < 3;
      if (filterMode === "mastered") return item.masteryLevel >= 3;
      return true;
    });
  }, [items, searchQuery, filterMode]);

  const currentCard = filteredItems[currentCardIndex] || filteredItems[0];

  const handleUpdateMastery = async (id: string, newLevel: number) => {
    try {
      const updated = await vocabularyService.updateMastery(id, newLevel);
      setItems((prev) => prev.map((item) => (item.id === id ? updated : item)));
    } catch {
      // Non-blocking
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await vocabularyService.deleteVocabulary(id);
      setItems((prev) => prev.filter((item) => item.id !== id));
      if (currentCardIndex >= items.length - 1) {
        setCurrentCardIndex(Math.max(0, items.length - 2));
      }
    } catch {
      // Non-blocking
    }
  };

  const handleAddWord = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newWord.trim() || !newMeaning.trim()) return;

    setIsSubmitting(true);
    try {
      const created = await vocabularyService.addVocabulary({
        word: newWord.trim(),
        meaning: newMeaning.trim(),
        example: newExample.trim() || `Example with "${newWord.trim()}"`,
      });

      setItems((prev) => [created, ...prev]);
      setNewWord("");
      setNewMeaning("");
      setNewExample("");
      setIsModalOpen(false);
    } catch {
      // Non-blocking
    } finally {
      setIsSubmitting(false);
    }
  };

  const nextCard = () => {
    setIsFlipped(false);
    setCurrentCardIndex((prev) => (prev + 1) % (filteredItems.length || 1));
  };

  const prevCard = () => {
    setIsFlipped(false);
    setCurrentCardIndex((prev) => (prev - 1 + (filteredItems.length || 1)) % (filteredItems.length || 1));
  };

  const masteredCount = items.filter((i) => i.masteryLevel >= 3).length;
  const learningCount = items.filter((i) => i.masteryLevel < 3).length;

  return (
    <AppShell>
      <div className="space-y-6 sm:space-y-8 max-w-5xl mx-auto pb-12 animate-in fade-in duration-300">
        {/* Header Hero */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-slate-900/90 border border-slate-800 rounded-3xl p-6 sm:p-8 shadow-xl relative overflow-hidden">
          <div className="absolute top-0 right-0 w-64 h-64 bg-purple-600/10 rounded-full blur-3xl pointer-events-none" />

          <div className="space-y-1.5 relative z-10">
            <div className="inline-flex items-center gap-2 px-3 py-1 bg-purple-950/60 border border-purple-800/50 rounded-full text-xs font-bold text-purple-300 uppercase tracking-wider">
              <Bookmark className="w-3.5 h-3.5 text-purple-400" />
              <span>Personal Notebook</span>
            </div>
            <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
              Vocabulary Bank & Flashcards
            </h1>
            <p className="text-xs sm:text-sm text-slate-400 max-w-lg leading-relaxed">
              Master new words and phrases introduced during your spoken conversations with Emma using spaced repetition.
            </p>
          </div>

          <div className="flex items-center gap-3 relative z-10 flex-shrink-0">
            <button
              type="button"
              onClick={() => setIsModalOpen(true)}
              className="inline-flex items-center space-x-2 px-4 py-2.5 bg-purple-600 hover:bg-purple-500 text-white rounded-2xl text-xs sm:text-sm font-bold shadow-lg shadow-purple-950/50 transition-all hover:scale-102"
            >
              <Plus className="w-4 h-4" />
              <span>Add New Word</span>
            </button>
          </div>
        </div>

        {/* Learning Statistics Bar */}
        <div className="grid grid-cols-3 gap-3 sm:gap-4">
          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4 sm:p-5 flex items-center space-x-3.5">
            <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-xl bg-purple-950/60 border border-purple-800/50 flex items-center justify-center text-purple-400">
              <BookOpen className="w-5 h-5 sm:w-6 sm:h-6" />
            </div>
            <div>
              <p className="text-[11px] sm:text-xs font-semibold text-slate-400">Total Saved</p>
              <p className="text-lg sm:text-2xl font-extrabold text-white">{items.length}</p>
            </div>
          </div>

          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4 sm:p-5 flex items-center space-x-3.5">
            <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-xl bg-amber-950/60 border border-amber-800/50 flex items-center justify-center text-amber-400">
              <Clock className="w-5 h-5 sm:w-6 sm:h-6" />
            </div>
            <div>
              <p className="text-[11px] sm:text-xs font-semibold text-slate-400">Learning</p>
              <p className="text-lg sm:text-2xl font-extrabold text-amber-300">{learningCount}</p>
            </div>
          </div>

          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4 sm:p-5 flex items-center space-x-3.5">
            <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-xl bg-emerald-950/60 border border-emerald-800/50 flex items-center justify-center text-emerald-400">
              <CheckCircle2 className="w-5 h-5 sm:w-6 sm:h-6" />
            </div>
            <div>
              <p className="text-[11px] sm:text-xs font-semibold text-slate-400">Mastered</p>
              <p className="text-lg sm:text-2xl font-extrabold text-emerald-400">{masteredCount}</p>
            </div>
          </div>
        </div>

        {/* View & Filter Toggle Controls */}
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
          {/* Mode Switcher: Flashcards vs Notebook List */}
          <div className="flex items-center bg-slate-900 border border-slate-800 rounded-2xl p-1 w-full sm:w-auto shadow-sm">
            <button
              type="button"
              onClick={() => setViewMode("flashcards")}
              className={`flex-1 sm:flex-none inline-flex items-center justify-center space-x-2 px-4 py-2 rounded-xl text-xs font-bold transition-all ${
                viewMode === "flashcards"
                  ? "bg-purple-600 text-white shadow-md"
                  : "text-slate-400 hover:text-white"
              }`}
            >
              <Layers className="w-3.5 h-3.5" />
              <span>Flashcard Deck</span>
            </button>
            <button
              type="button"
              onClick={() => setViewMode("list")}
              className={`flex-1 sm:flex-none inline-flex items-center justify-center space-x-2 px-4 py-2 rounded-xl text-xs font-bold transition-all ${
                viewMode === "list"
                  ? "bg-purple-600 text-white shadow-md"
                  : "text-slate-400 hover:text-white"
              }`}
            >
              <BookOpen className="w-3.5 h-3.5" />
              <span>Notebook View</span>
            </button>
          </div>

          {/* Search & Filter Bar */}
          <div className="flex items-center gap-2 w-full sm:w-auto">
            <div className="relative flex-1 sm:w-64">
              <Search className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Search words..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full bg-slate-900 border border-slate-800 text-slate-200 placeholder-slate-500 rounded-2xl pl-10 pr-4 py-2 text-xs font-medium focus:outline-none focus:border-purple-500 transition-colors"
              />
            </div>

            <div className="flex items-center bg-slate-900 border border-slate-800 rounded-2xl p-1 text-xs">
              <button
                type="button"
                onClick={() => setFilterMode("all")}
                className={`px-3 py-1.5 rounded-xl font-bold transition-all ${
                  filterMode === "all" ? "bg-slate-800 text-white" : "text-slate-400 hover:text-white"
                }`}
              >
                All
              </button>
              <button
                type="button"
                onClick={() => setFilterMode("learning")}
                className={`px-3 py-1.5 rounded-xl font-bold transition-all ${
                  filterMode === "learning" ? "bg-amber-950/60 text-amber-400" : "text-slate-400 hover:text-white"
                }`}
              >
                Learning
              </button>
              <button
                type="button"
                onClick={() => setFilterMode("mastered")}
                className={`px-3 py-1.5 rounded-xl font-bold transition-all ${
                  filterMode === "mastered" ? "bg-emerald-950/60 text-emerald-400" : "text-slate-400 hover:text-white"
                }`}
              >
                Mastered
              </button>
            </div>
          </div>
        </div>

        {/* Content Body */}
        {isLoading ? (
          <div className="py-20 text-center space-y-3">
            <Loader2 className="w-8 h-8 animate-spin text-purple-400 mx-auto" />
            <p className="text-sm font-semibold text-slate-400">Loading your vocabulary bank...</p>
          </div>
        ) : filteredItems.length === 0 ? (
          <div className="bg-slate-900/60 border border-slate-800 rounded-3xl p-12 text-center space-y-4 max-w-md mx-auto">
            <div className="w-14 h-14 rounded-2xl bg-purple-950/60 border border-purple-800/40 text-purple-400 flex items-center justify-center mx-auto">
              <BookOpen className="w-7 h-7" />
            </div>
            <div className="space-y-1">
              <h3 className="text-base font-extrabold text-white">No words found</h3>
              <p className="text-xs text-slate-400 leading-relaxed">
                {items.length === 0
                  ? "You haven't saved any words yet. Save tips during your practice sessions with Emma or add a custom word."
                  : "No words match your current search or filter."}
              </p>
            </div>
            <button
              type="button"
              onClick={() => setIsModalOpen(true)}
              className="inline-flex items-center space-x-2 px-4 py-2 bg-purple-600 hover:bg-purple-500 text-white rounded-xl text-xs font-bold transition-colors"
            >
              <Plus className="w-3.5 h-3.5" />
              <span>Add Your First Word</span>
            </button>
          </div>
        ) : viewMode === "flashcards" ? (
          /* ======================================================== */
          /* FLASHCARD STUDY VIEW */
          /* ======================================================== */
          <div className="max-w-xl mx-auto space-y-6">
            {/* Card Counter & Shuffle Indicator */}
            <div className="flex items-center justify-between text-xs font-bold text-slate-400 px-2">
              <span>
                Card {currentCardIndex + 1} of {filteredItems.length}
              </span>
              <span className="text-[11px] text-purple-400 uppercase tracking-wider">
                Tap card to flip
              </span>
            </div>

            {/* The 3D Interactive Flip Card */}
            <div
              onClick={() => setIsFlipped(!isFlipped)}
              className="min-h-[280px] sm:min-h-[320px] bg-gradient-to-br from-slate-900 via-slate-900/90 to-purple-950/30 border-2 border-slate-800 hover:border-purple-600/60 rounded-3xl p-6 sm:p-8 flex flex-col justify-between shadow-2xl cursor-pointer transition-all duration-300 relative group"
            >
              {/* Top Card Bar */}
              <div className="flex items-center justify-between">
                <span
                  className={`px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider ${
                    currentCard.masteryLevel >= 3
                      ? "bg-emerald-950/60 text-emerald-400 border border-emerald-800/60"
                      : "bg-amber-950/60 text-amber-400 border border-amber-800/60"
                  }`}
                >
                  {currentCard.masteryLevel >= 3 ? "Mastered" : "Learning"}
                </span>

                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    speakWord(currentCard.word);
                  }}
                  className="p-2.5 rounded-2xl bg-purple-950/60 hover:bg-purple-900 text-purple-300 border border-purple-800/50 transition-colors shadow-sm"
                  title="Listen to native pronunciation"
                >
                  <Volume2 className="w-4 h-4" />
                </button>
              </div>

              {/* Center Card Content */}
              <div className="text-center py-6 space-y-3">
                {!isFlipped ? (
                  /* Front: Word & Speaker */
                  <div className="space-y-2">
                    <h2 className="text-3xl sm:text-4xl font-extrabold text-white tracking-tight">
                      {currentCard.word}
                    </h2>
                    <p className="text-xs text-slate-400 font-medium">
                      Tap card to see definition & example
                    </p>
                  </div>
                ) : (
                  /* Back: Meaning & Example Sentence */
                  <div className="space-y-3 animate-in fade-in duration-200">
                    <div className="space-y-1">
                      <p className="text-xs font-bold uppercase tracking-wider text-purple-400">
                        Meaning:
                      </p>
                      <p className="text-base sm:text-lg font-semibold text-slate-100 leading-snug">
                        {currentCard.meaning}
                      </p>
                    </div>

                    {currentCard.example && (
                      <div className="pt-2 border-t border-slate-800/80">
                        <p className="text-xs font-bold uppercase tracking-wider text-slate-400">
                          Example:
                        </p>
                        <p className="text-xs sm:text-sm text-slate-300 italic pt-0.5">
                          &ldquo;{currentCard.example}&rdquo;
                        </p>
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Bottom Card Bar: Spaced Repetition Rating */}
              <div
                onClick={(e) => e.stopPropagation()}
                className="pt-4 border-t border-slate-800 flex flex-wrap items-center justify-between gap-2 text-xs font-bold"
              >
                <span className="text-slate-400 text-[11px]">Rate your recall:</span>
                <div className="flex items-center gap-1.5">
                  <button
                    type="button"
                    onClick={() => handleUpdateMastery(currentCard.id, 1)}
                    className={`px-3 py-1.5 rounded-xl transition-colors ${
                      currentCard.masteryLevel === 1
                        ? "bg-rose-950 text-rose-300 border border-rose-800"
                        : "bg-slate-800 hover:bg-slate-750 text-slate-300"
                    }`}
                  >
                    Hard
                  </button>
                  <button
                    type="button"
                    onClick={() => handleUpdateMastery(currentCard.id, 2)}
                    className={`px-3 py-1.5 rounded-xl transition-colors ${
                      currentCard.masteryLevel === 2
                        ? "bg-amber-950 text-amber-300 border border-amber-800"
                        : "bg-slate-800 hover:bg-slate-750 text-slate-300"
                    }`}
                  >
                    Good
                  </button>
                  <button
                    type="button"
                    onClick={() => handleUpdateMastery(currentCard.id, 3)}
                    className={`px-3 py-1.5 rounded-xl transition-colors ${
                      currentCard.masteryLevel >= 3
                        ? "bg-emerald-950 text-emerald-300 border border-emerald-800"
                        : "bg-slate-800 hover:bg-slate-750 text-slate-300"
                    }`}
                  >
                    Mastered
                  </button>
                </div>
              </div>
            </div>

            {/* Flashcard Carousel Controls */}
            <div className="flex items-center justify-between gap-4">
              <button
                type="button"
                onClick={prevCard}
                className="inline-flex items-center space-x-2 px-5 py-2.5 bg-slate-900 border border-slate-800 hover:bg-slate-800 text-slate-200 rounded-2xl text-xs font-bold transition-colors shadow-sm"
              >
                <ArrowLeft className="w-4 h-4" />
                <span>Previous</span>
              </button>

              <button
                type="button"
                onClick={() => setIsFlipped(!isFlipped)}
                className="inline-flex items-center space-x-1.5 text-xs font-bold text-purple-400 hover:text-purple-300"
              >
                <RotateCw className="w-3.5 h-3.5" />
                <span>Flip Card</span>
              </button>

              <button
                type="button"
                onClick={nextCard}
                className="inline-flex items-center space-x-2 px-5 py-2.5 bg-purple-600 hover:bg-purple-500 text-white rounded-2xl text-xs font-bold transition-colors shadow-md shadow-purple-950/40"
              >
                <span>Next</span>
                <ArrowRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        ) : (
          /* ======================================================== */
          /* NOTEBOOK LIST VIEW */
          /* ======================================================== */
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {filteredItems.map((item) => (
              <div
                key={item.id}
                className="bg-slate-900/90 border border-slate-800 hover:border-slate-700/80 rounded-2xl p-5 shadow-lg space-y-3 transition-all"
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="space-y-0.5">
                    <div className="flex items-center gap-2">
                      <h3 className="text-lg font-extrabold text-white">{item.word}</h3>
                      <button
                        type="button"
                        onClick={() => speakWord(item.word)}
                        className="text-purple-400 hover:text-purple-300 p-1"
                        title="Listen"
                      >
                        <Volume2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                    <p className="text-xs text-slate-300 font-medium leading-relaxed">
                      {item.meaning}
                    </p>
                  </div>

                  <button
                    type="button"
                    onClick={() => handleDelete(item.id)}
                    className="text-slate-500 hover:text-rose-400 p-1.5 rounded-lg hover:bg-rose-950/30 transition-colors"
                    title="Remove word"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>

                {item.example && (
                  <div className="bg-slate-950/60 rounded-xl p-3 border border-slate-800/80 text-xs text-slate-400 italic">
                    &ldquo;{item.example}&rdquo;
                  </div>
                )}

                <div className="pt-2 border-t border-slate-800/80 flex items-center justify-between text-xs">
                  <span className="text-[11px] font-semibold text-slate-500">Mastery:</span>
                  <div className="flex items-center gap-1.5">
                    <button
                      type="button"
                      onClick={() => handleUpdateMastery(item.id, 1)}
                      className={`px-2 py-0.5 rounded-lg font-bold text-[10px] ${
                        item.masteryLevel === 1 ? "bg-rose-950 text-rose-300" : "text-slate-400"
                      }`}
                    >
                      Hard
                    </button>
                    <button
                      type="button"
                      onClick={() => handleUpdateMastery(item.id, 2)}
                      className={`px-2 py-0.5 rounded-lg font-bold text-[10px] ${
                        item.masteryLevel === 2 ? "bg-amber-950 text-amber-300" : "text-slate-400"
                      }`}
                    >
                      Good
                    </button>
                    <button
                      type="button"
                      onClick={() => handleUpdateMastery(item.id, 3)}
                      className={`px-2 py-0.5 rounded-lg font-bold text-[10px] ${
                        item.masteryLevel >= 3 ? "bg-emerald-950 text-emerald-400" : "text-slate-400"
                      }`}
                    >
                      Mastered
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Add Word Modal */}
        {isModalOpen && (
          <div className="fixed inset-0 z-50 bg-slate-950/80 backdrop-blur-xs flex items-center justify-center p-4">
            <div className="bg-slate-900 border border-slate-800 rounded-3xl p-6 sm:p-8 max-w-md w-full shadow-2xl space-y-5 animate-in zoom-in-95 duration-200">
              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <h3 className="text-lg font-extrabold text-white">Add New Word or Phrase</h3>
                  <p className="text-xs text-slate-400">Save a phrase to practice with flashcards</p>
                </div>
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="text-slate-400 hover:text-white p-1 rounded-lg"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              <form onSubmit={handleAddWord} className="space-y-4">
                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-slate-300">
                    Word or Expression *
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. Articulate, In other words"
                    value={newWord}
                    onChange={(e) => setNewWord(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-800 rounded-2xl px-4 py-2.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-purple-500"
                  />
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-slate-300">
                    Meaning / Definition *
                  </label>
                  <textarea
                    required
                    rows={2}
                    placeholder="e.g. Expressing ideas clearly and fluently"
                    value={newMeaning}
                    onChange={(e) => setNewMeaning(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-800 rounded-2xl px-4 py-2.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-purple-500 resize-none"
                  />
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-slate-300">
                    Example Sentence (Optional)
                  </label>
                  <input
                    type="text"
                    placeholder="e.g. She gave an articulate response in the interview."
                    value={newExample}
                    onChange={(e) => setNewExample(e.target.value)}
                    className="w-full bg-slate-950 border border-slate-800 rounded-2xl px-4 py-2.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-purple-500"
                  />
                </div>

                <div className="flex items-center justify-end gap-3 pt-2">
                  <button
                    type="button"
                    onClick={() => setIsModalOpen(false)}
                    className="px-4 py-2 rounded-xl text-xs font-bold text-slate-400 hover:text-white"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={isSubmitting || !newWord.trim() || !newMeaning.trim()}
                    className="px-5 py-2.5 bg-purple-600 hover:bg-purple-500 disabled:opacity-50 text-white rounded-xl text-xs font-bold shadow-md transition-all flex items-center space-x-1.5"
                  >
                    {isSubmitting ? (
                      <>
                        <Loader2 className="w-3.5 h-3.5 animate-spin" />
                        <span>Saving...</span>
                      </>
                    ) : (
                      <span>Save Word</span>
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </AppShell>
  );
}
