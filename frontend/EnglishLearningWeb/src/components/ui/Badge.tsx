import React from "react";
import { CEFRLevel } from "@/types";

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: "default" | "success" | "warning" | "info" | "danger" | "outline";
  cefrLevel?: CEFRLevel | "All";
  size?: "sm" | "md";
}

export function Badge({
  children,
  variant = "default",
  cefrLevel,
  size = "md",
  className = "",
  ...props
}: BadgeProps) {
  const sizeStyles = {
    sm: "text-[11px] font-semibold px-2 py-0.5 rounded-md",
    md: "text-xs font-semibold px-2.5 py-1 rounded-lg",
  };

  let variantStyles = "bg-slate-100 text-slate-700 border border-slate-200";

  if (cefrLevel) {
    switch (cefrLevel) {
      case "A1":
      case "A2":
        variantStyles = "bg-emerald-50 text-emerald-700 border border-emerald-200";
        break;
      case "B1":
      case "B2":
        variantStyles = "bg-blue-50 text-blue-700 border border-blue-200";
        break;
      case "C1":
      case "C2":
        variantStyles = "bg-amber-50 text-amber-800 border border-amber-200";
        break;
      default:
        variantStyles = "bg-slate-100 text-slate-700 border border-slate-200";
    }
  } else {
    switch (variant) {
      case "success":
        variantStyles = "bg-emerald-50 text-emerald-700 border border-emerald-200";
        break;
      case "warning":
        variantStyles = "bg-amber-50 text-amber-700 border border-amber-200";
        break;
      case "info":
        variantStyles = "bg-sky-50 text-sky-700 border border-sky-200";
        break;
      case "danger":
        variantStyles = "bg-red-50 text-red-700 border border-red-200";
        break;
      case "outline":
        variantStyles = "bg-transparent text-slate-600 border border-slate-300";
        break;
      default:
        variantStyles = "bg-slate-100 text-slate-700 border border-slate-200";
    }
  }

  return (
    <span
      className={`inline-flex items-center justify-center font-medium tracking-tight ${sizeStyles[size]} ${variantStyles} ${className}`}
      {...props}
    >
      {cefrLevel ? `Level ${cefrLevel}` : children}
    </span>
  );
}
