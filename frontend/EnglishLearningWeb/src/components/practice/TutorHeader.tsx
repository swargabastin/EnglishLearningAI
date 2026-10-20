"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link";
import { Sparkles, Clock, LogOut, ArrowLeft, Bot, CheckCircle2 } from "lucide-react";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Modal } from "@/components/ui/Modal";

export interface TutorHeaderProps {
  topicTitle: string;
  englishLevel: string;
  onEndSession: () => void;
  messageCount?: number;
}

export function TutorHeader({
  topicTitle,
  englishLevel,
  onEndSession,
  messageCount = 0,
}: TutorHeaderProps) {
  const [secondsElapsed, setSecondsElapsed] = useState(0);
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);

  useEffect(() => {
    const timer = setInterval(() => {
      setSecondsElapsed((prev) => prev + 1);
    }, 1000);
    return () => clearInterval(timer);
  }, []);

  const formatTimer = (totalSeconds: number) => {
    const mins = Math.floor(totalSeconds / 60);
    const secs = totalSeconds % 60;
    return `${mins.toString().padStart(2, "0")}:${secs.toString().padStart(2, "0")}`;
  };

  return (
    <header className="bg-slate-900/90 border border-slate-800 rounded-3xl p-3.5 sm:p-4.5 shadow-xl transition-all">
      <div className="flex items-center justify-between gap-3">
        {/* Left: Back Link & Emma Coach Profile */}
        <div className="flex items-center space-x-3">
          <Link
            href="/topics"
            className="p-2 rounded-2xl text-slate-400 hover:text-white hover:bg-slate-800 transition-colors flex-shrink-0"
            title="Back to topics"
            aria-label="Back to topics"
          >
            <ArrowLeft className="w-5 h-5" />
          </Link>

          {/* Emma AI Tutor Avatar & Title */}
          <div className="flex items-center space-x-3">
            <div className="relative flex-shrink-0">
              <div className="w-10 h-10 sm:w-11 sm:h-11 rounded-2xl bg-gradient-to-br from-purple-600 to-indigo-600 flex items-center justify-center text-white font-extrabold shadow-md ring-2 ring-purple-500/30">
                <Bot className="w-5 h-5 sm:w-6 sm:h-6" />
              </div>
              <span
                className="absolute -bottom-0.5 -right-0.5 w-3.5 h-3.5 rounded-full bg-emerald-400 ring-2 ring-slate-900 animate-pulse"
                title="Emma is online"
              />
            </div>

            <div className="min-w-0">
              <div className="flex items-center space-x-2">
                <h1 className="text-sm sm:text-base font-extrabold text-white tracking-tight truncate">
                  Emma · AI Coach
                </h1>
                <span className="inline-flex items-center space-x-1 px-2 py-0.5 rounded-md bg-purple-950/60 text-purple-300 border border-purple-800/50 text-[10px] font-bold">
                  <Sparkles className="w-2.5 h-2.5 text-purple-400" />
                  <span>Coach</span>
                </span>
              </div>
              <p className="text-xs text-slate-400 font-medium truncate max-w-[200px] sm:max-w-xs">
                {topicTitle}
              </p>
            </div>
          </div>
        </div>

        {/* Right: Live Session Duration & Finish Action */}
        <div className="flex items-center space-x-2 sm:space-x-3">
          {/* Active Duration Timer */}
          <div className="flex items-center space-x-1.5 px-3 py-1.5 bg-slate-800/80 border border-slate-700/60 rounded-2xl text-xs font-bold text-slate-200 shadow-xs">
            <Clock className="w-3.5 h-3.5 text-purple-400" />
            <span className="font-mono tracking-tight">{formatTimer(secondsElapsed)}</span>
          </div>

          <Badge cefrLevel={(englishLevel as any) || "B1"} size="sm" />

          {/* End Session Button */}
          <Button
            variant="outline"
            size="sm"
            onClick={() => setIsConfirmOpen(true)}
            className="border-slate-700 hover:border-red-500 hover:text-red-400 hover:bg-red-950/30 text-slate-300 font-bold"
            leftIcon={<LogOut className="w-3.5 h-3.5 text-red-400" />}
          >
            <span className="hidden sm:inline">Finish</span>
          </Button>
        </div>
      </div>

      {/* Confirmation Modal */}
      <Modal
        isOpen={isConfirmOpen}
        onClose={() => setIsConfirmOpen(false)}
        title="Finish Practice Session?"
        description="Emma will evaluate your conversation turns and generate a comprehensive CEFR speaking assessment."
      >
        <div className="space-y-4 pt-2">
          <div className="p-4 rounded-2xl bg-purple-950/40 border border-purple-800/40 text-xs text-purple-200 space-y-1">
            <p className="font-bold">Summary of this practice run:</p>
            <p>• Topic: <strong className="text-white">{topicTitle}</strong></p>
            <p>• Duration: <strong className="text-white">{formatTimer(secondsElapsed)}</strong></p>
            <p>• Dialogue Turns: <strong className="text-white">{messageCount}</strong></p>
          </div>

          <div className="flex items-center justify-end space-x-3 pt-2">
            <Button
              variant="outline"
              size="md"
              onClick={() => setIsConfirmOpen(false)}
            >
              Continue Speaking
            </Button>
            <Button
              variant="primary"
              size="md"
              className="bg-purple-600 hover:bg-purple-500"
              onClick={() => {
                setIsConfirmOpen(false);
                onEndSession();
              }}
              leftIcon={<CheckCircle2 className="w-4 h-4" />}
            >
              Finish & View Feedback
            </Button>
          </div>
        </div>
      </Modal>
    </header>
  );
}
