"use client";

import React, { createContext, useContext, useEffect, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import { authService } from "@/services/authService";
import { progressService } from "@/services/progressService";
import {
  AuthUserDto,
  CEFRLevel,
  LearningGoal,
  LoginPayload,
  RegisterPayload,
  UserProfile,
} from "@/types";

interface AuthContextType {
  user: UserProfile | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => void;
  updateUserPreferences: (prefs: Partial<UserProfile["preferences"]>) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function mapDtoToUserProfile(dto: AuthUserDto): UserProfile {
  return {
    id: dto.id,
    name: dto.name,
    email: dto.email,
    level: (dto.englishLevel as CEFRLevel) || "B1",
    learningGoal: (dto.learningGoal as LearningGoal) || "Speaking Fluency",
    dailyPracticeTargetMinutes: 15,
    todayPracticeMinutes: 0,
    streakDays: 0,
    avatarUrl: undefined,
    createdAt: dto.createdAt,
    preferences: {
      tutorVoice: "alloy",
      playbackSpeed: 1.0,
      autoPlayAudio: true,
      showLiveTranscript: true,
    },
  };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const router = useRouter();

  const loadUser = useCallback(async () => {
    if (typeof window === "undefined") {
      setIsLoading(false);
      return;
    }

    const savedToken = localStorage.getItem("el_access_token");
    if (!savedToken) {
      setUser(null);
      setToken(null);
      setIsLoading(false);
      return;
    }

    setToken(savedToken);
    try {
      const userDto = await authService.getMe();
      const profile = mapDtoToUserProfile(userDto);
      try {
        const dashboard = await progressService.getDashboard();
        if (dashboard?.stats) {
          profile.todayPracticeMinutes = Math.min(
            dashboard.stats.totalPracticeMinutes || 0,
            profile.dailyPracticeTargetMinutes
          );
        }
      } catch {
        // Fallback to initial 0 metrics if dashboard stats not yet populated
      }
      setUser(profile);
    } catch {
      localStorage.removeItem("el_access_token");
      setUser(null);
      setToken(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadUser();
  }, [loadUser]);

  const login = async (payload: LoginPayload) => {
    setIsLoading(true);
    try {
      const data = await authService.login(payload);
      localStorage.setItem("el_access_token", data.accessToken);
      setToken(data.accessToken);
      setUser(mapDtoToUserProfile(data.user));
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (payload: RegisterPayload) => {
    setIsLoading(true);
    try {
      const data = await authService.register(payload);
      localStorage.setItem("el_access_token", data.accessToken);
      setToken(data.accessToken);
      setUser(mapDtoToUserProfile(data.user));
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    if (typeof window !== "undefined") {
      localStorage.removeItem("el_access_token");
    }
    setToken(null);
    setUser(null);
    router.push("/login");
  };

  const updateUserPreferences = (prefs: Partial<UserProfile["preferences"]>) => {
    setUser((prev) => {
      if (!prev) return null;
      return {
        ...prev,
        preferences: {
          ...prev.preferences,
          ...prefs,
        },
      };
    });
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!user && !!token,
        isLoading,
        login,
        register,
        logout,
        updateUserPreferences,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
