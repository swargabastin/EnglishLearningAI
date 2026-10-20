"use client";

import { AppShell } from "@/components/layout/AppShell";
import { PageHeader } from "@/components/ui/PageHeader";
import { HistoryTable } from "@/components/history/HistoryTable";
import { useLanguage } from "@/context/LanguageContext";

export default function HistoryPage() {
  const { t } = useLanguage();

  return (
    <AppShell>
      <div className="space-y-6">
        <PageHeader
          title={t.learningHistory}
          description="Review past speaking dialogues, Emma's detailed feedback, and track your longitudinal CEFR growth."
        />
        <HistoryTable />
      </div>
    </AppShell>
  );
}
