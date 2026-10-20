"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Sparkles, User, Mail, Lock, Eye, EyeOff, AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { useAuth } from "@/context/AuthContext";
import { CEFRLevel } from "@/types";
import { ApiException } from "@/services/apiClient";

export function RegisterForm() {
  const router = useRouter();
  const { register } = useAuth();
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [level, setLevel] = useState<CEFRLevel>("B1");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim() || !email.trim() || !password) {
      setError("Please fill out all required registration fields.");
      return;
    }

    if (password.length < 8) {
      setError("Password must be at least 8 characters long.");
      return;
    }

    setIsLoading(true);

    try {
      await register({
        name: name.trim(),
        email: email.trim(),
        password,
        confirmPassword: password,
        englishLevel: level,
        learningGoal: "Speaking Fluency",
      });
      router.push("/dashboard");
    } catch (err) {
      if (err instanceof ApiException) {
        setError(err.details || err.message || "Registration failed. Please check your inputs.");
      } else {
        setError((err as Error).message || "An unexpected registration error occurred.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md space-y-6">
      {/* Header */}
      <div className="text-left space-y-1.5">
        <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-md shadow-purple-600/30 lg:hidden mb-4">
          <Sparkles className="w-6 h-6" />
        </div>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
          Create Account
        </h1>
        <p className="text-xs sm:text-sm text-slate-400">
          Start your personalized spoken English journey with Emma
        </p>
      </div>

      {/* Error Alert Box */}
      {error && (
        <div className="p-4 rounded-2xl bg-red-950/50 border border-red-800/60 text-red-300 text-xs flex items-start space-x-3 animate-in fade-in">
          <AlertCircle className="w-4 h-4 text-red-400 mt-0.5 flex-shrink-0" />
          <div className="space-y-0.5">
            <span className="font-bold block">Registration Notice</span>
            <span>{error}</span>
          </div>
        </div>
      )}

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Full Name"
          type="text"
          placeholder="Swarga Bastin"
          value={name}
          onChange={(e) => setName(e.target.value)}
          leftIcon={<User className="w-4 h-4" />}
          required
        />

        <Input
          label="Email Address"
          type="email"
          autoComplete="email"
          placeholder="learner@example.com"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          leftIcon={<Mail className="w-4 h-4" />}
          required
        />

        <Input
          label="Password (min. 8 chars, 1 uppercase, 1 digit)"
          type={showPassword ? "text" : "password"}
          autoComplete="new-password"
          placeholder="••••••••"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          leftIcon={<Lock className="w-4 h-4" />}
          rightIcon={
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="text-slate-400 hover:text-slate-200 focus:outline-none"
            >
              {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
            </button>
          }
          required
        />

        {/* Initial CEFR Level Selector */}
        <div className="space-y-1.5 text-left">
          <label className="block text-xs font-bold text-slate-300 tracking-wide">
            Your Current Spoken English Level (CEFR)
          </label>
          <div className="grid grid-cols-3 gap-2">
            {(["A1", "A2", "B1", "B2", "C1", "C2"] as CEFRLevel[]).map((lvl) => (
              <button
                key={lvl}
                type="button"
                onClick={() => setLevel(lvl)}
                className={`py-2 px-3 rounded-2xl text-xs font-bold border transition-all ${
                  level === lvl
                    ? "bg-purple-600 text-white border-purple-500 shadow-md shadow-purple-950/40"
                    : "bg-slate-800 border-slate-700 text-slate-300 hover:bg-slate-750"
                }`}
              >
                {lvl} {lvl === "B1" ? "(Recommended)" : ""}
              </button>
            ))}
          </div>
        </div>

        {/* Submit Button */}
        <Button
          type="submit"
          variant="primary"
          size="lg"
          className="w-full bg-purple-600 hover:bg-purple-500 shadow-lg shadow-purple-950/50"
          isLoading={isLoading}
        >
          Create Account & Start Speaking
        </Button>
      </form>

      {/* Sign in link */}
      <div className="text-center pt-2 border-t border-slate-800 text-xs text-slate-400">
        Already have an account?{" "}
        <Link
          href="/login"
          className="font-bold text-purple-400 hover:text-purple-300 hover:underline"
        >
          Sign in
        </Link>
      </div>
    </div>
  );
}
