# AI English Learning App — System Architecture Document

**Document Version:** 1.1 (Post-Review Architecture Baseline)  
**Status:** Approved Architecture  
**Target Milestone:** Phase 1 — Requirements and Architecture  
**Base Specification:** [SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/SRS.md)

---

## 1. System Overview

The **AI English Learning Application** is a voice-first, AI-driven language acquisition platform designed to provide learners with natural, level-appropriate spoken English practice. The application enables learners to engage in spoken dialogues tailored to their Common European Framework of Reference for Languages (CEFR) level (A1 to C2) and chosen topic, followed by post-session assessments covering grammar, vocabulary, fluency, and overall communication score.

### 1.1 Architecture Goals & Principles

1. **Voice-First User Experience:** Frictionless audio capture and playback allowing natural conversational practice.
2. **Provider Agnosticism via Abstractions:** Decoupling of AI/LLM, Speech-to-Text (STT), and Text-to-Speech (TTS) engines via backend interface abstractions. For the MVP, exactly **one** concrete provider is implemented per service (OpenAI API stack).
3. **Clean Architecture Separation:** Strict boundary separation between Presentation (API), Application, Domain, and Infrastructure in the backend, alongside modular Next.js frontend layers.
4. **Security & Zero Secret Leakage:** Server-side orchestration ensuring third-party AI keys, billing credentials, and internal prompt templates never reach the client browser.
5. **Simplicity First for MVP:** Direct synchronous REST request/response flow without prematurely introducing message brokers (RabbitMQ), distributed caches (Redis), background task queues (Hangfire), WebSockets, or container orchestrators (Kubernetes/Docker).

### 1.2 High-Level System Architecture Diagram (MVP)

```text
Next.js Frontend (TypeScript + Tailwind CSS)
    ↓ HTTPS / REST (JWT)
ASP.NET Core Web API (Controllers, Middlewares, Filters)
    ↓
Application Services (Conversation, Assessment, Progress, Profile)
    ↓
Domain (Entities, Value Objects, Enums, Business Rules)
    ↓
Infrastructure
    ├── PostgreSQL (EF Core 8 with Npgsql)
    ├── AI Provider (IAiConversationService -> OpenAiConversationService)
    ├── STT Provider (ISpeechToTextService -> WhisperSpeechToTextService)
    └── TTS Provider (ITextToSpeechService -> OpenAiTextToSpeechService)
```

---

## 2. Technology Stack & Framework Versions

A single, consistent .NET target framework (`net8.0`) is used across all backend projects and test suites, verified against the installed .NET SDK.

