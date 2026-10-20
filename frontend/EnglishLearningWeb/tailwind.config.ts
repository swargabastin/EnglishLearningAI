import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ["var(--font-jakarta)", "system-ui", "-apple-system", "sans-serif"],
      },
      colors: {
        fluent: {
          bg: "#090d16",
          dark: "#0d1322",
          surface: "#121b2f",
          card: "#17223b",
          elevated: "#1e2c4c",
          border: "rgba(148, 163, 184, 0.15)",
          "border-active": "rgba(139, 92, 246, 0.4)",
          purple: {
            DEFAULT: "#7c3aed",
            light: "#8b5cf6",
            dark: "#6d28d9",
            glow: "rgba(124, 58, 237, 0.35)",
          },
          cyan: {
            DEFAULT: "#06b6d4",
            light: "#22d3ee",
            glow: "rgba(6, 182, 212, 0.3)",
          },
        },
        brand: {
          50: "#eff6ff",
          100: "#dbeafe",
          200: "#bfdbfe",
          300: "#93c5fd",
          400: "#60a5fa",
          500: "#3b82f6",
          600: "#2563eb",
          700: "#1d4ed8",
          800: "#1e40af",
          900: "#1e3a8a",
          950: "#172554",
        },
        accent: {
          50: "#f5f3ff",
          100: "#ede9fe",
          200: "#ddd6fe",
          300: "#c4b5fd",
          400: "#a78bfa",
          500: "#8b5cf6",
          600: "#7c3aed",
          700: "#6d28d9",
          800: "#5b21b6",
          900: "#4c1d95",
        },
        slate: {
          850: "#151e2e",
          950: "#0b0f19",
        },
      },
      boxShadow: {
        "xs": "0 1px 2px 0 rgba(0, 0, 0, 0.05)",
        "soft-sm": "0 2px 8px -2px rgba(15, 23, 42, 0.06), 0 1px 3px -1px rgba(15, 23, 42, 0.04)",
        "soft-md": "0 6px 16px -4px rgba(15, 23, 42, 0.08), 0 2px 6px -2px rgba(15, 23, 42, 0.04)",
        "soft-lg": "0 12px 28px -6px rgba(15, 23, 42, 0.1), 0 4px 10px -3px rgba(15, 23, 42, 0.05)",
        "soft-xl": "0 20px 40px -12px rgba(15, 23, 42, 0.14)",
        "glow-brand": "0 0 24px -4px rgba(37, 99, 235, 0.35)",
        "glow-accent": "0 0 24px -4px rgba(124, 58, 237, 0.3)",
        "glow-purple": "0 0 32px -4px rgba(124, 58, 237, 0.45)",
        "glow-cyan": "0 0 28px -4px rgba(6, 182, 212, 0.35)",
        "glow-subtle": "0 0 24px -4px rgba(99, 102, 241, 0.25)",
      },
      borderRadius: {
        "3xl": "1.5rem",
        "4xl": "2rem",
      },
      animation: {
        "pulse-slow": "pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite",
        "sound-wave": "soundWave 1.2s ease-in-out infinite",
        "bounce-subtle": "bounceSubtle 2s ease-in-out infinite",
        "glow-pulse": "glowPulse 2.5s ease-in-out infinite",
      },
      keyframes: {
        soundWave: {
          "0%, 100%": { height: "6px" },
          "50%": { height: "24px" },
        },
        bounceSubtle: {
          "0%, 100%": { transform: "translateY(0)" },
          "50%": { transform: "translateY(-4px)" },
        },
        glowPulse: {
          "0%, 100%": { opacity: "0.6", transform: "scale(1)" },
          "50%": { opacity: "1", transform: "scale(1.03)" },
        },
      },
    },
  },
  plugins: [],
};

export default config;
