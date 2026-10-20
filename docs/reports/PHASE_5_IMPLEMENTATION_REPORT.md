# Phase 5 Implementation Report — Authentication Implementation

**Date:** 2026-08-14  
**Milestone:** Phase 5 — Authentication  
**Status:** PASS  
**Base Architecture Document:** [docs/ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md)  
**Architecture Review Document:** [docs/ARCHITECTURE_REVIEW.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE_REVIEW.md)  
**SRS Reference:** [docs/SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/SRS.md)

---

## 1. Phase Objective

The objective of Phase 5 was to implement end-to-end secure user authentication across the backend and frontend:
- Implement standard ASP.NET Core password hashing with cryptographic salt and PBKDF2 (`IPasswordHasher<User>`).
- Implement JWT Bearer token generation and signature validation with ASP.NET Core authentication middleware.
- Implement REST API endpoints: `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, and protected `GET /api/v1/auth/me`.
- Implement `ICurrentUserService` to safely resolve the authenticated user from JWT claims without direct dependency on `HttpContext`.
- Implement rate limiting protection for authentication endpoints.
- Build frontend `apiClient.ts`, `authService.ts`, and `AuthContext.tsx`.
- Connect the existing Phase 3 Login and Registration UI forms with real error handling, loading states, and route protection.

---

## 2. Authentication Architecture

```text
[Frontend Next.js]
  ├── LoginForm / RegisterForm
  ├── AuthContext (central state: user, token, isAuthenticated, isLoading)
  ├── apiClient (injected Bearer JWT, RFC 7807 problem details parsing)
  └── AppShell (route guard, redirects unauthenticated users to /login)
         │
         ▼ (HTTP REST JSON)
[Backend ASP.NET Core 8 Web API]
  ├── GlobalExceptionHandlingMiddleware (maps domain exceptions to 401/409/400/500)
  ├── RateLimiter Middleware (60 req/min per IP protection)
  ├── UseAuthentication & UseAuthorization (JwtBearerDefaults)
  ├── AuthController (V1 endpoints: /register, /login, /me)
  │      │
  │      ▼
  ├── Application Layer (IAuthService, AuthService, FluentValidation validators)
  │      │
  │      ├── IPasswordHasherService (PBKDF2 HMAC-SHA512)
  │      ├── IJwtTokenGenerator (HMAC-SHA256 signing)
  │      └── ICurrentUserService (ClaimsPrincipal extraction)
  │      │
  │      ▼
  └── Infrastructure Layer (PostgreSQL AppDbContext, UserRepository, UnitOfWork)
