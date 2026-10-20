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
import { mockUser as initialUser } from "@/lib/mock";
import { CEFRLevel, LearningGoal, UserProfile } from "@/types";

export default function ProfilePage() {
  const { user: authUser } = useAuth();
  const { t } = useLanguage();
  const [user, setUser] = useState<UserProfile>(authUser || initialUser);

  useEffect(() => {
    if (authUser) {
      setUser(authUser);
    }
  }, [authUser]);

  const handleUpdateProfile = (name: string, email: string) => {
    setUser((prev) => ({ ...prev, name, email }));
  };

  const handleSelectLevel = (level: CEFRLevel) => {
    setUser((prev) => ({ ...prev, level }));
  };

  const handleSelectGoal = (goal: LearningGoal) => {
    setUser((prev) => ({ ...prev, learningGoal: goal }));
  };

  return (
    <AppShell>
      <div className="space-y-8 max-w-4xl mx-auto">
        <PageHeader
          title={t.learnerProfile}
          description="Manage your personal details, English proficiency baseline, learning target, and Emma coach voice preferences."
        />

        {/* User Account Overview */}
        <ProfileOverview user={user} onSave={handleUpdateProfile} />

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
