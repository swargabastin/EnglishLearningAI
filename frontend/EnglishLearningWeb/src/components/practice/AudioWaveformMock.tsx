"use client";

import React from "react";

export interface AudioWaveformMockProps {
  isActive: boolean;
  variant?: "user" | "ai";
}

export function AudioWaveformMock({
  isActive,
  variant = "user",
}: AudioWaveformMockProps) {
  const bars = [35, 70, 50, 95, 65, 100, 75, 90, 45, 85, 60, 80, 55, 90, 65, 35];

  const colorClass =
    variant === "ai"
      ? "bg-indigo-600"
      : "bg-blue-600";

  return (
    <div className="flex items-center justify-center space-x-1.5 h-9 px-4 py-1.5" aria-hidden="true">
      {bars.map((heightPercent, idx) => (
        <div
          key={idx}
          className={`w-1 rounded-full transition-all duration-150 ${colorClass} ${
            isActive ? "opacity-100" : "opacity-25"
          }`}
          style={{
            height: isActive
              ? `${Math.max(20, heightPercent * (0.45 + (idx % 3) * 0.25))}%`
              : "20%",
            animation: isActive
              ? `pulse 0.75s ease-in-out infinite ${(idx * 0.06).toFixed(2)}s`
              : "none",
          }}
        />
      ))}
    </div>
  );
}