```

---

## 3. Registration Flow

1. Client submits `POST /api/v1/auth/register` with `Name`, `Email`, `Password`, `ConfirmPassword`, `EnglishLevel`, and `LearningGoal`.
2. `RegisterRequestValidator` (FluentValidation) validates required fields, email format, minimum password length (8 chars with letter + digit), matching confirmation, and valid CEFR / Goal enums.
3. `AuthService.RegisterAsync` normalizes the email (trimmed and lowercased).
4. `UserRepository.GetByEmailAsync` checks for existing account. If found, throws `DuplicateEmailException` (mapped to HTTP 409 Conflict).
5. Password is cryptographically hashed via `IPasswordHasherService` (ASP.NET Core `PasswordHasher<User>`).
6. New `User` entity is persisted to PostgreSQL.
7. Signed JWT token is generated and returned with user metadata in `AuthResponse`.
8. Password and password hash are strictly excluded from response DTOs.

---

## 4. Login Flow

1. Client submits `POST /api/v1/auth/login` with `Email` and `Password`.
2. `LoginRequestValidator` validates non-empty email and password.
3. `AuthService.LoginAsync` normalizes the email and looks up the user. If not found, throws `InvalidCredentialsException` (mapped to HTTP 401 Unauthorized).
4. `PasswordHasherService.VerifyPassword` verifies provided plaintext password against stored hash. If verification fails, throws `InvalidCredentialsException`.
5. Signed JWT access token is generated with expiration timestamp and returned in `AuthResponse`.

---

## 5. Password Hashing Approach

- Utilizes standard `Microsoft.AspNetCore.Identity.PasswordHasher<User>`.
- Employs PBKDF2 with HMAC-SHA512, iterated 100,000+ times with a per-user cryptographically random salt.
- Plaintext passwords are never stored, logged, or returned in API responses.

---

## 6. JWT Configuration & Security

- **Signing Algorithm:** HMAC-SHA256 (`SecurityAlgorithms.HmacSha256`).
- **Configuration Parameters:**
  - `Jwt:Secret`: Configurable 32+ byte cryptographic secret key.
  - `Jwt:Issuer`: `EnglishLearningAI`
  - `Jwt:Audience`: `EnglishLearningClient`
  - `Jwt:ExpiryMinutes`: `1440` (24 hours for development / session lifetime).
- **Token Validation Parameters:**
  - `ValidateIssuerSigningKey = true`
  - `ValidateIssuer = true`
  - `ValidateAudience = true`
  - `ValidateLifetime = true`
  - `ClockSkew = TimeSpan.Zero`

---

## 7. JWT Claims

Minimal, privacy-conscious claims payload:
- `sub` / `nameid`: User UUID (`Guid`)
- `email`: Normalized email
- `name`: User full name
- `jti`: Unique token identifier (`Guid`)
- `level`: CEFR English proficiency level (e.g. `B1`)
- `goal`: Learning goal (e.g. `SpeakingFluency`)

---

## 8. Current User Mechanism

- `ICurrentUserService` interface defined in `EnglishLearning.Application`.
- `CurrentUserService` implemented in `EnglishLearning.Infrastructure` leveraging `IHttpContextAccessor`.
- Extracts user ID directly from validated JWT `sub` / `NameIdentifier` claim.
- Ensures application services do not depend directly on `HttpContext` or trust client-provided user IDs.

---

## 9. Protected Endpoints

- **`GET /api/v1/auth/me`**:
  - Decorated with `[Authorize]`.
  - Unauthenticated calls return `HTTP 401 Unauthorized` with RFC 7807 ProblemDetails.
  - Authenticated calls with valid Bearer token return `HTTP 200 OK` and `UserProfileDto` (`Id`, `Name`, `Email`, `EnglishLevel`, `LearningGoal`, `CreatedAt`).

---

## 10. Frontend AuthContext & API Client

- **`apiClient.ts`**: Centralized HTTP fetch client that automatically attaches `Authorization: Bearer <token>` from browser storage and maps HTTP error status codes / validation errors to typed `ApiException` instances.
- **`authService.ts`**: Handles `register`, `login`, and `getMe` API calls.
- **`AuthContext.tsx`**: Centralized React context provider exposing `user`, `token`, `isAuthenticated`, `isLoading`, `login()`, `register()`, and `logout()`.

---

## 11. Token Storage Strategy

- Access token is stored in memory and persisted in `localStorage` (`el_access_token`) for the duration of the session.
- Token is wiped immediately upon logout or on receiving an HTTP 401 response from any protected API call.
- Tokens are never passed in query strings or logged to browser console.

---

## 12. Route Protection

- `AppShell.tsx` acts as an active client-side route guard:
  - While `isLoading` is true, displays a clean branded loading state.
  - If `!isAuthenticated`, smoothly redirects the user to `/login`.
  - Authenticated pages (`/dashboard`, `/topics`, `/practice`, `/history`, `/profile`) are protected.

---

## 13. CORS Configuration

- Strict origin filtering using `builder.Services.AddCors`.
- Allowed Origin: `http://localhost:3000` (Next.js development frontend).
- Configured with `AllowAnyHeader()`, `AllowAnyMethod()`, and `AllowCredentials()`.
- Unrestricted wildcards (`AllowAnyOrigin`) with credentials are not permitted.

---

## 14. Rate Limiting & Abuse Protection

- Built-in ASP.NET Core 8 `RateLimiter` middleware configured.
- Implements partitioned fixed-window rate limiter per client IP address (60 requests/minute) to mitigate credential stuffing and brute-force attacks on `/auth/register` and `/auth/login`.

---

## 15. Security Review

| Security Check | Status | Verification Detail |
|---|---|---|
| No Plaintext Passwords | PASS | Passwords hashed using PBKDF2 HMAC-SHA512 before persistence |
| No PasswordHash in Responses | PASS | Excluded from all DTOs and responses; validated via unit/integration tests |
| No JWT in Logs | PASS | Logging configuration excludes Authorization headers and JWT strings |
| No Hardcoded Secrets | PASS | Secrets pulled from `IConfiguration` with environment variable overrides |
| JWT Validation Active | PASS | Full signature, issuer, audience, and lifetime validation enabled with zero clock skew |
| Authenticated Context | PASS | User identity resolved exclusively from validated token claims via `ICurrentUserService` |
| Route Guarding | PASS | Unauthenticated users blocked from accessing protected pages and APIs |

