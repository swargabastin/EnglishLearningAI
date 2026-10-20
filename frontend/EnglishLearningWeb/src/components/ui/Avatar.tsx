import React from "react";
import { User, Sparkles } from "lucide-react";

export interface AvatarProps {
  src?: string;
  name?: string;
  isAi?: boolean;
  size?: "sm" | "md" | "lg" | "xl";
  className?: string;
  isSpeaking?: boolean;
}

export function Avatar({
  src,
  name,
  isAi = false,
  size = "md",
  className = "",
  isSpeaking = false,
}: AvatarProps) {
  const sizeMap = {
    sm: "w-8 h-8 text-xs",
    md: "w-10 h-10 text-sm",
    lg: "w-12 h-12 text-base",
    xl: "w-16 h-16 text-lg",
  };

  const ringStyles = isSpeaking
    ? "ring-4 ring-blue-400 ring-offset-2 animate-pulse"
    : "";

  return (
    <div
      className={`relative inline-flex items-center justify-center rounded-full flex-shrink-0 select-none overflow-hidden transition-all ${
        sizeMap[size]
      } ${ringStyles} ${
        isAi
          ? "bg-gradient-to-tr from-blue-600 to-indigo-600 text-white shadow-sm"
          : "bg-slate-200 text-slate-700 font-semibold"
      } ${className}`}
    >
      {isAi ? (
        <Sparkles className="w-1/2 h-1/2 text-white" />
      ) : src ? (
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={src}
          alt={name || "User Avatar"}
          className="w-full h-full object-cover"
        />
      ) : name ? (
        <span>{name.charAt(0).toUpperCase()}</span>
      ) : (
        <User className="w-1/2 h-1/2 text-slate-500" />
      )}
    </div>
  );
}
