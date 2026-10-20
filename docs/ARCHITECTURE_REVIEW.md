# AI English Learning App — Architecture Review & Final Sign-Off

**Document Version:** 1.0  
**Date:** 2026-08-14  
**Review Type:** Pre-Implementation Architectural Verification & Alignment  
**Reviewed Documents:**
- [docs/SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/SRS.md) (v1.0)
- [docs/ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md) (v1.1)

---

## 1. Architecture Decision Summary

A comprehensive architectural review was conducted against the project requirements and constraints. The core decisions established for the project include:

1. **Clean Architecture Separation:** The backend is organized into four strictly decoupled projects (`Domain`, `Application`, `Infrastructure`, `API`) to guarantee testability, maintainability, and domain isolation.
2. **Modular Next.js Frontend:** A modular frontend structure using the Next.js App Router, separating Server Components from interactive Client Components, and establishing modular directories (`app`, `components`, `hooks`, `services`, `context`, `types`, `lib`).
3. **Provider Abstraction with Single Concrete MVP Implementation:** External AI, STT, and TTS integrations are strictly hidden behind domain service interfaces (`IAiConversationService`, `ISpeechToTextService`, `ITextToSpeechService`). For the MVP, a single unified concrete provider stack (OpenAI API: GPT-4o-mini, Whisper, TTS) is selected to minimize initial integration complexity while maintaining zero vendor lock-in in the domain/application layers.
4. **Relational Data Integrity with PostgreSQL:** PostgreSQL is the single primary datastore, utilizing native UUID keys, ACID transactions, and JSONB columns for flexible, structured assessment feedback.
5. **Stateless JWT Authentication:** Secure bearer authentication using HMAC-SHA256 signed tokens and PBKDF2 password hashing.
6. **Synchronous REST Flow for MVP:** Keeping the MVP architecture simple and robust by utilizing direct RESTful HTTP turn exchanges without introducing unnecessary infrastructure overhead.

---

## 2. Corrections & Refinements Made

During the review, the following corrections and scope adjustments were applied:

| # | Area | Previous Proposed State | Corrected Architecture Baseline | Rationale |
|---|---|---|---|---|
| 1 | **Target Framework** | Unspecified / Mixed .NET versions | Unified **`net8.0`** across all backend and test projects | Verified installed SDKs (8.0.422 & 10.0.301). Using .NET 8 LTS ensures consistency and package stability across all layers. |
| 2 | **AI / Speech Providers** | Multiple concurrent provider adapters outlined for MVP | **Single concrete provider (OpenAI stack)** implemented behind interfaces | Avoids premature multi-provider complexity in MVP while preserving interface abstraction for future switching. |
| 3 | **Infrastructure Scope** | Mentioned Docker Compose, Redis, Hangfire, SignalR in active design | **Excluded from MVP implementation**; documented strictly as future scale items | Prevents over-engineering and keeps deployment, debugging, and initial implementation straightforward. |
| 4 | **Development Phases** | 11 phases with overlapping scaffolding | **Aligned strictly to 10 sequential phases** (Phase 1 to Phase 10) matching project milestones | Clear, dependency-ordered roadmap ensuring each phase is built and verified incrementally. |
| 5 | **Audio Protocol** | Mixed streaming & multipart references | **Standard `multipart/form-data` REST endpoint** for audio turn submission in MVP | Robust across all standard browser environments without WebSocket lifecycle management. |

---

## 3. Technology Versions Selected

The following verified versions are established as the canonical baseline for all implementation phases:

| Component | Technology | Version / Target | Justification |
|---|---|---|---|
| **Runtime / Target** | .NET SDK | `net8.0` (LTS) | Consistent target across Domain, Application, Infrastructure, API, and Unit/Integration Tests. |
| **Backend API** | ASP.NET Core Web API | 8.0 LTS | Enterprise-grade REST APIs, dependency injection, and middleware pipeline. |
| **ORM** | Entity Framework Core | 8.0 LTS | Linq-to-Entities and automated migration management. |
| **Database Provider** | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.x | Official, stable PostgreSQL provider for EF Core 8. |
| **Database Engine** | PostgreSQL | 16+ | ACID-compliant relational storage with JSONB support. |
| **Frontend Framework**| Next.js (App Router) | 14.x / 15.x | Server-Side Rendering and modern React application structure. |
| **Frontend Language** | TypeScript | 5.x | Strict type safety across client UI, state, and API contracts. |
| **Node.js / npm** | Node.js / npm | v26.3.0 / 11.16.0 | Installed and verified local runtime. |
| **Styling** | Tailwind CSS | 3.4+ / 4.x | Design system tokens and responsive utility styles. |
| **Validation** | FluentValidation | 11.x | Strongly typed request validation rules. |
| **Logging** | Serilog | 3.x / 4.x | Structured JSON logging with trace context. |
| **AI / STT / TTS** | OpenAI API SDK / REST | v1 API | GPT-4o-mini (LLM), Whisper (STT), OpenAI TTS-1 (TTS). |

