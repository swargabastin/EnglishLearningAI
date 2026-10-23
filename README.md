# FluentAI / Emma — Adaptive Spoken English Learning System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Next.js 14](https://img.shields.io/badge/Next.js-14.2-black.svg)](https://nextjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![OpenAI](https://img.shields.io/badge/OpenAI-Whisper%20%2B%20GPT--4o-412991.svg)](https://openai.com/)
[![Tests](https://img.shields.io/badge/Tests-120%20Passing-brightgreen.svg)]()

FluentAI is an adaptive conversational English learning platform powered by an intelligent AI speech coach named **Emma**. The system evaluates each learner's proficiency across CEFR criteria (A1–C2), generates deterministic, personalized daily study plans across four mandatory phases, and provides real-time voice practice with instant pronunciation, grammar, and fluency feedback.

---

## Key Features

### 🎙️ 1. Emma — Live Voice AI Coach (`/practice`)
- **Dual-Engine Speech Recognition:** Real-time browser Web Speech API transcription paired with backend OpenAI Whisper (`whisper-1`) integration with acoustic prompt hints for high-accuracy accented speech capture.
- **Natural Voice Synthesis:** Crystal-clear speech responses via OpenAI TTS (`tts-1` / voice `nova`) with native browser speech synthesis fallback.
- **Pedagogical Feedback Engine:** Real-time detection of common ESL grammar and phrasing errors (e.g. *"I am agree"*, *"passed out"*, *"discuss about"*, *"years experience"*), offering constructive explanations and better conversational alternatives.
- **Curriculum Context Bridge:** Directly links Phase 4 speaking practice to the day's target grammar rule (e.g. Past Simple vs. Present Perfect), prompting Emma to elicit and coach the target concept.

### 📚 2. Adaptive Learning Path & Curriculum (`/plan` & `/assessment`)
- **CEFR Level Validation:** Initial multi-skill assessment covering **Grammar**, **Vocabulary**, **Sentence Construction**, **Reading Comprehension**, and **Speaking**, categorizing learners into **Basic**, **Standard**, or **Advanced** tiers.
- **Four Mandatory Daily Learning Phases:**
  1. **Phase 1 — Grammar Study:** In-depth explanation of daily grammar rules with contextual examples.
  2. **Phase 2 — Sentence Making:** Active sentence construction exercises applying the rule.
  3. **Phase 3 — Practice:** Interactive multiple-choice and fill-in-the-blank practice tasks.
  4. **Phase 4 — AI Speaking with Emma:** Live voice dialogue reinforcing the target grammatical objective.
- **Mastery & Anti-Repetition Engine:** Distinguishes completion from skill mastery, schedules spaced repetition revisions (2-day and 7-day intervals), and maintains historical records to prevent duplicate task generation.

### 🌐 3. "Translate to English" Mode (`/dashboard`)
- Translates native regional phrasing (Malayalam, Hindi, Tamil, or informal ESL) into both:
  - **Natural English** (everyday conversational dialogue).
  - **Formal English** (workplace, business emails, and interviews).
- Provides grammatical breakdown, vocabulary pairings, and phonetic pronunciation guidance.
- One-click **"Practice with Emma"** button initiates an interactive speaking turn around the translated phrase.

### 📄 4. Document / PDF English Practice (`/dashboard`)
- Analyzes uploaded text/PDF documents or pasted notes.
- Generates an executive summary, estimated CEFR reading difficulty, key vocabulary with definitions and sample sentences, and three discussion questions.
- One-click **"Discuss with Emma"** button launches a dedicated speaking session based on the document.

---

## Architecture Overview

FluentAI is engineered following **Clean Architecture** principles and domain-driven separation of concerns:

```text
FluentAI/
├── backend/
│   ├── EnglishLearning.sln
│   ├── EnglishLearning.Domain/           # Entities, Enums, Domain Exceptions, Base Models
│   ├── EnglishLearning.Application/      # DTOs, Business Services, Validators, Repository Interfaces
│   ├── EnglishLearning.Infrastructure/   # EF Core DbContext, PostgreSQL Migrations, Auth, OpenAI Integrations
│   └── EnglishLearning.Api/              # ASP.NET Core 8 Web API, Controllers, Middlewares, Rate Limiting
├── frontend/
│   └── EnglishLearningWeb/               # Next.js 14 App Router, TypeScript, Tailwind CSS, Lucide Icons
└── tests/
    ├── EnglishLearning.UnitTests/        # 69 Unit Tests (xUnit, Moq, FluentAssertions)
    └── EnglishLearning.IntegrationTests/ # 51 Integration Tests (WebApplicationFactory, InMemory DB)
```

---

## Technology Stack

- **Backend:** C# .NET 8, ASP.NET Core Web API, Entity Framework Core 8, Npgsql.
- **Database:** PostgreSQL 16.
- **Frontend:** Next.js 14.2 (App Router), React 18, TypeScript 5, Tailwind CSS, Lucide React.
- **Authentication:** JWT Bearer tokens with BCrypt password hashing and partitioned rate limiting.
- **AI & Audio Services:**
  - OpenAI GPT-4o-mini (`chat/completions`)
  - OpenAI Whisper (`audio/transcriptions`)
  - OpenAI TTS (`audio/speech`)
  - Web Audio API (MediaRecorder) + Web Speech API (SpeechRecognition & SpeechSynthesis)

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) & `npm`
- [PostgreSQL](https://www.postgresql.org/download/) running on `localhost:5432`
- *(Optional)* [OpenAI API Key](https://platform.openai.com/api-keys) for live GPT-4o, Whisper, and TTS.

---

### 1. Database Setup

Create the PostgreSQL database:
```sql
CREATE DATABASE english_learning_db;
```

Update your connection string in `backend/EnglishLearning.Api/appsettings.json` or configure it via environment variables:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=english_learning_db;Username=postgres;Password=your_password"
}
```

Apply migrations:
```bash
dotnet ef database update --project backend/EnglishLearning.Infrastructure --startup-project backend/EnglishLearning.Api
```

---

### 2. Run the Backend API

```bash
# Navigate to the API directory
cd backend/EnglishLearning.Api

# (Optional) Export your OpenAI API key for live AI features
export OPENAI_API_KEY="sk-..."    # On Linux/macOS
# $env:OPENAI_API_KEY="sk-..."   # On Windows PowerShell

# Run the API server
dotnet run
```
The API server will start on **`http://localhost:5288`** (Swagger docs available at `http://localhost:5288/swagger`).

---

### 3. Run the Frontend Web Application

```bash
# Navigate to the frontend directory
cd frontend/EnglishLearningWeb

# Install dependencies
npm install

# Start development server
npm run dev
```
The web app will start on **`http://localhost:3001`**.

---

## Automated Test Suite

Run the full suite of unit and integration tests:

```bash
dotnet test backend/EnglishLearning.sln
```

Test Results:
- **Unit Tests:** 69 / 69 passed (Domain logic, validators, adaptive planning, tutor prompts).
- **Integration Tests:** 51 / 51 passed (Auth, speech uploads, conversation sessions, rate limiting, security headers).
- **Total:** 120 / 120 passing (100%).

Lint check for frontend:
```bash
cd frontend/EnglishLearningWeb
npm run lint
```

---

## API Endpoints Summary

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register a new learner account |
| `POST` | `/api/v1/auth/login` | Authenticate and obtain JWT token |
| `GET` | `/api/v1/auth/me` | Retrieve authenticated user profile |
| `GET` | `/api/v1/topics` | Get active practice conversation topics |
| `POST` | `/api/v1/conversations` | Start a conversation session (supports `targetObjective`) |
| `POST` | `/api/v1/conversations/{id}/messages` | Send a text turn to Emma |
| `POST` | `/api/v1/conversations/{id}/speech` | Upload audio + client transcript for speech evaluation |
| `POST` | `/api/v1/conversations/{id}/end` | Complete conversation and generate assessment |
| `GET` | `/api/v1/adaptivelearning/plan/today` | Fetch or generate idempotent 4-phase daily study plan |
| `POST` | `/api/v1/adaptivelearning/plan/phase/progress` | Submit phase completion evidence & scores |
| `POST` | `/api/v1/adaptivelearning/translate` | Translate native phrase to Natural & Formal English |
| `POST` | `/api/v1/adaptivelearning/document/analyze` | Extract summary, CEFR score, vocabulary & questions |

---

## Security & Hardening Highlights

- **JWT Fail-Safe:** API throws on startup if `Jwt:Secret` is missing in non-development environments.
- **Partitioned Rate Limiting:** 10 requests/minute per IP on authentication routes (`/auth/login`, `/auth/register`) to stop brute-force attacks; 60 requests/minute per IP global limiter.
- **Production Security Headers:** `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, and restricted `Permissions-Policy` configured in `next.config.mjs`.
- **Zero Mock Fallbacks in Live UI:** Dashboards and practice views consume live authenticated services with proper loading and error states.

---

## License

This project is licensed under the [MIT License](LICENSE).
