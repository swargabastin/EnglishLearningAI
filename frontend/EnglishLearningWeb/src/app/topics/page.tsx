"use client";

import { AppShell } from "@/components/layout/AppShell";
import { PageHeader } from "@/components/ui/PageHeader";
import { TopicGrid } from "@/components/topics/TopicGrid";
import { useLanguage } from "@/context/LanguageContext";

export default function TopicsPage() {
  const { t } = useLanguage();

  return (
    <AppShell>
      <div className="space-y-6">
        <PageHeader
          title="Conversation Scenarios"
          description="Choose what you want to practice. Emma adapts dialogue complexity and vocabulary to your CEFR proficiency level."
        />
        <TopicGrid />
      </div>
    </AppShell>
  );
}
