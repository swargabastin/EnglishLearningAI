"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { Search, Calendar, Clock, ArrowRight, MessageSquare, Loader2, ChevronLeft, ChevronRight, Filter } from "lucide-react";
import { HistoryItemDto, PaginatedListDto, progressService } from "@/services/progressService";
import { Badge } from "@/components/ui/Badge";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { SessionDetailModal } from "./SessionDetailModal";
import { useLanguage } from "@/context/LanguageContext";
import { CEFRLevel } from "@/types";

export function HistoryTable() {
  const { t } = useLanguage();
  const [searchTerm, setSearchTerm] = useState("");
  const [filterTab, setFilterTab] = useState<string>("All");
  const [page, setPage] = useState<number>(1);
  const [historyData, setHistoryData] = useState<PaginatedListDto<HistoryItemDto> | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [selectedSession, setSelectedSession] = useState<HistoryItemDto | null>(null);

  useEffect(() => {
    let isMounted = true;
    setIsLoading(true);

    progressService
      .getHistory(page, 15)
      .then((data) => {
        if (isMounted) {
          setHistoryData(data);
        }
      })
      .catch(() => {
        // Handle error
      })
      .finally(() => {
        if (isMounted) {
          setIsLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [page]);

  const items = historyData?.items || [];
  const filteredSessions = items.filter((s) => {
    const matchesSearch = s.topicName.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesSearch;
  });

  const averageScore =
    items.length > 0
      ? Math.round(items.reduce((acc, s) => acc + s.overallScore, 0) / items.length)
      : 0;

  return (
    <div className="space-y-6">
      {/* Search and Summary Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="relative max-w-sm w-full">
          <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search past topics..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 bg-slate-900 rounded-2xl border border-slate-800 text-sm text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-purple-500/30 focus:border-purple-500 shadow-xs"
          />
        </div>

        <div className="flex items-center space-x-4 text-xs text-slate-400">
          <span>
            Total Sessions: <strong className="text-white">{historyData?.totalCount || 0}</strong>
          </span>
          <span>•</span>
          <span>
            Average Score: <strong className="text-purple-400 font-bold">{averageScore}%</strong>
          </span>
        </div>
      </div>

      {/* Loading State */}
      {isLoading ? (
        <div className="p-12 text-center text-slate-400 space-y-3">
          <Loader2 className="w-6 h-6 animate-spin mx-auto text-purple-400" />
          <p className="text-sm font-semibold text-slate-400">Loading practice history...</p>
        </div>
      ) : filteredSessions.length === 0 ? (
        <div className="p-12 bg-slate-900/90 border border-slate-800 rounded-3xl text-center text-slate-400 space-y-4 shadow-lg">
          <div className="w-14 h-14 rounded-3xl bg-purple-950/50 border border-purple-800/40 text-purple-400 flex items-center justify-center mx-auto">
            <MessageSquare className="w-7 h-7" />
          </div>
          <div className="space-y-1">
            <p className="text-base font-bold text-white">
              {searchTerm ? "No practice sessions matched your search." : t.noSessionsYet}
            </p>
            <p className="text-xs text-slate-400 max-w-sm mx-auto">
              Complete speaking practice turns with Emma to build your learning journal.
            </p>
          </div>
          <Link
            href="/practice"
            className="inline-flex items-center space-x-1.5 px-4 py-2 bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold rounded-xl shadow-md transition-all hover:scale-102"
          >
            <span>{t.startWithEmma}</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </Link>
        </div>
      ) : (
        /* Sessions List */
        <div className="space-y-3">
          {filteredSessions.map((session) => (
            <div
              key={session.sessionId}
              onClick={() => setSelectedSession(session)}
              className="bg-slate-900/90 border border-slate-800 hover:border-purple-500/50 hover:bg-slate-850 rounded-2xl p-4 sm:p-5 transition-all cursor-pointer flex flex-col sm:flex-row sm:items-center justify-between gap-4 select-none group shadow-md"
            >
              {/* Left Info */}
              <div className="space-y-1.5">
                <div className="flex items-center space-x-2.5">
                  <h3 className="text-base font-bold text-white group-hover:text-purple-300 transition-colors">
                    {session.topicName}
                  </h3>
                  <Badge cefrLevel={(session.englishLevel as CEFRLevel) || "B1"} size="sm" />
                </div>

                <div className="flex flex-wrap items-center gap-3 text-xs text-slate-400">
                  <span className="flex items-center space-x-1">
                    <Calendar className="w-3.5 h-3.5 text-slate-500" />
                    <span>{new Date(session.startedAt).toLocaleDateString([], { month: "short", day: "numeric", year: "numeric" })}</span>
                  </span>
                  <span>•</span>
                  <span className="flex items-center space-x-1">
                    <Clock className="w-3.5 h-3.5 text-slate-500" />
                    <span>{session.durationMinutes} min practice</span>
                  </span>
                  <span>•</span>
                  <span className="flex items-center space-x-1">
                    <MessageSquare className="w-3.5 h-3.5 text-slate-500" />
                    <span>{session.messageCount} turns</span>
                  </span>
                </div>
              </div>

              {/* Right Scores */}
              <div className="flex items-center justify-between sm:justify-end space-x-4 border-t sm:border-t-0 pt-3 sm:pt-0 border-slate-800">
                <div className="flex items-center space-x-3 text-xs text-slate-300">
                  <div className="text-center px-2">
                    <span className="block text-[10px] text-slate-400 uppercase font-semibold">Grammar</span>
                    <span className="font-bold text-white">{Math.round(session.grammarScore)}%</span>
                  </div>
                  <div className="text-center px-2">
                    <span className="block text-[10px] text-slate-400 uppercase font-semibold">Vocab</span>
                    <span className="font-bold text-white">{Math.round(session.vocabularyScore)}%</span>
                  </div>
                  <div className="text-center px-2">
                    <span className="block text-[10px] text-slate-400 uppercase font-semibold">Fluency</span>
                    <span className="font-bold text-white">{Math.round(session.fluencyScore)}%</span>
                  </div>
                </div>

                <div className="flex items-center space-x-3 pl-2 border-l border-slate-800">
                  <div className="text-right">
                    <span className="text-lg font-black text-purple-400 block leading-tight">
                      {Math.round(session.overallScore)}
                    </span>
                    <span className="text-[10px] text-slate-400 font-semibold uppercase">Score</span>
                  </div>
                  <div className="p-2 rounded-xl bg-slate-800 text-slate-400 group-hover:text-white group-hover:bg-purple-600 transition-colors">
                    <ArrowRight className="w-4 h-4" />
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination Controls */}
      {historyData && historyData.totalPages > 1 && (
        <div className="flex items-center justify-between pt-4 border-t border-slate-800">
          <Button
            variant="outline"
            size="sm"
            disabled={!historyData.hasPreviousPage}
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            leftIcon={<ChevronLeft className="w-4 h-4" />}
          >
            Previous
          </Button>
          <span className="text-xs text-slate-400 font-medium">
            Page {historyData.page} of {historyData.totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={!historyData.hasNextPage}
            onClick={() => setPage((p) => p + 1)}
            rightIcon={<ChevronRight className="w-4 h-4" />}
          >
            Next
          </Button>
        </div>
      )}

      {/* Selected Session Detail Modal */}
      <SessionDetailModal
        session={selectedSession}
        onClose={() => setSelectedSession(null)}
      />
    </div>
  );
}
