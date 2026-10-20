import React from "react";
import { CheckCircle2, TrendingUp } from "lucide-react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/Card";

export function StrengthsAndWeaknesses({
  strengths,
  areasToImprove,
}: {
  strengths: string[];
  areasToImprove: string[];
}) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {/* Strengths Card */}
      <Card className="border-emerald-100 bg-emerald-50/20">
        <CardHeader className="flex flex-row items-center space-x-2.5 pb-2">
          <div className="p-2 rounded-xl bg-emerald-100 text-emerald-700">
            <CheckCircle2 className="w-5 h-5" />
          </div>
          <div>
            <CardTitle className="text-base font-bold text-slate-900">
              Demonstrated Strengths
            </CardTitle>
            <p className="text-xs text-slate-500">
              Areas where your spoken English was effective
            </p>
          </div>
        </CardHeader>
        <CardContent>
          <ul className="space-y-2.5">
            {strengths.map((item, idx) => (
              <li
                key={idx}
                className="flex items-start space-x-2.5 text-xs sm:text-sm text-slate-700 leading-relaxed"
              >
                <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 flex-shrink-0 mt-2" />
                <span>{item}</span>
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>

      {/* Areas to Improve Card */}
      <Card className="border-amber-100 bg-amber-50/20">
        <CardHeader className="flex flex-row items-center space-x-2.5 pb-2">
          <div className="p-2 rounded-xl bg-amber-100 text-amber-800">
            <TrendingUp className="w-5 h-5" />
          </div>
          <div>
            <CardTitle className="text-base font-bold text-slate-900">
              Areas for Improvement
            </CardTitle>
            <p className="text-xs text-slate-500">
              Targeted skills to sharpen in your next session
            </p>
          </div>
        </CardHeader>
        <CardContent>
          <ul className="space-y-2.5">
            {areasToImprove.map((item, idx) => (
              <li
                key={idx}
                className="flex items-start space-x-2.5 text-xs sm:text-sm text-slate-700 leading-relaxed"
              >
                <span className="w-1.5 h-1.5 rounded-full bg-amber-500 flex-shrink-0 mt-2" />
                <span>{item}</span>
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