| Layer / Concern | Technology Selection | Version / Standard | Scope in Project |
|---|---|---|---|
| **Frontend Framework** | Next.js (App Router) | 14.x / 15.x | Web client application shell, routing, pages, and components. |
| **Frontend Language** | TypeScript | 5.x | Strict type safety across client UI, state, and API contracts. |
| **Styling & Design System**| Tailwind CSS | 3.4+ / 4.x | Responsive design tokens, utility classes, dynamic styling. |
| **UI Components & Icons** | Lucide React | Latest | Modern, lightweight SVG iconography. |
| **Client Audio Handling** | Web Audio API / MediaRecorder | W3C Standard | Browser-based audio capture (WebM/WAV) and playback. |
| **Target Framework** | .NET (C# 12) | `net8.0` (LTS) | Consistent across all 4 backend and 2 test projects. |
| **Backend Framework** | ASP.NET Core Web API | 8.0 LTS | REST API endpoints, JWT authentication, and dependency injection. |
| **ORM / Data Access** | Entity Framework Core | 8.0 LTS | Object-Relational Mapping, LINQ queries, and code-first migrations. |
| **Database Engine** | PostgreSQL | 16+ | Primary relational database for all application entities. |
| **Database Driver** | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0+ | PostgreSQL provider for EF Core. |
| **Authentication** | JWT (JSON Web Tokens) | RFC 7519 | Stateless bearer token authentication. |
| **Password Hashing** | ASP.NET Core PasswordHasher / PBKDF2 | HMAC-SHA512 | Cryptographic salting and password hashing. |
| **Validation** | FluentValidation | 11.x | Strongly typed request DTO validation rules. |
| **AI LLM Provider (MVP)** | OpenAI API (GPT-4o-mini) | REST / JSON | Single concrete implementation for conversation & assessment. |
| **STT Provider (MVP)** | OpenAI Whisper API | REST / Multipart | Single concrete implementation for audio transcription. |
| **TTS Provider (MVP)** | OpenAI Audio TTS API | REST / Binary Stream | Single concrete implementation for natural speech synthesis. |
| **Logging** | Serilog | 3.x / 4.x | Structured JSON logging with trace context. |
| **API Documentation** | Swashbuckle (Swagger/OpenAPI) | 6.x | Interactive API documentation and contract testing. |

---

## 3. Frontend Architecture

### 3.1 Modular Directory Structure

```text
frontend/EnglishLearningWeb/
├── public/
│   ├── audio/                      # Audio cues
│   └── images/                     # Static assets & brand marks
├── src/
│   ├── app/                        # Next.js App Router
│   │   ├── (auth)/
│   │   │   ├── login/page.tsx      # User Login (FR-002)
│   │   │   └── register/page.tsx   # User Registration (FR-001)
│   │   ├── (dashboard)/
│   │   │   ├── layout.tsx          # Navigation shell & user header
│   │   │   ├── page.tsx            # Main Dashboard (FR-021, SRS 9.2)
│   │   │   ├── topics/page.tsx     # Topic Selection Catalog (FR-006)
│   │   │   ├── conversation/
│   │   │   │   └── [id]/
│   │   │   │       ├── page.tsx    # Live Speaking Practice Screen (FR-008..FR-013, SRS 9.3)
│   │   │   │       └── result/
│   │   │   │           └── page.tsx# Session Assessment & Feedback (FR-014..FR-019, SRS 9.4)
│   │   │   ├── history/page.tsx    # Conversation History (FR-020)
│   │   │   └── profile/page.tsx    # Profile & CEFR Level (FR-004, FR-005)
│   │   ├── globals.css             # Tailwind base & utilities
│   │   └── layout.tsx              # Root HTML layout & Context Providers
│   ├── components/                 # Reusable UI component modules
│   │   ├── audio/                  # AudioVisualizer, MicButton, AudioPlayer
│   │   ├── conversation/           # ChatBubble, LiveTranscript, SessionControls
│   │   ├── assessment/             # ScoreCard, CorrectionItem, FeedbackSection
│   │   ├── dashboard/              # DailyGoalCard, LevelBadge, RecentSessionsList
│   │   └── ui/                     # Button, Input, Modal, Card, Progress
│   ├── hooks/                      # Custom hooks (useAudioRecorder, useAudioPlayer, useConversation, useAuth)
│   ├── services/                   # Typed API clients (apiClient, authService, conversationService, etc.)
│   ├── context/                    # React Context (AuthContext)
│   ├── types/                      # TypeScript DTO interfaces
│   └── lib/                        # Helpers, formatters, constants
├── tailwind.config.ts
├── tsconfig.json
└── package.json
```

### 3.2 Client Audio & Conversation Turn State Flow

1. **User Speaks:** Learner clicks `MicButton` -> `useAudioRecorder` records microphone input into an audio Blob (WebM/WAV).
2. **Turn Submission:** WebApp posts audio Blob to `POST /api/v1/conversations/{id}/messages/audio`.
3. **Turn Response:** Server transcribes audio, generates AI tutor reply, synthesizes audio, and returns user transcript + AI text + AI audio data.
4. **Playback & Update:** WebApp appends transcript to message list and plays tutor speech using `useAudioPlayer`.

---

## 4. Backend Architecture

### 4.1 Clean Architecture Solution Structure

All projects target `net8.0` uniformly:

```text
backend/EnglishLearningAPI/
├── EnglishLearning.sln
│
├── EnglishLearning.Domain/           [Target: net8.0]
│   ├── Entities/ (User, Topic, ConversationSession, ConversationMessage, Assessment, UserVocabulary)
│   ├── Enums/ (EnglishProficiencyLevel, SpeakerRole, SessionStatus, LearningGoal)
│   └── Exceptions/ (DomainException, EntityNotFoundException, DuplicateEmailException)
│
├── EnglishLearning.Application/      [Target: net8.0]
│   ├── Interfaces/
│   │   ├── Services/ (IAuthService, IConversationService, IAssessmentService, ITopicService, IProgressService)
│   │   ├── Infrastructure/ (IAiConversationService, ISpeechToTextService, ITextToSpeechService, IPasswordHasher)
│   │   └── Repositories/ (IUnitOfWork, IUserRepository, IConversationRepository, ITopicRepository, IAssessmentRepository)
│   ├── DTOs/ (Auth, Conversation, Assessment, Topic, Profile)
│   └── Validators/ (FluentValidation request validators)
│
├── EnglishLearning.Infrastructure/   [Target: net8.0]
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/ (Entity type configurations)
│   │   ├── Migrations/
│   │   └── Repositories/
│   ├── ExternalServices/
│   │   ├── AI/ (OpenAiConversationService, Prompts/PromptBuilder)
│   │   ├── Speech/ (WhisperSpeechToTextService, OpenAiTextToSpeechService)
│   │   └── Auth/ (JwtTokenGenerator, Pbkdf2PasswordHasher)
│   └── Extensions/ServiceCollectionExtensions.cs
│
└── EnglishLearning.Api/              [Target: net8.0]
    ├── Controllers/ (AuthController, UsersController, TopicsController, ConversationsController, AssessmentsController, ProgressController)
    ├── Middlewares/ (GlobalExceptionHandlingMiddleware, RequestLoggingMiddleware)
    ├── Filters/ (ValidationFilter)
    ├── appsettings.json
    └── Program.cs
```

### 4.2 Single Concrete Provider Implementation for MVP

In accordance with MVP simplicity:
- **`IAiConversationService`** is implemented by `OpenAiConversationService` (using GPT-4o-mini).
- **`ISpeechToTextService`** is implemented by `WhisperSpeechToTextService` (using OpenAI Whisper API).
- **`ITextToSpeechService`** is implemented by `OpenAiTextToSpeechService` (using OpenAI TTS API).

Other providers (Azure Speech, ElevenLabs, Anthropic) remain possible future implementations without changing application interfaces or domain code.

---

## 5. PostgreSQL Database Architecture

### 5.1 Tables & Entity Relationships

1. **`users`**: Learner credentials, CEFR level (`A1`..`C2`), primary goal, and timestamps.
2. **`topics`**: Standard practice topics (Job Interview, Travel, Daily Conversation, etc.) with difficulty indicators.
3. **`conversation_sessions`**: Practice sessions with learner ID, topic ID, CEFR level at session time, duration, and completion status.
4. **`conversation_messages`**: Ordered dialogue messages (`User` / `Assistant`) for each session.
5. **`assessments`**: Post-session evaluation linked 1-to-1 with `conversation_sessions`, storing numeric scores (grammar, vocabulary, fluency, overall) and structured JSONB columns for corrections and feedback.
6. **`user_vocabulary`**: Tracked vocabulary items identified during conversations for future review.
7. **`user_daily_progress`**: Daily aggregated practice duration, completed session count, and daily score average.

### 5.2 Schema Details & Data Types

- Primary Keys: PostgreSQL `UUID` (`gen_random_uuid()`).
- Scores: `NUMERIC(5,2)` for accurate 0.00 to 100.00 score representation.
- Timestamps: `TIMESTAMPTZ` (UTC).
- Complex Feedback: Native `JSONB` for structured arrays of grammar corrections, strengths, improvement areas, and vocabulary suggestions.

---

## 6. API Architecture

### 6.1 Versioning & Response Conventions

- **Base Route:** `/api/v1`
- **Auth Header:** `Authorization: Bearer <jwt>`
- **Standard Envelope:** `ApiResponse<T>` with `{ success, data, message, timestamp }`
- **Standard Error:** RFC 7807 `ProblemDetails`

### 6.2 Endpoint Catalog

- **Auth:**
  - `POST /api/v1/auth/register` (FR-001)
  - `POST /api/v1/auth/login` (FR-002)
  - `GET /api/v1/auth/me` (FR-004)
- **Profile:**
  - `GET /api/v1/users/profile` (FR-004)
  - `PUT /api/v1/users/profile` (FR-004)
  - `PUT /api/v1/users/level` (FR-005)
- **Topics:**
  - `GET /api/v1/topics` (FR-006)
  - `GET /api/v1/topics/{id}` (FR-006)
- **Conversations:**
  - `POST /api/v1/conversations` (FR-007, FR-010) — Starts session and returns tutor greeting.
  - `GET /api/v1/conversations/{id}` (FR-011) — Gets message history.
  - `POST /api/v1/conversations/{id}/messages/audio` (FR-008..FR-012) — Sends audio turn; returns transcript, AI text, and synthesized audio.
  - `POST /api/v1/conversations/{id}/messages/text` (FR-010..FR-012) — Text fallback turn.
  - `POST /api/v1/conversations/{id}/end` (FR-013) — Ends session and generates assessment.
- **Assessments & Progress:**
  - `GET /api/v1/assessments/session/{sessionId}` (FR-014..FR-019)
  - `GET /api/v1/progress/dashboard` (FR-021)
  - `GET /api/v1/progress/history` (FR-020)

---

## 7. AI & Speech Architecture

### 7.1 Provider Interfaces

```csharp
// Application/Interfaces/Infrastructure/IAiConversationService.cs
public interface IAiConversationService
{
    Task<AiMessageResponse> GenerateGreetingAsync(Topic topic, EnglishProficiencyLevel level, CancellationToken ct = default);
    Task<AiMessageResponse> GenerateNextTurnAsync(ConversationContext context, string userTranscript, CancellationToken ct = default);
}

// Application/Interfaces/Infrastructure/IAssessmentService.cs
public interface IAssessmentService
{
    Task<AssessmentResultDto> GenerateAssessmentAsync(ConversationSession session, IReadOnlyList<ConversationMessage> messages, CancellationToken ct = default);
}

// Application/Interfaces/Infrastructure/ISpeechToTextService.cs
public interface ISpeechToTextService
{
    Task<SpeechRecognitionResult> TranscribeAsync(Stream audioStream, string contentType, string languageCode = "en-US", CancellationToken ct = default);
}

// Application/Interfaces/Infrastructure/ITextToSpeechService.cs
public interface ITextToSpeechService
{
    Task<TextToSpeechResult> SynthesizeAsync(string text, VoiceOptions options, CancellationToken ct = default);
}
```

### 7.2 Prompt Strategy
- Dynamic system prompt incorporating learner's CEFR level (A1..C2) to constrain vocabulary complexity and sentence length.
- Structured JSON output mode for the assessment prompt to ensure reliable parsing of grammar corrections, vocabulary suggestions, and fluency metrics.

---

## 8. Authentication & Security

- **JWT Bearer Authentication:** Signed with HMAC-SHA256. Carries standard user claims (`sub`, `email`, `name`, `level`).
- **Password Security:** Salted hashes using PBKDF2 with HMAC-SHA512.
- **Credential Protection:** External AI and speech API keys remain strictly in backend configuration (`appsettings.json` / Environment Variables).
- **CORS & Input Validation:** Restricted CORS to frontend origin and FluentValidation on all API input DTOs.

---

## 9. Development Phases & Roadmap

Feature development proceeds through 10 sequential phases:

- **Phase 1 — Requirements & Architecture:** Finalize SRS, Architecture Document, and Architecture Review.
- **Phase 2 — Project Skeleton:** Initialize Next.js app, ASP.NET Core solution targeting `net8.0`, PostgreSQL connection, and test projects.
- **Phase 3 — Frontend UI:** Implement all 8 core screens in Next.js with Tailwind CSS and mock state.
- **Phase 4 — Backend & Database:** Implement Domain Entities, EF Core `AppDbContext`, Migrations, Repository layer, and Swagger.
- **Phase 5 — Authentication:** Implement Registration, Login, JWT issuance, Password Hashing, and frontend AuthContext.
- **Phase 6 — AI Conversation:** Implement `IAiConversationService`, prompt builders, topic context handling, and text conversation endpoint.
- **Phase 7 — Speech Integration:** Implement `ISpeechToTextService`, `ITextToSpeechService`, browser recording (`useAudioRecorder`), and audio turn endpoint.
- **Phase 8 — Assessment:** Implement `IAssessmentService`, structured evaluation JSON generation, score persistence, and Results UI.
- **Phase 9 — Progress & History:** Implement History pagination, Daily Progress calculation, and Dashboard metrics.
- **Phase 10 — Testing & Hardening:** Complete unit tests, integration tests, end-to-end user flows, security review, and error handling hardening.

---

## 10. Future Scalability Considerations (Post-MVP)

The following items are explicitly **excluded from the MVP** and reserved for future scale phases:
- Distributed caching with Redis
- Asynchronous task queues with RabbitMQ / Hangfire
- Real-time bidirectional streaming with SignalR / WebSockets
- Containerization and orchestration with Docker / Kubernetes
- Microservices decomposition
- Multiple alternative AI/Speech provider fallbacks
- Detailed pronunciation phoneme scoring (FR-017)

---
*End of System Architecture Document.*
