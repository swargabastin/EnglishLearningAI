import React from "react";
import Link from "next/link";
import { Sparkles, ArrowRight, Briefcase, Building2 } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function RecommendedTopicsCard() {
  const recommendations = [
    {
      id: "22222222-2222-2222-2222-222222222222",
      title: "Job Interview Practice",
      description: "Answer common behavioral questions with structured STAR phrasing.",
      level: "B2" as const,
      icon: Briefcase,
      duration: "15 min",
    },
    {
      id: "66666666-6666-6666-6666-666666666666",
      title: "Workplace Collaboration",
      description: "Practice giving project updates and negotiating team timelines.",
      level: "B1" as const,
      icon: Building2,
      duration: "15 min",
    },
  ];

  return (
    <Card className="h-full flex flex-col justify-between">
      <div>
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <div className="flex items-center space-x-2">
            <Sparkles className="w-4 h-4 text-blue-600" />
            <CardTitle className="text-base font-bold">Recommended for You</CardTitle>
          </div>
          <Badge variant="info" size="sm">Level B1 Focus</Badge>
        </CardHeader>

        <CardContent className="space-y-3">
          {recommendations.map((rec) => {
            const Icon = rec.icon;
            return (
              <div
                key={rec.id}
                className="p-4 rounded-xl border border-slate-100 bg-slate-50/50 hover:border-blue-200 hover:bg-blue-50/30 transition-all flex items-start justify-between gap-3"
              >
                <div className="flex items-start space-x-3">
                  <div className="p-2.5 rounded-xl bg-white border border-slate-200 text-blue-600 shadow-xs flex-shrink-0">
                    <Icon className="w-5 h-5" />
                  </div>
                  <div className="space-y-1">
                    <div className="flex items-center space-x-2">
                      <h4 className="text-sm font-bold text-slate-800">{rec.title}</h4>
                      <Badge cefrLevel={rec.level} size="sm" />
                    </div>
                    <p className="text-xs text-slate-500 line-clamp-2">
                      {rec.description}
                    </p>
                  </div>
                </div>

                <Link
                  href={`/practice?topic=${rec.id}`}
                  className="flex-shrink-0 p-2 rounded-lg bg-blue-600 hover:bg-blue-700 text-white shadow-xs transition-colors self-center"
                  title="Start topic"
                >
                  <ArrowRight className="w-4 h-4" />
                </Link>
              </div>
            );
          })}
        </CardContent>
      </div>

      <div className="p-4 bg-slate-50/80 border-t border-slate-100 rounded-b-2xl flex items-center justify-between text-xs">
        <span className="text-slate-500">Need a different scenario?</span>
        <Link
          href="/topics"
          className="font-bold text-blue-600 hover:text-blue-700 flex items-center space-x-1"
        >
          <span>Browse 7 topics</span>
          <ArrowRight className="w-3 h-3" />
        </Link>
      </div>
    </Card>
  );
}
