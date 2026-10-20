import React from "react";

export interface ProgressBarProps {
  value: number;
  max?: number;
  label?: string;
  subLabel?: string;
  variant?: "primary" | "emerald" | "amber" | "blue";
  size?: "sm" | "md" | "lg";
  className?: string;
}

export function ProgressBar({
  value,
  max = 100,
  label,
  subLabel,
  variant = "primary",
  size = "md",
  className = "",
}: ProgressBarProps) {
  const percentage = Math.min(100, Math.max(0, Math.round((value / max) * 100)));

  const heightStyles = {
    sm: "h-1.5",
    md: "h-2.5",
    lg: "h-3.5",
  };

  const fillColors = {
    primary: "bg-blue-600",
    blue: "bg-blue-600",
    emerald: "bg-emerald-500",
    amber: "bg-amber-500",
  };

  return (
    <div className={`w-full space-y-1.5 ${className}`}>
      {(label || subLabel) && (
        <div className="flex justify-between items-center text-xs font-semibold text-slate-700">
          {label && <span>{label}</span>}
          {subLabel && <span className="text-slate-500 font-normal">{subLabel}</span>}
        </div>
      )}
      <div className={`w-full bg-slate-100 rounded-full overflow-hidden ${heightStyles[size]}`}>
        <div
          className={`h-full rounded-full transition-all duration-500 ease-out ${fillColors[variant]}`}
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  );
}
