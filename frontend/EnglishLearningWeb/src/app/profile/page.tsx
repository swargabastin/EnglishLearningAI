"use client";

import React, { useState, useEffect } from "react";
import { AppShell } from "@/components/layout/AppShell";
import { PageHeader } from "@/components/ui/PageHeader";
import { ProfileOverview } from "@/components/profile/ProfileOverview";
import { LevelSelector } from "@/components/profile/LevelSelector";
import { GoalSelector } from "@/components/profile/GoalSelector";
import { AudioPreferencesCard } from "@/components/profile/AudioPreferencesCard";
import { useAuth } from "@/context/AuthContext";
import { useLanguage } from "@/context/LanguageContext";
import { Loader2 } from "lucide-react";
import { CEFRLevel, LearningGoal, UserProfile } from "@/types";

export default function ProfilePage() {
  const { user: authUser, isLoading: authLoading } = useAuth();
  const { t } = useLanguage();
  const [user, setUser] = useState<UserProfile | null>(null);

  useEffect(() => {
    if (authUser) {
      setUser(authUser);
    }
  }, [authUser]);

  if (authLoading || !user) {
    return (
      <AppShell>
        <div className="flex flex-col items-center justify-center min-h-[60vh] space-y-4">
          <Loader2 className="w-8 h-8 text-purple-400 animate-spin" />
          <p className="text-sm text-slate-400">Loading your profile...</p>
        </div>
      </AppShell>
    );
  }

  const handleUpdateProfile = (name: string, email: string) => {
    setUser((prev) => (prev ? { ...prev, name, email } : null));
  };

  const handleSelectLevel = (level: CEFRLevel) => {
    setUser((prev) => (prev ? { ...prev, level } : null));
  };

  const handleSelectGoal = (goal: LearningGoal) => {
    setUser((prev) => (prev ? { ...prev, learningGoal: goal } : null));
  };

  const handleAvatarUpdated = (avatarUrl: string) => {
    setUser((prev) => (prev ? { ...prev, avatarUrl } : null));
  };

  return (
    <AppShell>
      <div className="space-y-8 max-w-4xl mx-auto">
        <PageHeader
          title={t.learnerProfile}
          description="Manage your personal details, English proficiency baseline, learning target, and Emma coach voice preferences."
        />

        {/* User Account Overview */}
        <ProfileOverview
          user={user}
          onSave={handleUpdateProfile}
          onAvatarUpdated={handleAvatarUpdated}
        />

        {/* CEFR Level Selection */}
        <LevelSelector
          currentLevel={user.level}
          onSelectLevel={handleSelectLevel}
        />

        {/* Learning Goal Selection */}
        <GoalSelector
          currentGoal={user.learningGoal}
          onSelectGoal={handleSelectGoal}
        />

        {/* Audio & Speech Preferences */}
        <AudioPreferencesCard />
      </div>
    </AppShell>
  );
}
