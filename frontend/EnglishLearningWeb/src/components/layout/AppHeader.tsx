"use client";

import React from "react";
import Link from "next/link";
import { Sparkles, Flame, LogOut, Globe, Bot } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { useLanguage } from "@/context/LanguageContext";
import { mockUser } from "@/lib/mock";
import { Avatar } from "@/components/ui/Avatar";
import { Badge } from "@/components/ui/Badge";

export function AppHeader() {
  const { user: authUser, logout } = useAuth();
  const { language, setLanguage, t } = useLanguage();
  const user = authUser || mockUser;

  const toggleLanguage = () => {
    setLanguage(language === "en" ? "ml" : "en");
  };

  return (
    <header className="sticky top-0 z-30 flex items-center justify-between px-4 sm:px-8 py-3 bg-slate-900/90 backdrop-blur-xl border-b border-slate-800/80">
      {/* Left: Mobile Brand / Desktop Welcome */}
      <div className="flex items-center space-x-3">
        <Link href="/dashboard" className="flex items-center space-x-2.5 md:hidden">
          <div className="w-8 h-8 rounded-xl bg-gradient-to-tr from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-sm shadow-purple-600/30">
            <Sparkles className="w-4 h-4" />
          </div>
          <span className="font-extrabold text-white text-base tracking-tight">
            FluentAI
          </span>
        </Link>

        <div className="hidden md:flex items-center space-x-2.5 text-sm">
          <span className="text-slate-400">Welcome,</span>
          <span className="font-bold text-white">{user.name}</span>
          <Badge cefrLevel={user.level} size="sm" />
        </div>
      </div>

      {/* Center: Emma Online Pill (Desktop) */}
      <div className="hidden lg:flex items-center space-x-2 px-3 py-1 bg-purple-950/40 border border-purple-800/40 rounded-full text-xs">
        <span className="relative flex h-2 w-2">
          <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-purple-400 opacity-75" />
          <span className="relative inline-flex rounded-full h-2 w-2 bg-purple-500" />
        </span>
        <span className="font-semibold text-purple-200">Emma AI Coach</span>
        <span className="text-purple-400 text-[11px]">· Online</span>
      </div>

      {/* Right: Language Toggle, Streak, Profile, Logout */}
      <div className="flex items-center space-x-2.5 sm:space-x-3">
        {/* Language Selector Toggle */}
        <button
          type="button"
          onClick={toggleLanguage}
          className="flex items-center space-x-1.5 px-2.5 py-1.5 bg-slate-800/80 hover:bg-slate-700/80 border border-slate-700/60 rounded-xl text-xs font-semibold text-slate-200 transition-all hover:scale-102"
          title="Switch Language / ഭാഷ മാറ്റുക"
        >
          <Globe className="w-3.5 h-3.5 text-purple-400" />
          <span>{language === "en" ? "EN" : "മലയാളം"}</span>
        </button>

        {/* Daily Streak Indicator */}
        <div className="flex items-center space-x-1.5 px-2.5 py-1.5 bg-amber-950/40 border border-amber-800/40 rounded-xl text-xs font-bold text-amber-300">
          <Flame className="w-4 h-4 text-amber-400 fill-amber-400" />
          <span className="hidden sm:inline">{user.streakDays}d Streak</span>
          <span className="sm:hidden">{user.streakDays}d</span>
        </div>

        {/* User Avatar Menu Link */}
        <Link
          href="/profile"
          className="flex items-center space-x-2 p-0.5 rounded-full hover:ring-2 hover:ring-purple-500/50 transition-all"
          title="View Profile"
        >
          <Avatar name={user.name} src={user.avatarUrl} size="sm" />
        </Link>

        {/* Logout Button */}
        <button
          type="button"
          onClick={logout}
          className="p-2 text-slate-400 hover:text-red-400 hover:bg-red-950/30 rounded-xl transition-colors"
          title="Sign Out"
        >
          <LogOut className="w-4 h-4" />
        </button>
      </div>
    </header>
  );
}
