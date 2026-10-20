"use client";

import React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  LayoutDashboard,
  Compass,
  Mic,
  History,
  User,
  Sparkles,
  Flame,
  Bot,
  ArrowRight,
  BookOpen,
  CalendarCheck2,
} from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { useLanguage } from "@/context/LanguageContext";
import { mockUser } from "@/lib/mock";
import { Badge } from "@/components/ui/Badge";

export function Sidebar() {
  const pathname = usePathname();
  const { user: authUser } = useAuth();
  const { t } = useLanguage();
  const user = authUser || mockUser;

  const navItems = [
    { href: "/dashboard", label: t.navHome, icon: LayoutDashboard },
    { href: "/plan", label: t.navPlan, icon: CalendarCheck2 },
    { href: "/topics", label: t.navTopics, icon: Compass },
    { href: "/vocabulary", label: t.navVocabulary, icon: BookOpen },
    { href: "/practice", label: t.navPractice, icon: Mic, highlight: true },
    { href: "/history", label: t.navHistory, icon: History },
    { href: "/profile", label: t.navProfile, icon: User },
  ];

  return (
    <aside className="hidden md:flex flex-col w-64 border-r border-slate-800/80 bg-slate-900 min-h-screen p-4 justify-between select-none">
      <div className="space-y-6">
        {/* Brand Logo */}
        <Link href="/dashboard" className="flex items-center space-x-3 px-3 py-2">
          <div className="w-10 h-10 rounded-2xl bg-gradient-to-tr from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-lg shadow-purple-600/30 ring-1 ring-purple-400/30">
            <Sparkles className="w-5 h-5" />
          </div>
          <div>
            <span className="font-extrabold text-white tracking-tight text-lg block leading-tight">
              FluentAI
            </span>
            <span className="text-[11px] text-purple-300 font-medium tracking-wide">
              Emma · AI Coach
            </span>
          </div>
        </Link>

        {/* User Summary Pill */}
        <div className="mx-1 p-3 bg-slate-850 border border-slate-800 rounded-2xl flex items-center justify-between shadow-xs">
          <div className="space-y-0.5">
            <p className="text-xs font-semibold text-slate-200">{user.name}</p>
            <Badge cefrLevel={user.level} size="sm" />
          </div>
          <div className="flex items-center space-x-1 text-amber-400 bg-amber-950/40 border border-amber-800/40 px-2 py-1 rounded-xl text-xs font-bold">
            <Flame className="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
            <span>{user.streakDays}d</span>
          </div>
        </div>

        {/* Navigation Links */}
        <nav className="space-y-1.5">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive =
              pathname === item.href ||
              (item.href !== "/dashboard" && pathname.startsWith(item.href));

            if (item.highlight) {
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center justify-between px-3.5 py-3 rounded-2xl text-sm font-bold transition-all ${
                    isActive
                      ? "bg-gradient-to-r from-purple-600 to-indigo-600 text-white shadow-md shadow-purple-600/30"
                      : "bg-purple-950/30 hover:bg-purple-900/40 text-purple-200 border border-purple-800/30"
                  }`}
                >
                  <div className="flex items-center space-x-3">
                    <Icon className="w-5 h-5 text-purple-300" />
                    <span>{item.label}</span>
                  </div>
                  <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
                </Link>
              );
            }

            return (
              <Link
                key={item.href}
                href={item.href}
                className={`flex items-center space-x-3 px-3.5 py-2.5 rounded-xl text-sm font-medium transition-all ${
                  isActive
                    ? "bg-slate-800 text-white font-semibold shadow-xs border border-slate-700"
                    : "text-slate-400 hover:bg-slate-850 hover:text-slate-200"
                }`}
              >
                <Icon
                  className={`w-5 h-5 ${
                    isActive ? "text-purple-400" : "text-slate-500"
                  }`}
                />
                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>
      </div>

      {/* Footer Daily Goal Card */}
      <div className="space-y-3 pt-4 border-t border-slate-800">
        <div className="p-3.5 bg-gradient-to-br from-slate-850 to-slate-900 border border-slate-800 rounded-2xl space-y-2 text-center">
          <p className="text-xs font-semibold text-purple-300">{t.dailyGoal}</p>
          <p className="text-xs text-slate-300 font-medium">
            {user.todayPracticeMinutes} / {user.dailyPracticeTargetMinutes} min
          </p>
          <div className="w-full bg-slate-800 rounded-full h-1.5 overflow-hidden">
            <div
              className="bg-gradient-to-r from-purple-500 to-indigo-500 h-full rounded-full transition-all duration-500"
              style={{
                width: `${Math.min(
                  100,
                  Math.round((user.todayPracticeMinutes / user.dailyPracticeTargetMinutes) * 100)
                )}%`,
              }}
            />
          </div>
          <Link
            href="/practice"
            className="inline-flex items-center space-x-1 text-[11px] font-bold text-purple-400 hover:text-purple-300 pt-1"
          >
            <span>{t.talkToEmmaCta}</span>
            <ArrowRight className="w-3 h-3" />
          </Link>
        </div>
      </div>
    </aside>
  );
}
