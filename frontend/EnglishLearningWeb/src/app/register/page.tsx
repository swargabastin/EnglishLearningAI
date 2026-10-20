import { RegisterForm } from "@/components/auth/RegisterForm";
import { Sparkles, CheckCircle2, Mic, Award, Compass, Bot } from "lucide-react";

export default function RegisterPage() {
  return (
    <main className="min-h-screen bg-slate-950 flex items-center justify-center p-4 sm:p-6 lg:p-12">
      <div className="w-full max-w-5xl bg-slate-900 rounded-3xl shadow-2xl border border-slate-800 overflow-hidden grid grid-cols-1 lg:grid-cols-12">
        {/* Left Marketing Section (Desktop) */}
        <div className="hidden lg:flex lg:col-span-5 flex-col justify-between p-10 bg-gradient-to-br from-slate-950 via-slate-900 to-purple-950/60 text-white relative overflow-hidden border-r border-slate-800">
          <div className="absolute top-0 right-0 -mt-12 -mr-12 w-64 h-64 bg-purple-600/20 rounded-full blur-3xl pointer-events-none" />
          <div className="absolute bottom-0 left-0 -mb-12 -ml-12 w-64 h-64 bg-indigo-600/20 rounded-full blur-3xl pointer-events-none" />

          {/* Brand Header */}
          <div className="relative z-10 space-y-3">
            <div className="flex items-center space-x-3">
              <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-lg shadow-purple-600/30 ring-2 ring-purple-400/30">
                <Sparkles className="w-6 h-6" />
              </div>
              <div>
                <span className="font-extrabold text-white text-xl tracking-tight block leading-tight">
                  FluentAI
                </span>
                <span className="text-xs text-purple-300 font-medium tracking-wide">
                  Emma · AI English Coach
                </span>
              </div>
            </div>
          </div>

          {/* Value Highlights */}
          <div className="relative z-10 space-y-6 my-auto py-8">
            <div className="space-y-2">
              <span className="px-3 py-1 bg-emerald-950/60 border border-emerald-800/50 text-emerald-300 rounded-full text-xs font-bold uppercase tracking-wider">
                Start Speaking Today
              </span>
              <h2 className="text-2xl font-black tracking-tight text-white leading-tight">
                Join English Learners Practicing Every Day
              </h2>
              <p className="text-xs text-slate-300 leading-relaxed">
                Build spontaneous conversation skills with instant feedback tailored to your CEFR level and career goals.
              </p>
            </div>

            <div className="space-y-4 pt-2">
              <div className="flex items-start space-x-3 text-xs text-slate-200">
                <div className="p-1.5 rounded-xl bg-purple-950/60 border border-purple-800/40 text-purple-400 mt-0.5">
                  <Mic className="w-4 h-4" />
                </div>
                <div>
                  <strong className="text-white block font-semibold">1-on-1 Practice with Emma</strong>
                  <span className="text-slate-400">Natural voice turn-taking and supportive guidance</span>
                </div>
              </div>

              <div className="flex items-start space-x-3 text-xs text-slate-200">
                <div className="p-1.5 rounded-xl bg-indigo-950/60 border border-indigo-800/40 text-indigo-400 mt-0.5">
                  <Award className="w-4 h-4" />
                </div>
                <div>
                  <strong className="text-white block font-semibold">Instant Pedagogical Scores</strong>
                  <span className="text-slate-400">Grammar, vocabulary, and pronunciation metrics</span>
                </div>
              </div>

              <div className="flex items-start space-x-3 text-xs text-slate-200">
                <div className="p-1.5 rounded-xl bg-emerald-950/60 border border-emerald-800/40 text-emerald-400 mt-0.5">
                  <Compass className="w-4 h-4" />
                </div>
                <div>
                  <strong className="text-white block font-semibold">Real-Life Scenarios</strong>
                  <span className="text-slate-400">Job interviews, workplace meetings, and daily life</span>
                </div>
              </div>
            </div>
          </div>

          <div className="relative z-10 pt-4 border-t border-slate-800/80 text-[11px] text-slate-400">
            PostgreSQL Database · Zero mock data · Pure AI Voice Coaching
          </div>
        </div>

        {/* Right Form Section */}
        <div className="lg:col-span-7 p-6 sm:p-10 flex items-center justify-center bg-slate-900">
          <RegisterForm />
        </div>
      </div>
    </main>
  );
}
