---
name: clean-architecture
description: >-
  Enforces Clean Architecture rules and dependency boundaries for the ASP.NET Core backend
  (Domain, Application, Infrastructure, and API layers). Use whenever inspecting, designing,
  modifying, or refactoring backend services, entities, repositories, and controllers.
---

# Clean Architecture — English Learning AI

This skill enforces the canonical Clean Architecture dependency rules for the ASP.NET Core backend of the English Learning AI application.

## Dependency Hierarchy

```text
       ┌──────────────┐
       │     API      │  (HTTP Transport, Controllers, DI, Middleware)
       └──────┬───────┘
              │
              ▼
       ┌──────────────┐
       │ Application  │  (Use Cases, DTOs, Interfaces, Validators, Workflows)
       └──────┬───────┘
              │
              ▼
       ┌──────────────┐
       │    Domain    │  (Entities, Value Objects, Enums, Domain Exceptions)
       └──────────────┘
              ▲
              │
       ┌──────┴───────┐
       │Infrastructure│  (EF Core, PostgreSQL, Repositories, OpenAI, Whisper, TTS)
       └──────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (`EnglishLearning.Domain`)
- **Contents:** Core domain entities (`User`, `Topic`, `ConversationSession`, `ConversationMessage`, `Assessment`, `Vocabulary`), enums, domain exceptions, and business rules.
- **Strict Rule:** Domain is the innermost layer and must remain completely independent.
- **Prohibited:** Domain must NOT reference ASP.NET Core, EF Core, PostgreSQL, OpenAI SDK, HTTP clients, Next.js, or Infrastructure implementations.

### 2. Application Layer (`EnglishLearning.Application`)
- **Contents:** Application use cases, service orchestrations, DTOs, FluentValidation validators, and external capability abstractions (`IAiConversationService`, `ISpeechToTextService`, `ITextToSpeechService`, `IAssessmentService`, `IProgressService`, `IUserRepository`, `IConversationSessionRepository`).
- **Dependencies:** Depends strictly on `EnglishLearning.Domain`.
- **Prohibited:** Application must NOT depend on concrete Infrastructure classes, PostgreSQL specifics, or external OpenAI SDKs.

### 3. Infrastructure Layer (`EnglishLearning.Infrastructure`)
- **Contents:** EF Core `AppDbContext`, Npgsql PostgreSQL provider configurations, repository implementations, password hashing, JWT generation, and external AI providers (`OpenAiConversationService`, `WhisperSpeechToTextService`, `OpenAiTextToSpeechService`, `OpenAiAssessmentService`).
- **Dependencies:** Implements interfaces defined in `EnglishLearning.Application`.

### 4. API Layer (`EnglishLearning.Api`)
- **Contents:** Controllers (`AuthController`, `TopicsController`, `ConversationsController`, `ProgressController`, `HealthController`), JWT bearer authentication middleware, rate limiting, and global exception handling.
- **Rule:** Controllers must remain thin delegators. Business calculations, scoring algorithms, and database queries belong in Application services and Infrastructure repositories.

## Core Architectural Guardrails
1. **Identify the Owner Layer First:** Before writing or modifying backend code, identify the correct layer that owns the change.
2. **No Circular Dependencies:** Never introduce circular project references.
3. **No Microservices / No Architecture Drift:** Do NOT replace or fragment Clean Architecture into microservices or distributed buses.
4. **Preserve Thin Controllers:** Never place business logic, SQL queries, or AI prompts directly inside API controllers.
