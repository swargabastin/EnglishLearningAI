"use client";

import React from "react";
import Link from "next/link";
import { History, ArrowRight, Calendar, Clock, MessageSquare } from "lucide-react";
import { RecentSessionSummaryDto } from "@/services/progressService";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { CEFRLevel } from "@/types";

export interface RecentSessionsCardProps {
  sessions?: RecentSessionSummaryDto[];
}

export function RecentSessionsCard({ sessions = [] }: RecentSessionsCardProps) {
  return (
    <Card className="h-full flex flex-col justify-between">
      <div>
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <div className="flex items-center space-x-2">
            <History className="w-4 h-4 text-slate-400" />
            <CardTitle className="text-base font-bold">Recent Practice Sessions</CardTitle>
          </div>
          <Link
            href="/history"
            className="text-xs font-semibold text-blue-600 hover:text-blue-700 flex items-center space-x-1"
          >
            <span>View all</span>
            <ArrowRight className="w-3 h-3" />
          </Link>
        </CardHeader>

        <CardContent className="space-y-3">
          {sessions.length === 0 ? (
            <div className="p-8 text-center text-slate-400 space-y-2">
              <MessageSquare className="w-8 h-8 mx-auto text-slate-300" />
              <p className="text-xs font-medium">No completed practice sessions yet.</p>
              <Link
                href="/practice"
                className="inline-block text-xs font-bold text-blue-600 hover:underline"
              >
                Start your first conversation &rarr;
              </Link>
            </div>
          ) : (
            sessions.map((session) => (
              <Link
                key={session.sessionId}
                href={`/practice/result?session=${session.sessionId}`}
                className="group block p-3.5 rounded-xl border border-slate-100 hover:border-slate-300 hover:bg-slate-50/70 transition-all"
              >
                <div className="flex items-start justify-between">
                  <div className="space-y-1">
                    <div className="flex items-center space-x-2">
                      <span className="text-sm font-bold text-slate-800 group-hover:text-blue-600 transition-colors">
                        {session.topicName}
                      </span>
                      <Badge cefrLevel={(session.englishLevel as CEFRLevel) || "B1"} size="sm" />
                    </div>
                    <div className="flex items-center space-x-3 text-xs text-slate-400">
                      <span className="flex items-center space-x-1">
                        <Calendar className="w-3.5 h-3.5" />
                        <span>{new Date(session.startedAt).toLocaleDateString([], { month: "short", day: "numeric" })}</span>
                      </span>
                      <span className="flex items-center space-x-1">
                        <Clock className="w-3.5 h-3.5" />
                        <span>{session.durationMinutes} min</span>
                      </span>
                    </div>
                  </div>

                  <div className="text-right">
                    <span className="text-base font-extrabold text-slate-900">
                      {Math.round(session.overallScore)}
                    </span>
                    <span className="text-xs text-slate-400 block">/ 100</span>
                  </div>
                </div>
              </Link>
            ))
          )}
        </CardContent>
      </div>

      <div className="p-4 bg-slate-50/80 border-t border-slate-100 rounded-b-2xl text-center">
        <Link
          href="/history"
          className="text-xs font-medium text-slate-600 hover:text-slate-900"
        >
          View detailed score histories and past tutor transcripts
        </Link>
      </div>
    </Card>
  );
}
