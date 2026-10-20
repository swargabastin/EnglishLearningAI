import React from "react";
import { BookCheck, AlertCircle, CheckCircle, ArrowRight } from "lucide-react";
import { GrammarCorrection } from "@/types";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function GrammarCorrectionsList({
  corrections,
}: {
  corrections: GrammarCorrection[];
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <div className="flex items-center space-x-2.5">
          <div className="p-2 rounded-xl bg-blue-50 text-blue-600">
            <BookCheck className="w-5 h-5" />
          </div>
          <div>
            <CardTitle className="text-base font-bold text-slate-900">
              Grammar Corrections & Explanations
            </CardTitle>
            <p className="text-xs text-slate-500">
              Exact sentence improvements identified during your speaking turns
            </p>
          </div>
        </div>
        <Badge variant="info" size="sm">
          {corrections.length} Corrections
        </Badge>
      </CardHeader>

      <CardContent className="space-y-4">
        {corrections.map((item) => (
          <div
            key={item.id}
            className="p-4 sm:p-5 rounded-xl border border-slate-200/80 bg-slate-50/40 space-y-3"
          >
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-slate-700 uppercase tracking-wide">
                Rule: {item.ruleCategory}
              </span>
            </div>

            {/* Original vs Correction Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs sm:text-sm">
              {/* Original */}
              <div className="p-3 bg-red-50/60 border border-red-200/70 rounded-xl space-y-1">
                <span className="text-[11px] font-bold text-red-700 uppercase tracking-wider flex items-center gap-1">
                  <AlertCircle className="w-3.5 h-3.5" />
                  Your Sentence
                </span>
                <p className="text-slate-800 line-through decoration-red-400 font-medium">
                  &ldquo;{item.original}&rdquo;
                </p>
              </div>

              {/* Corrected */}
              <div className="p-3 bg-emerald-50/60 border border-emerald-200/70 rounded-xl space-y-1">
                <span className="text-[11px] font-bold text-emerald-700 uppercase tracking-wider flex items-center gap-1">
                  <CheckCircle className="w-3.5 h-3.5" />
                  Tutor Correction
                </span>
                <p className="text-emerald-950 font-bold">
                  &ldquo;{item.corrected}&rdquo;
                </p>
              </div>
            </div>

            {/* Explanation */}
            <div className="p-3 bg-white border border-slate-200/70 rounded-xl text-xs text-slate-600 leading-relaxed">
              <span className="font-bold text-slate-800">Grammar Tip: </span>
              {item.explanation}
            </div>
          </div>
        ))}
      </CardContent>
    </Card>
  );
}
