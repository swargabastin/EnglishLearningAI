---
name: ui-ux-design-system
description: >-
  Maintains the premium, modern, accessible English learning user interface and practice screen design system.
  Use whenever implementing or modifying frontend components, practice views, dashboards, audio controls,
  or responsive layouts.
---

# UI/UX Design System — English Learning AI

This skill governs the visual design language, accessibility, and interactive states of the Next.js frontend.

## Product Experience Feel
The application is a dedicated **AI English Learning Product**, not a generic admin dashboard:
- **Tone:** Modern, premium, clean, friendly, educational, and trustworthy.
- **Visual Design:** Curated HSL color palettes, balanced whitespace, clear typographic hierarchy, subtle card borders (`border-slate-200/80`), and smooth micro-interactions.

## Practice Screen Interaction States
The `/practice` interface is the core learner workflow. The voice and interaction states must be clearly and distinctly represented:

| State | Visual Cues | Behavior |
| :--- | :--- | :--- |
| **`ready` / `idle`** | Solid blue mic button (`bg-blue-600`), helper text. | Ready to start recording speech. |
| **`recording`** | Red pulsing button, live duration timer (`00:04`), active waveform. | User is actively speaking into mic. |
| **`transcribing` / `processing`** | Rotating spinner icon, status badge. | Whisper STT is processing uploaded audio. |
| **`ai_responding` / `ai_thinking`** | Pulsing indicator, Emma thinking message. | AI Tutor Emma is reasoning and synthesizing voice. |
| **`speaking`** | Glowing audio wave, active playing badge. | TTS audio is playing back in browser. |
| **`error`** | Red badge, clear error message, retry button. | Displays actionable remediation (e.g. mic permission, audio format). |

## Dialogue Feed Clarity
The conversation feed must unambiguously differentiate:
1. **User Message:** Clean right-aligned blue bubble with timestamp and transcription status.
2. **Emma's Response:** Left-aligned crisp white card with Emma's avatar, clear typography, and CEFR-appropriate phrasing.
3. **Emma's Audio:** Interactive voice playback button (`Listen to Emma` / `Pause Voice`) that plays real synthesized MP3 audio.

## Design Rules
1. **No Fake / Flashing Placeholders:** Never initialize components with dummy mock dialogue that flashes before real data loads. Show clean loading skeletons instead.
2. **Accessibility Standards:** High contrast ratios, visible focus rings, aria-labels on interactive controls, and keyboard navigability.
3. **Responsive Adaptability:** Fully fluid across Mobile (375px+), Tablet (768px+), and Desktop (1024px+).
4. **Preserve Design Language:** Do not introduce unrelated third-party component libraries or redesign working screens without explicit authorization.
