import React from "react";
import Link from "next/link";
import { Compass, ArrowRight, BookMarked } from "lucide-react";
import { Button } from "@/components/ui/Button";

export function RecommendedActionCard({
  recommendedPractice,
}: {
  recommendedPractice: {
    title: string;
    description: string;
    actionTopicId?: string;
  };
}) {
  return (
    <div className="bg-gradient-to-r from-blue-900 to-indigo-900 text-white rounded-3xl p-6 sm:p-8 shadow-md flex flex-col md:flex-row md:items-center md:justify-between gap-6">
      <div className="flex items-start space-x-4 max-w-xl">
        <div className="p-3 rounded-2xl bg-white/10 text-white flex-shrink-0 backdrop-blur-sm">
          <BookMarked className="w-6 h-6 text-blue-300" />
        </div>
        <div className="space-y-1">
          <span className="text-xs font-bold uppercase tracking-wider text-blue-300">
            Recommended Next Step
          </span>
          <h3 className="text-lg font-bold text-white tracking-tight">
            {recommendedPractice.title}
          </h3>
          <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
            {recommendedPractice.description}
          </p>
        </div>
      </div>

      <Link href="/practice" className="flex-shrink-0">
        <Button
          variant="primary"
          size="lg"
          className="bg-white text-slate-900 hover:bg-slate-100 font-bold shadow-md w-full md:w-auto"
          rightIcon={<ArrowRight className="w-4 h-4 text-slate-900" />}
        >
          Launch Targeted Practice
        </Button>
      </Link>
    </div>
  );
}
