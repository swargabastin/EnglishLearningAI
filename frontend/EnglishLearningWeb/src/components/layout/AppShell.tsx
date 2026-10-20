"use client";

import React, { useEffect } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import {
  LayoutDashboard,
  Compass,
  Mic,
  History,
  User,
  Sparkles,
  Bot,
  BookOpen,
  CalendarCheck2,
} from "lucide-react";
import { Sidebar } from "./Sidebar";
import { AppHeader } from "./AppHeader";
import { useAuth } from "@/context/AuthContext";
import { useLanguage } from "@/context/LanguageContext";

export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth();
  const { t } = useLanguage();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login");
    }
  }, [isLoading, isAuthenticated, router]);

  const mobileNavItems = [
    { href: "/dashboard", label: t.navHome, icon: LayoutDashboard },
    { href: "/plan", label: t.navPlan, icon: CalendarCheck2 },
    { href: "/practice", label: "Emma", icon: Mic, highlight: true },
    { href: "/vocabulary", label: t.navVocabulary, icon: BookOpen },
    { href: "/profile", label: t.navProfile, icon: User },
  ];

  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center space-y-4">
        <div className="w-14 h-14 rounded-3xl bg-gradient-to-tr from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-xl shadow-purple-600/30 animate-pulse ring-4 ring-purple-500/20">
          <Sparkles className="w-7 h-7" />
        </div>
        <p className="text-sm font-semibold text-slate-400">Loading your learning space...</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="flex min-h-screen bg-slate-950 text-slate-100">
      {/* Desktop Sidebar */}
      <Sidebar />

      {/* Main Content Column */}
      <div className="flex-1 flex flex-col min-w-0 pb-24 md:pb-8">
        <AppHeader />
        <main className="flex-1 p-4 sm:p-6 lg:p-8 max-w-6xl w-full mx-auto animate-in fade-in duration-200">
          {children}
        </main>
      </div>

      {/* Mobile Bottom Navigation Bar */}
      <nav className="md:hidden fixed bottom-0 left-0 right-0 z-40 bg-slate-900/95 backdrop-blur-2xl border-t border-slate-800 flex items-center justify-around py-2 px-3 safe-bottom shadow-2xl">
        {mobileNavItems.map((item) => {
          const Icon = item.icon;
          const isActive =
            pathname === item.href ||
            (item.href !== "/dashboard" && pathname.startsWith(item.href));

          if (item.highlight) {
            return (
              <Link
                key={item.href}
                href={item.href}
                className="flex flex-col items-center -mt-6 group"
              >
                <div className="w-14 h-14 rounded-full bg-gradient-to-tr from-purple-600 via-indigo-600 to-blue-500 flex items-center justify-center text-white shadow-lg shadow-purple-600/40 ring-4 ring-slate-950 active:scale-95 transition-all group-hover:scale-105">
                  <Icon className="w-7 h-7 animate-bounce-subtle" />
                </div>
                <span className="text-[10px] font-extrabold text-purple-400 mt-1">
                  {item.label}
                </span>
              </Link>
            );
          }

          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex flex-col items-center py-1 px-3 rounded-xl text-[11px] font-semibold transition-all ${
                isActive
                  ? "text-purple-400 font-bold scale-105"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              <Icon className="w-5 h-5 mb-0.5" />
              <span>{item.label}</span>
            </Link>
          );
        })}
      </nav>
    </div>
  );
}
