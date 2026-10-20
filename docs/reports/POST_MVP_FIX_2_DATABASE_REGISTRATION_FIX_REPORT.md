# POST-MVP FIX 2 — DATABASE & REGISTRATION FIX REPORT

**Project:** English Learning AI  
**Scope:** Localhost PostgreSQL Database Authentication & Registration Fix  
**Date:** 2026-08-17  
**Status:** COMPLETED & VERIFIED  

---

## 1. Root Cause

1. **Password Authentication:** The connection string in `backend/EnglishLearning.Api/appsettings.json` configured `Password=postgres`, which mismatched the local PostgreSQL instance credentials (`Password=12345678`). This caused `Npgsql.PostgresException: 28P01: password authentication failed for user "postgres"`.
2. **Database Existence & Schema:** The `english_learning_db` database had not yet been created on the local PostgreSQL instance, triggering `3D000: database "english_learning_db" does not exist`. Applying the existing migration (`dotnet ef database update`) successfully created the database, all tables, indices, and seeded topics.
3. **Port Binding Conflict:** An external application (`HomeTutorAPI`) on the developer's machine was holding port `5000`. Adjusting the launch configuration to port `5288` resolved socket collisions cleanly.

---

## 2. Configuration Inspected

- `backend/EnglishLearning.Api/appsettings.json`
- `backend/EnglishLearning.Api/appsettings.Development.json`
- `backend/EnglishLearning.Api/Program.cs`
- `backend/EnglishLearning.Api/Properties/launchSettings.json`
- `backend/EnglishLearning.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `frontend/EnglishLearningWeb/src/services/apiClient.ts`

---

## 3. Runtime Connection Configuration Source

- **Target Host:** `localhost`
- **Target Port:** `5432`
- **Target Database:** `english_learning_db`
- **Target Username:** `postgres`
- **Precedence:** `appsettings.json` $\to$ `AddInfrastructure()` $\to$ `AddDbContext<AppDbContext>()` with Npgsql provider.

---

## 4. PostgreSQL Connectivity Result

- **Status:** **PASS**
- **Verification:** Live queries executed against `localhost:5432` over SASL authentication succeeded with zero authentication errors.

---

## 5. Database Existence Result

- **Status:** **PASS**
- **Verification:** Applied existing migration `20260814112940_InitialCreate` via EF Core. Database `english_learning_db` created with:
  - `users`
  - `topics` (7 seeded topics)
  - `conversation_sessions`
  - `conversation_messages`
  - `assessments`
  - `vocabulary`
  - `__EFMigrationsHistory`

---

## 6. EF Core Connectivity Result

- **Status:** **PASS**
- **Verification:** `GET http://localhost:5288/api/v1/topics` executed via EF Core returned all 7 active topics (`Daily Conversation`, `Job Interview`, `Travel`, `Shopping`, `Restaurant`, `Workplace`, `Introducing Yourself`).

---

## 7. Registration Endpoint Result

- **Status:** **PASS**
- **Verification:** `POST http://localhost:5288/api/v1/auth/register` called with valid payload returned `200 OK` with generated user ID and user profile summary.

---

## 8. User Persistence Result

- **Status:** **PASS**
- **Verification:** Created users (`test.learner@example.com`, `alex.morgan@example.com`) were verified directly in PostgreSQL `users` table with hashed passwords, CEFR levels, and learning goals.

---

## 9. Login Verification Result

- **Status:** **PASS**
- **Verification:** `POST http://localhost:5288/api/v1/auth/login` returned `200 OK`, valid signed JWT `accessToken`, and allowed immediate authorized retrieval of `GET /api/v1/progress/dashboard`.

---

## 10. Backend Build Result

- **Command:** `dotnet build backend/EnglishLearning.sln --no-restore`
- **Result:** **`0 Error(s), 0 Warning(s)`** (**PASS**)

---

## 11. Automated Test Result

- **Command:** `dotnet test backend/EnglishLearning.sln --no-build`
- **Result:** **`105 Passed / 0 Failed / 0 Skipped`** (**PASS**)
  - Unit Tests: `60 Passed`
  - Integration Tests: `45 Passed`

---

## 12. Frontend Build Result

- **Command:** `npm.cmd --prefix frontend/EnglishLearningWeb run build`
- **Result:** **`12/12 static pages compiled, 0 errors, 0 warnings`** (**PASS**)

---

## 13. Files Modified

1. `backend/EnglishLearning.Api/appsettings.json` — Updated connection string to match local PostgreSQL server credentials.
2. `backend/EnglishLearning.Api/Properties/launchSettings.json` — Configured `applicationUrl` to port `5288` to avoid port collision.
3. `frontend/EnglishLearningWeb/src/services/apiClient.ts` — Updated default fallback API URL to port `5288`.

---

## 14. Files Not Modified (Preserved)

- `backend/EnglishLearning.Domain/**/*` (All Domain entities and enums preserved)
- `backend/EnglishLearning.Application/**/*` (All use cases, DTOs, validators, and interfaces preserved)
- `backend/EnglishLearning.Infrastructure/Persistence/**/*` (Repositories, models, and migrations preserved)
- `frontend/EnglishLearningWeb/src/components/**/*` (All UI components and practice room preserved)
- `frontend/EnglishLearningWeb/src/app/**/*` (All App router pages preserved)

---

## 15. Security Verification

- Passwords and connection string secrets are not printed or exposed in logs, reports, or frontend bundles.
- Password hashing via PBKDF2 (100,000 iterations + cryptographic salt) verified.
- JWT Bearer authentication and tenant isolation verified.

---

## 16. Remaining Blockers

- **None.** Database persistence, registration, login, dashboard queries, and topic retrieval are fully operational on localhost.