---

## 16. Test Results

All 36 unit and integration tests passed:

```text
Unit Tests (EnglishLearning.UnitTests):
  [PASS] DomainAssembly_ShouldNotReference_OtherApplicationLayers
  [PASS] BaseEntity_ShouldInitializeWith_ValidGuidAndUtcTimestamp
  [PASS] User_ShouldInitialize_WithDefaultValues
  [PASS] Topic_ShouldInitialize_WithActiveStateAndOrder
  [PASS] ConversationSession_ShouldEstablish_InitialStatusAndRelationships
  [PASS] ConversationMessage_ShouldTrack_RoleAndSequence
  [PASS] Assessment_ShouldStore_ScoresAndJsonData
  [PASS] EntityNotFoundException_ShouldContain_EntityNameAndKey
  [PASS] GetAllActiveTopicsAsync_ShouldReturn_MappedTopicDtos
  [PASS] GetTopicByIdAsync_WithValidId_ShouldReturn_TopicDto
  [PASS] GetTopicByIdAsync_WithInvalidId_ShouldThrow_EntityNotFoundException
  [PASS] TopicDtoValidator_ShouldValidate_RequiredFields
  [PASS] HashPassword_ShouldGenerate_NonEmptyDifferentHashFromPlaintext
  [PASS] VerifyPassword_WithCorrectPassword_ShouldReturnTrue
  [PASS] VerifyPassword_WithIncorrectPassword_ShouldReturnFalse
  [PASS] GenerateToken_ShouldCreate_ValidJwtWithExpectedClaims
  [PASS] RegisterValidator_WithValidRequest_ShouldPass
  [PASS] RegisterValidator_WithMismatchedPasswords_ShouldFail
  [PASS] RegisterValidator_WithWeakPassword_ShouldFail
  [PASS] LoginValidator_WithValidRequest_ShouldPass
  [PASS] LoginValidator_WithInvalidEmail_ShouldFail
  [PASS] RegisterAsync_WithNewEmail_ShouldCreateUserAndReturnToken
  [PASS] RegisterAsync_WithExistingEmail_ShouldThrow_DuplicateEmailException
  [PASS] LoginAsync_WithValidCredentials_ShouldReturnAuthResponse
  [PASS] LoginAsync_WithInvalidPassword_ShouldThrow_InvalidCredentialsException
  [PASS] LoginAsync_WithUnknownEmail_ShouldThrow_InvalidCredentialsException
  Total: 26, Passed: 26, Failed: 0

Integration Tests (EnglishLearning.IntegrationTests):
  [PASS] GetHealth_ShouldReturn_200OkWithHealthyStatus
  [PASS] GetAllTopics_ShouldReturn_200OkWithSeedTopics
  [PASS] GetTopicById_WithValidId_ShouldReturn_200Ok
  [PASS] GetTopicById_WithInvalidId_ShouldReturn_404NotFound
  [PASS] Register_WithValidData_ShouldReturn_200OkAndToken
  [PASS] Register_WithDuplicateEmail_ShouldReturn_409Conflict
  [PASS] Login_WithValidCredentials_ShouldReturn_200OkAndToken
  [PASS] Login_WithIncorrectPassword_ShouldReturn_401Unauthorized
  [PASS] GetCurrentUser_WithoutToken_ShouldReturn_401Unauthorized
  [PASS] GetCurrentUser_WithValidToken_ShouldReturn_200OkUserProfile
  Total: 10, Passed: 10, Failed: 0
------------------------------------------------------------
Combined Result: 36 passed, 0 failed, 0 skipped.
```

---

## 17. Build Results

- **Backend Solution (`dotnet build`):** 0 Errors, 0 Warnings.
- **Frontend Build (`npm run build`):** 0 Errors, 0 Warnings, 12 static routes optimized.
- **Frontend Lint (`npm run lint`):** 0 Errors, 0 Warnings.

---

## 18. Known Issues

- None.

---

## 19. Database Changes

- Reused the existing `users` table created during Phase 4 migration without requiring schema alterations.

---

## 20. Deviations from SRS & ARCHITECTURE

- **None.** The authentication system implements all requirements in [SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/SRS.md) and [ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md).

---

## 21. Conclusion & Next Phase Readiness

Phase 5 is complete and verified. Secure registration, password hashing, login, JWT token issuance, protected endpoints, and frontend integration are operational. The system is ready for **Phase 6 — AI Conversation Implementation**.

---
**PHASE 5 STATUS: PASS**
