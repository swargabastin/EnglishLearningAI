"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { Award, BookOpen, Sparkles, Activity, MessageSquare, Loader2, ArrowRight } from "lucide-react";
import { HistoryItemDto, SessionDetailDto, progressService } from "@/services/progressService";
import { Modal } from "@/components/ui/Modal";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { CEFRLevel } from "@/types";

export interface SessionDetailModalProps {
  session: HistoryItemDto | null;
  onClose: () => void;
}

export function SessionDetailModal({ session, onClose }: SessionDetailModalProps) {
  const [detail, setDetail] = useState<SessionDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  useEffect(() => {
    let isMounted = true;
    if (session) {
      setIsLoading(true);
      progressService
        .getSessionDetail(session.sessionId)
        .then((res) => {
          if (isMounted) setDetail(res);
        })
        .catch(() => {
          // Ignored
        })
        .finally(() => {
          if (isMounted) setIsLoading(false);
        });
  } else {
      setDetail(null);
    }
    return () => {
      isMounted = false;
    };
  }, [session]);

  if (!session) return null;

  const formattedDate = new Date(session.startedAt).toLocaleDateString([], {
    month: "short",
    day: "numeric",
    year: "numeric",
  });

  return (
    <Modal
      isOpen={!!session}
      onClose={onClose}
      title={session.topicName}
      description={`Session completed on ${formattedDate} • Duration: ${session.durationMinutes} min`}
      maxWidth="lg"
    >
      <div className="space-y-5 pt-2">
        {/* Overall Score Banner */}
        <div className="p-5 bg-gradient-to-r from-purple-950/80 to-slate-900 border border-purple-800/50 rounded-2xl flex items-center justify-between">
          <div className="flex items-center space-x-3.5">
            <div className="p-3 bg-purple-600 rounded-2xl text-white shadow-md shadow-purple-600/30">
              <Award className="w-6 h-6" />
            </div>
            <div>
              <span className="text-[11px] uppercase tracking-wider text-purple-300 font-bold">
                Overall Session Score
              </span>
              <p className="text-2xl font-black text-white">
                {Math.round(session.overallScore)} / 100
              </p>
            </div>
          </div>
          <Badge cefrLevel={(session.englishLevel as CEFRLevel) || "B1"} size="md" />
        </div>

        {/* Skill Scores Grid */}
        <div className="grid grid-cols-3 gap-3 text-center">
          <div className="p-3.5 bg-slate-850 border border-slate-800 rounded-2xl space-y-1">
            <span className="text-xs text-slate-400 font-medium flex items-center justify-center gap-1">
              <BookOpen className="w-3.5 h-3.5 text-purple-400" />
              Grammar
            </span>
            <p className="text-lg font-extrabold text-white">
              {Math.round(session.grammarScore)}%
            </p>
          </div>
          <div className="p-3.5 bg-slate-850 border border-slate-800 rounded-2xl space-y-1">
            <span className="text-xs text-slate-400 font-medium flex items-center justify-center gap-1">
              <Sparkles className="w-3.5 h-3.5 text-indigo-400" />
              Vocabulary
            </span>
            <p className="text-lg font-extrabold text-white">
              {Math.round(session.vocabularyScore)}%
            </p>
          </div>
          <div className="p-3.5 bg-slate-850 border border-slate-800 rounded-2xl space-y-1">
            <span className="text-xs text-slate-400 font-medium flex items-center justify-center gap-1">
              <Activity className="w-3.5 h-3.5 text-emerald-400" />
              Fluency
            </span>
            <p className="text-lg font-extrabold text-white">
              {Math.round(session.fluencyScore)}%
            </p>
          </div>
        </div>

        {/* Feedback Section */}
        {detail?.assessment?.feedback && (
          <div className="p-4 bg-slate-850 border border-slate-800 rounded-2xl space-y-1.5">
            <h4 className="text-xs font-bold uppercase tracking-wider text-purple-300">
              Emma&apos;s Feedback
            </h4>
            <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
              {detail.assessment.feedback}
            </p>
          </div>
        )}

        {/* Transcript Conversation Turns */}
        <div className="space-y-3">
          <h4 className="text-xs font-bold uppercase tracking-wider text-slate-400">
            Conversation Transcript
          </h4>

          {isLoading ? (
            <div className="py-8 text-center text-slate-400 space-y-2">
              <Loader2 className="w-5 h-5 animate-spin mx-auto text-purple-400" />
              <p className="text-xs">Loading dialogue transcript...</p>
            </div>
          ) : detail?.messages && detail.messages.length > 0 ? (
            <div className="space-y-2.5 max-h-60 overflow-y-auto pr-1">
              {detail.messages.map((msg, index) => {
                const isUser = msg.role.toLowerCase() === "user";
                return (
                  <div
                    key={msg.id || index}
                    className={`p-3.5 rounded-2xl text-xs sm:text-sm leading-relaxed ${
                      isUser
                        ? "bg-purple-950/50 border border-purple-800/40 text-purple-100 ml-4"
                        : "bg-slate-850 border border-slate-800 text-slate-200 mr-4"
                    }`}
                  >
                    <div className="flex items-center justify-between text-[10px] font-bold text-slate-400 mb-1">
                      <span>{isUser ? "You · Learner" : "Emma · AI Coach"}</span>
                      <span>
                        {new Date(msg.createdAt).toLocaleTimeString([], {
                          hour: "2-digit",
                          minute: "2-digit",
                        })}
                      </span>
                    </div>
                    <p>{msg.messageText}</p>
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="p-4 bg-slate-850 rounded-xl text-center text-xs text-slate-400">
              No transcript messages recorded for this practice turn.
            </div>
          )}
        </div>

        {/* Footer Actions */}
        <div className="pt-2 flex justify-end gap-3 border-t border-slate-800">
          <Button variant="outline" size="sm" onClick={onClose}>
            Close
          </Button>
          <Link href={`/practice?session=${session.sessionId}`}>
            <Button
              variant="primary"
              size="sm"
              className="bg-purple-600 hover:bg-purple-500"
              rightIcon={<ArrowRight className="w-3.5 h-3.5" />}
            >
              Resume / Practice Again
            </Button>
          </Link>
        </div>
      </div>
    </Modal>
  );
}
