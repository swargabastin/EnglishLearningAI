import { UserProfile } from "@/types";

export const mockUser: UserProfile = {
  id: "user-101",
  name: "Alex Morgan",
  email: "alex.morgan@example.com",
  level: "B1",
  learningGoal: "Workplace English",
  dailyPracticeTargetMinutes: 15,
  todayPracticeMinutes: 12,
  streakDays: 4,
  avatarUrl: "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80",
  createdAt: "2026-06-15T08:00:00Z",
  preferences: {
    tutorVoice: "alloy",
    playbackSpeed: 1.0,
    autoPlayAudio: true,
    showLiveTranscript: true,
  },
};
