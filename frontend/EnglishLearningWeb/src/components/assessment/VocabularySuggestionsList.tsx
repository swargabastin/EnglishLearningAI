import React from "react";
import { Sparkles, ArrowRight, BookOpen } from "lucide-react";
import { VocabularySuggestion } from "@/types";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function VocabularySuggestionsList({
  suggestions,
}: {
  suggestions: VocabularySuggestion[];
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <div className="flex items-center space-x-2.5">
          <div className="p-2 rounded-xl bg-indigo-50 text-indigo-600">
            <Sparkles className="w-5 h-5" />
          </div>
          <div>
            <CardTitle className="text-base font-bold text-slate-900">
              Vocabulary Enhancements
            </CardTitle>
            <p className="text-xs text-slate-500">
              Elevate simple phrasing with natural, advanced alternatives
            </p>
          </div>
        </div>
        <Badge variant="info" size="sm">
          {suggestions.length} Suggestions
        </Badge>
      </CardHeader>

      <CardContent className="space-y-3">
        {suggestions.map((item) => (
          <div
            key={item.id}
            className="p-4 rounded-xl border border-slate-100 bg-slate-50/50 hover:border-slate-300 transition-colors flex flex-col sm:flex-row sm:items-center justify-between gap-3"
          >
            <div className="space-y-1">
              <div className="flex items-center space-x-2 text-xs">
                <span className="font-semibold text-slate-500 line-through">
                  &ldquo;{item.originalWord}&rdquo;
                </span>
                <ArrowRight className="w-3.5 h-3.5 text-slate-400" />
                <span className="font-bold text-blue-700 text-sm">
                  &ldquo;{item.suggestedAlternative}&rdquo;
                </span>
              </div>
              <p className="text-xs text-slate-500">
                <span className="font-medium text-slate-700">Context: </span>
                {item.context}
              </p>
            </div>

            <Badge cefrLevel={item.cefrLevel} size="sm" className="self-start sm:self-center" />
          </div>
        ))}
      </CardContent>
    </Card>
  );
}
