"use client";

import React, { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Sparkles, Mail, Lock, Eye, EyeOff, AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { useAuth } from "@/context/AuthContext";
import { ApiException } from "@/services/apiClient";

export function LoginForm() {
  const router = useRouter();
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!email.trim() || !password) {
      setError("Please enter both email and password.");
      return;
    }

    setIsLoading(true);

    try {
      await login({ email: email.trim(), password });
      router.push("/dashboard");
    } catch (err) {
      if (err instanceof ApiException) {
        setError(err.details || err.message || "Invalid email or password.");
      } else {
        setError((err as Error).message || "An unexpected error occurred.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md space-y-6">
      {/* Brand Header */}
      <div className="text-left space-y-2">
        <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-purple-600 to-indigo-600 flex items-center justify-center text-white shadow-md shadow-purple-600/30 lg:hidden mb-4">
          <Sparkles className="w-6 h-6" />
        </div>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
          Sign In
        </h1>
        <p className="text-sm text-slate-400">
          Enter your credentials to access your speaking sessions and progress
        </p>
      </div>

      {/* Error Alert Box */}
      {error && (
        <div className="p-4 rounded-2xl bg-red-950/50 border border-red-800/60 text-red-300 text-xs flex items-start space-x-3 animate-in fade-in">
          <AlertCircle className="w-4 h-4 text-red-400 mt-0.5 flex-shrink-0" />
          <div className="space-y-0.5">
            <span className="font-bold block">Authentication Notice</span>
            <span>{error}</span>
          </div>
        </div>
      )}

      {/* Login Form */}
      <form onSubmit={handleSubmit} className="space-y-4">
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

        <div className="space-y-1.5">
          <Input
            label="Password"
            type={showPassword ? "text" : "password"}
            autoComplete="current-password"
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
                {showPassword ? (
                  <EyeOff className="w-4 h-4" />
                ) : (
                  <Eye className="w-4 h-4" />
                )}
              </button>
            }
            required
          />
        </div>

        {/* Remember me / Forgot password */}
        <div className="flex items-center justify-between text-xs pt-1">
          <label className="flex items-center space-x-2 text-slate-300 cursor-pointer">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
              className="rounded border-slate-700 bg-slate-800 text-purple-600 focus:ring-purple-500 focus:ring-offset-slate-900"
            />
            <span>Remember me</span>
          </label>
        </div>

        {/* Submit Button */}
        <Button
          type="submit"
          variant="primary"
          size="lg"
          className="w-full bg-purple-600 hover:bg-purple-500 shadow-lg shadow-purple-950/50"
          isLoading={isLoading}
        >
          Sign In
        </Button>

        {/* Quick Demo Access Button */}
        <button
          type="button"
          disabled={isLoading}
          onClick={async () => {
            setEmail("learner@example.com");
            setPassword("Password123!");
            setIsLoading(true);
            try {
              await login({ email: "learner@example.com", password: "Password123!" });
              router.push("/dashboard");
            } catch {
              setError("Demo sign-in failed. Please try manual login.");
            } finally {
              setIsLoading(false);
            }
          }}
          className="w-full py-2.5 px-4 rounded-xl border border-purple-500/30 bg-purple-950/40 hover:bg-purple-900/50 text-purple-200 text-xs font-bold transition-all flex items-center justify-center space-x-2 shadow-xs"
        >
          <Sparkles className="w-3.5 h-3.5 text-purple-400" />
          <span>Quick Demo Sign-In (Emma Learner)</span>
        </button>
      </form>

      {/* Register Link */}
      <div className="text-center pt-2 border-t border-slate-800 text-xs text-slate-400">
        Don&apos;t have an account?{" "}
        <Link
          href="/register"
          className="font-bold text-purple-400 hover:text-purple-300 hover:underline"
        >
          Create an account
        </Link>
      </div>
    </div>
  );
}
