"use client";

import React, { useState, useRef } from "react";
import { User, Mail, Calendar, Save, CheckCircle2, Globe, Camera, Loader2, AlertCircle } from "lucide-react";
import { UserProfile } from "@/types";
import { authService } from "@/services/authService";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Avatar } from "@/components/ui/Avatar";
import { Badge } from "@/components/ui/Badge";
import { useLanguage } from "@/context/LanguageContext";

export function ProfileOverview({
  user,
  onSave,
  onAvatarUpdated,
}: {
  user: UserProfile;
  onSave: (name: string, email: string) => void;
  onAvatarUpdated?: (avatarUrl: string) => void;
}) {
  const { language, setLanguage, t } = useLanguage();
  const [name, setName] = useState(user.name);
  const [email, setEmail] = useState(user.email);
  const [dailyTarget, setDailyTarget] = useState(user.dailyPracticeTargetMinutes);
  const [avatarUrl, setAvatarUrl] = useState(user.avatarUrl);
  const [isUploadingAvatar, setIsUploadingAvatar] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isSaved, setIsSaved] = useState(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setIsUploadingAvatar(true);
    setUploadError(null);
    try {
      const newUrl = await authService.uploadAvatar(file);
      setAvatarUrl(newUrl);
      onAvatarUpdated?.(newUrl);
    } catch (err: unknown) {
      setUploadError((err as Error).message || "Failed to upload avatar photo.");
    } finally {
      setIsUploadingAvatar(false);
    }
  };

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
          <div className="relative group shrink-0">
            <Avatar src={avatarUrl || user.avatarUrl} name={name} size="lg" />
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={isUploadingAvatar}
              className="absolute inset-0 bg-slate-950/70 rounded-full opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center justify-center text-white cursor-pointer"
              title="Click to upload profile photo"
            >
              {isUploadingAvatar ? (
                <Loader2 className="w-5 h-5 animate-spin text-purple-400" />
              ) : (
                <>
                  <Camera className="w-5 h-5 text-purple-300" />
                  <span className="text-[9px] font-bold mt-0.5">Edit</span>
                </>
              )}
            </button>
            <input
              type="file"
              ref={fileInputRef}
              accept="image/png,image/jpeg,image/webp,image/gif"
              className="hidden"
              onChange={handleFileSelect}
            />
          </div>

          <div className="space-y-1">
            <div className="flex items-center space-x-2">
              <CardTitle className="text-lg font-bold text-white">{name}</CardTitle>
              <Badge cefrLevel={user.level} size="sm" />
            </div>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                disabled={isUploadingAvatar}
                className="text-[11px] font-semibold text-purple-400 hover:text-purple-300 underline flex items-center gap-1"
              >
                <Camera className="w-3 h-3" />
                <span>{isUploadingAvatar ? "Uploading..." : "Upload photo"}</span>
              </button>
              <span className="text-slate-600">•</span>
              <p className="text-xs text-slate-400">
                {user.streakDays} Day Practice Streak
              </p>
            </div>
            {uploadError && (
              <p className="text-[11px] text-rose-400 flex items-center gap-1 font-medium">
                <AlertCircle className="w-3 h-3" />
                <span>{uploadError}</span>
              </p>
            )}
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
