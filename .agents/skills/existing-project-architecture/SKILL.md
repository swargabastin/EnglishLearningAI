---
name: existing-project-architecture
description: >-
  Defines and protects the approved, implemented architecture of the English Learning AI project across
  Next.js frontend, ASP.NET Core backend, and PostgreSQL database. Use whenever navigating, inspecting,
  extending, or debugging any part of this existing codebase.
---

# Existing Project Architecture — English Learning AI

This skill preserves the approved, canonical architecture of the existing English Learning AI project.

## Full System Architecture

```text
Next.js 14 Web Frontend
       │
       │ HTTPS / REST / JWT Bearer (/api/v1)
       ▼
ASP.NET Core 8 Web API
       │
       ▼
Application Layer
       │
       ▼
Domain Layer
       ▲
       │
Infrastructure Layer ──► PostgreSQL (english_learning_db)
                     ──► OpenAI GPT (gpt-4o-mini)
                     ──► OpenAI Whisper (whisper-1 STT)
                     ──► OpenAI TTS (tts-1 Nova)
```

## Solution Layout

### Backend (`backend/EnglishLearning.sln`)
- `EnglishLearning.Domain/` — Entities, Enums, Exceptions, Domain Rules
- `EnglishLearning.Application/` — DTOs, Service Interfaces, Use Cases, Validators
- `EnglishLearning.Infrastructure/` — EF Core, PostgreSQL, Repositories, AI Providers
- `EnglishLearning.Api/` — Controllers, Middleware, JWT Auth, Swagger
- `tests/EnglishLearning.UnitTests/` — Fast isolated unit tests
- `tests/EnglishLearning.IntegrationTests/` — WebApplicationFactory & API integration tests

### Frontend (`frontend/EnglishLearningWeb/`)
Built with **Next.js 14 (App Router)**, **React 18**, **TypeScript 5**, and **Tailwind CSS 3.4**:
- `src/app/` — Route pages: `/`, `/login`, `/register`, `/dashboard`, `/topics`, `/practice`, `/practice/result`, `/history`, `/profile`
- `src/components/` — Focused reusable UI components (`practice/`, `dashboard/`, `history/`, `assessment/`, `layout/`, `ui/`)
- `src/hooks/` — Reusable client interaction logic (`useAudioRecorder`)
- `src/services/` — Centralized typed API clients (`authService`, `conversationService`, `assessmentService`, `progressService`)
- `src/context/` — Centralized global state (`AuthContext`)
- `src/types/` — Shared TypeScript data models and CEFR definitions
- `src/lib/` — Shared utility helpers

## Critical Architectural Invariants
1. **Frontend Architecture is NOT MVVM:** Do NOT migrate the frontend to MVVM or MVC. Do NOT introduce Redux or MobX.
2. **API Boundary Protection:** The frontend communicates strictly via `/api/v1` REST endpoints. The browser must NEVER connect directly to PostgreSQL or call external OpenAI APIs.
3. **No Project Recreation:** Never create parallel projects, regenerate the repository, or replace completed layers.
4. **Preserve Contracts:** Do not modify API DTO contracts or route schemas without explicit requirement and approval.