---

## 4. MVP Architecture

### 4.1 Request & Data Flow Diagram

```text
Next.js Frontend (TypeScript + Tailwind CSS)
    ↓  (HTTPS / REST with JWT Bearer Token)
ASP.NET Core Web API (Controllers, Middlewares, Validation Filters)
    ↓
Application Services (ConversationService, AssessmentService, ProgressService, AuthService)
    ↓
Domain Layer (Entities, Value Objects, Enums, Business Rules)
    ↓
Infrastructure Layer
    ├── PostgreSQL (EF Core 8 with Npgsql)
    ├── AI Provider (OpenAiConversationService : IAiConversationService)
    ├── STT Provider (WhisperSpeechToTextService : ISpeechToTextService)
    └── TTS Provider (OpenAiTextToSpeechService : ITextToSpeechService)
```

### 4.2 Module Dependency Constraints

- **`EnglishLearning.Domain`:** Zero third-party dependencies. Contains only pure C# entities, enums, and domain logic.
- **`EnglishLearning.Application`:** Depends only on `Domain`. Defines all interfaces, DTOs, and business use cases.
- **`EnglishLearning.Infrastructure`:** Implements `Application` interfaces, configuring EF Core, PostgreSQL, and external OpenAI API clients.
- **`EnglishLearning.Api`:** Depends on `Application` and `Infrastructure` for dependency injection wiring and HTTP transport.
- **`EnglishLearningWeb` (Frontend):** Consumes `/api/v1/*` endpoints via typed services. No direct database or external AI communication.

---

## 5. Development Roadmap (Phases 1 to 10)

Feature implementation will proceed strictly through the following sequential phases:

```text
Phase 1  — Requirements & Architecture (Complete)
Phase 2  — Project Skeleton (Solution setup, net8.0 projects, Next.js init, PG connection)
Phase 3  — Frontend UI (8 MVP screens with Tailwind CSS & mock state)
Phase 4  — Backend & Database (Domain entities, AppDbContext, Migrations, Repositories)
Phase 5  — Authentication (Registration, Login, JWT, Password Hashing, AuthContext)
Phase 6  — AI Conversation (IAiConversationService, prompts, topic context, dialogue turns)
Phase 7  — Speech Integration (ISpeechToTextService, ITextToSpeechService, audio capture)
Phase 8  — Assessment (IAssessmentService, JSON evaluation parsing, score calculation, Results UI)
Phase 9  — Progress & History (History list, daily aggregation, Dashboard statistics)
Phase 10 — Testing & Hardening (Unit tests, integration tests, security review, error handling)
```

---

## 6. Future Architecture Items (Post-MVP)

The following architectural components are documented as future scale enhancements and are explicitly excluded from MVP implementation:

1. **Real-Time Bidirectional Streaming:** SignalR / WebSockets for low-latency live audio streaming.
2. **Distributed Caching:** Redis for caching topic catalogs, session state, and CEFR prompt templates.
3. **Asynchronous Task Queuing:** RabbitMQ / Hangfire for background assessment and heavy report generation.
4. **Containerization & Orchestration:** Docker Compose and Kubernetes for containerized cluster deployment.
5. **Multi-Provider AI Fallback:** Dynamic runtime switching between OpenAI, Azure OpenAI, Anthropic Claude, and Deepgram.
6. **Detailed Pronunciation Assessment:** Phoneme-level acoustic scoring (FR-017) via dedicated speech assessment APIs.

---

## 7. Remaining Technical Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation Strategy |
|---|---|---|---|
| **Audio Format Browser Variations** | Medium | Medium | Client records in standard WebM/Opus with fallback WAV; Whisper STT service natively handles both formats. |
| **External AI Response Latency** | Medium | Medium | Use lightweight, fast model (`gpt-4o-mini`); cap conversation context to recent 10 turns; optimize audio bitrate. |
| **Microphone Permission Handling** | Medium | Low | Comprehensive UI error states and clear user instructions in the frontend audio hook. |
| **API Rate Limits / Outages** | Low | High | Polly exponential backoff retry policies and graceful user-facing error banners. |

---

## 8. Final Approval Status

All functional and non-functional requirements from [SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/SRS.md) have been analyzed, verified, and mapped into a production-grade, modular system architecture in [ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md). All corrections regarding version consistency, MVP simplicity, provider abstraction, and development milestones are in place.

**ARCHITECTURE STATUS: APPROVED FOR IMPLEMENTATION**
