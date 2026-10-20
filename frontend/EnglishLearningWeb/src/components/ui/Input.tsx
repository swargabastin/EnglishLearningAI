import React from "react";

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  helperText?: string;
  error?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      label,
      helperText,
      error,
      leftIcon,
      rightIcon,
      id,
      className = "",
      disabled,
      ...props
    },
    ref
  ) => {
    const inputId = id || (label ? label.toLowerCase().replace(/\s+/g, "-") : undefined);

    return (
      <div className="w-full space-y-1.5 text-left">
        {label && (
          <label
            htmlFor={inputId}
            className="block text-xs font-bold text-slate-300 tracking-wide"
          >
            {label}
          </label>
        )}
        <div className="relative rounded-2xl shadow-sm">
          {leftIcon && (
            <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
              {leftIcon}
            </div>
          )}
          <input
            id={inputId}
            ref={ref}
            disabled={disabled}
            className={`block w-full rounded-2xl border bg-slate-800 text-white placeholder:text-slate-500 text-sm transition-all duration-150 py-3 ${
              leftIcon ? "pl-10" : "pl-4"
            } ${rightIcon ? "pr-10" : "pr-4"} ${
              error
                ? "border-red-500 focus:border-red-400 focus:ring-2 focus:ring-red-500/20"
                : "border-slate-700 hover:border-slate-600 focus:border-purple-500 focus:ring-2 focus:ring-purple-500/20 focus:bg-slate-850"
            } ${
              disabled
                ? "bg-slate-900 text-slate-600 cursor-not-allowed opacity-60"
                : ""
            } ${className}`}
            {...props}
          />
          {rightIcon && (
            <div className="absolute inset-y-0 right-0 pr-3.5 flex items-center text-slate-400">
              {rightIcon}
            </div>
          )}
        </div>
        {error ? (
          <p className="text-xs text-red-400 font-medium">{error}</p>
        ) : helperText ? (
          <p className="text-xs text-slate-400">{helperText}</p>
        ) : null}
      </div>
    );
  }
);

Input.displayName = "Input";
