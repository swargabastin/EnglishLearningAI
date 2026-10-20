"use client";

import React, { useState } from "react";
import { User, Mail, Calendar, Save, CheckCircle2, Globe } from "lucide-react";
import { UserProfile } from "@/types";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Avatar } from "@/components/ui/Avatar";
import { Badge } from "@/components/ui/Badge";
import { useLanguage } from "@/context/LanguageContext";

export function ProfileOverview({
  user,
  onSave,
}: {
  user: UserProfile;
  onSave: (name: string, email: string) => void;
}) {
  const { language, setLanguage, t } = useLanguage();
  const [name, setName] = useState(user.name);
  const [email, setEmail] = useState(user.email);
  const [dailyTarget, setDailyTarget] = useState(user.dailyPracticeTargetMinutes);
  const [isSaved, setIsSaved] = useState(false);

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(name, email);
    setIsSaved(true);
    setTimeout(() => setIsSaved(false), 3000);
  };

  return (
    <Card>
      <CardHeader className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4">
        <div className="flex items-center space-x-4">
          <Avatar src={user.avatarUrl} name={name} size="lg" />
          <div className="space-y-1">
            <div className="flex items-center space-x-2">
              <CardTitle className="text-lg font-bold text-white">{name}</CardTitle>
              <Badge cefrLevel={user.level} size="sm" />
            </div>
            <p className="text-xs text-slate-400">
              Active Member since June 2026 • {user.streakDays} Day Practice Streak
            </p>
          </div>
        </div>

        {isSaved && (
          <div className="px-3 py-1.5 rounded-xl bg-emerald-950/50 border border-emerald-800/40 text-emerald-300 text-xs font-semibold flex items-center space-x-1.5 animate-in fade-in">
            <CheckCircle2 className="w-4 h-4 text-emerald-400" />
            <span>Profile Saved Successfully</span>
          </div>
        )}
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSave} className="space-y-5">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input
              label="Full Name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              leftIcon={<User className="w-4 h-4" />}
              required
            />
            <Input
              label="Email Address"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              leftIcon={<Mail className="w-4 h-4" />}
              required
            />
          </div>

          {/* Interface Language Preference */}
          <div className="space-y-2">
            <label className="block text-xs font-bold text-slate-300 tracking-wide">
              Interface Language / ഭാഷാ മുൻഗണന
            </label>
            <div className="flex items-center space-x-3">
              <button
                type="button"
                onClick={() => setLanguage("en")}
                className={`px-4 py-2 rounded-2xl text-xs font-bold border transition-all ${
                  language === "en"
                    ? "bg-purple-600 text-white border-purple-500 shadow-md shadow-purple-950/40"
                    : "bg-slate-850 border-slate-700 text-slate-300 hover:bg-slate-800"
                }`}
              >
                English (EN)
              </button>
              <button
                type="button"
                onClick={() => setLanguage("ml")}
                className={`px-4 py-2 rounded-2xl text-xs font-bold border transition-all ${
                  language === "ml"
                    ? "bg-purple-600 text-white border-purple-500 shadow-md shadow-purple-950/40"
                    : "bg-slate-850 border-slate-700 text-slate-300 hover:bg-slate-800"
                }`}
              >
                മലയാളം (Malayalam)
              </button>
            </div>
          </div>

          {/* Daily Target */}
          <div className="space-y-2">
            <label className="block text-xs font-bold text-slate-300 tracking-wide">
              Daily Practice Target: <span className="text-purple-400 font-bold">{dailyTarget} minutes/day</span>
            </label>
            <div className="flex items-center space-x-2">
              {[10, 15, 20, 30].map((mins) => (
                <button
                  key={mins}
                  type="button"
                  onClick={() => setDailyTarget(mins)}
                  className={`px-4 py-2 rounded-2xl text-xs font-bold border transition-all ${
                    dailyTarget === mins
                      ? "bg-purple-600 text-white border-purple-500 shadow-md shadow-purple-950/40"
                      : "bg-slate-850 border-slate-700 text-slate-300 hover:bg-slate-800"
                  }`}
                >
                  {mins} mins
                </button>
              ))}
            </div>
          </div>

          <div className="pt-2 flex justify-end">
            <Button
              type="submit"
              variant="primary"
              size="md"
              className="bg-purple-600 hover:bg-purple-500"
              leftIcon={<Save className="w-4 h-4" />}
            >
              Save Profile Changes
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
