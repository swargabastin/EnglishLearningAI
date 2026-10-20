import { LoginForm } from "@/components/auth/LoginForm";
import { Sparkles, MessageSquare, Mic, TrendingUp, CheckCircle2, Bot } from "lucide-react";

export default function LoginPage() {
  return (
    <main className="min-h-screen bg-slate-950 flex items-center justify-center p-4 sm:p-6 lg:p-12">
      <div className="w-full max-w-5xl bg-slate-900 rounded-3xl shadow-2xl border border-slate-800 overflow-hidden grid grid-cols-1 lg:grid-cols-2">
        {/* Left Side: Brand Marketing & Value Proposition (Desktop) */}
        <div className="hidden lg:flex flex-col justify-between p-10 bg-gradient-to-br from-slate-950 via-slate-900 to-purple-950/60 text-white relative overflow-hidden border-r border-slate-800">
          {/* Subtle Background Glows */}
          <div className="absolute top-0 right-0 -mt-12 -mr-12 w-64 h-64 bg-purple-600/20 rounded-full blur-3xl pointer-events-none" />
          <div className="absolute bottom-0 left-0 -mb-12 -ml-12 w-64 h-64 bg-indigo-600/20 rounded-full blur-3xl pointer-events-none" />

          {/* Top Brand Logo */}
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

          {/* Center Value Proposition */}
          <div className="relative z-10 space-y-6 my-auto py-8">
            <div className="space-y-2">
              <span className="px-3 py-1 bg-purple-950/60 border border-purple-800/50 text-purple-300 rounded-full text-xs font-bold uppercase tracking-wider">
                Real-Time Voice Coaching
              </span>
              <h2 className="text-3xl font-black tracking-tight text-white leading-tight">
                Master Spoken English with AI Coach Emma
              </h2>
              <p className="text-sm text-slate-300 leading-relaxed">
                Practice real spoken dialogues in a judgment-free environment. Receive instant CEFR-aligned feedback on your grammar, vocabulary, and fluency.
              </p>
            </div>

            {/* Feature Highlights */}
            <div className="space-y-3.5 pt-2">
              <div className="flex items-start space-x-3 text-sm text-slate-200">
                <div className="p-1.5 rounded-xl bg-purple-950/60 border border-purple-800/40 text-purple-400 mt-0.5">
                  <Mic className="w-4 h-4" />
                </div>
                <div>
                  <strong className="text-white block font-semibold">Live Whisper Speech Recognition</strong>
                  <span className="text-xs text-slate-400">Accurate speech-to-text with conversational turn-taking</span>
                </div>
              </div>

              <div className="flex items-start space-x-3 text-sm text-slate-200">
                <div className="p-1.5 rounded-xl bg-indigo-950/60 border border-indigo-800/40 text-indigo-400 mt-0.5">
                  <TrendingUp className="w-4 h-4" />
                </div>
                <div>
                  <strong className="text-white block font-semibold">CEFR-Standard Assessment</strong>
                  <span className="text-xs text-slate-400">Personalized scores across Grammar, Vocab, and Fluency</span>
                </div>
              </div>

              <div className="flex items-start space-x-3 text-sm text-slate-200">
                <div className="p-1.5 rounded-xl bg-emerald-950/60 border border-emerald-800/40 text-emerald-400 mt-0.5">
                  <CheckCircle2 className="w-4 h-4" />
                </div>
                <div>
                  <strong className="text-white block font-semibold">Multi-Language Assistance</strong>
                  <span className="text-xs text-slate-400">Support for Malayalam and other native languages</span>
                </div>
              </div>
            </div>
          </div>

          {/* Bottom Security / Trust Notice */}
          <div className="relative z-10 pt-6 border-t border-slate-800/80 text-xs text-slate-400">
            Secure PostgreSQL Persistence · OpenAI Speech Pipeline
          </div>
        </div>

        {/* Right Side: Form */}
        <div className="p-8 sm:p-12 flex items-center justify-center bg-slate-900">
          <LoginForm />
        </div>
      </div>
    </main>
  );
}
