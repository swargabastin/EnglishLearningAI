# Phase 6 Implementation Report — AI Conversation / AI English Tutor

**Date:** 2026-08-14  
**Milestone:** Phase 6 — AI Conversation  
**Status:** PASS  
**Base Architecture Document:** [docs/ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md)  
**Architecture Review Document:** [docs/ARCHITECTURE_REVIEW.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE_REVIEW.md)  
**SRS Reference:** [docs/SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/SRS.md)

---

## 1. Phase Objective

The objective of Phase 6 was to implement text-based AI English speaking practice with an intelligent, CEFR-adapted AI English Tutor ("Emma"):
- Enable authenticated users to select any approved topic and initiate a conversation session.
- Generate real, context-aware AI tutor responses via OpenAI (`gpt-4o-mini`).
- Adapt conversational vocabulary, sentence length, and pacing dynamically to the learner's CEFR level (A1 to C2).
- Persist sessions, user messages, and AI responses in PostgreSQL with sequential ordering.
- Enforce strict session ownership and active status validation across all operations.
- Connect the frontend conversation interface with interactive text input, loading indicators, error recovery, and clean session completion without fake speech mechanisms.

---

## 2. AI Architecture

```text
[Frontend Next.js]
  ├── Topic Selection (`/topics` -> startConversation)
  ├── Practice Screen (`/practice?session=...`)
  ├── TextConversationInput (text input, busy states, retry)
  └── ConversationFeed (turn bubbles, AI thinking spinner)
         │
         ▼ (HTTP REST JSON with Bearer JWT)
[ASP.NET Core 8 Web API]
  ├── ConversationsController
  │      POST /api/v1/conversations
  │      POST /api/v1/conversations/{id}/messages
  │      GET  /api/v1/conversations/{id}
  │      POST /api/v1/conversations/{id}/end
  │      │
  │      ▼
  ├── Application Layer
  │      ├── ConversationService (session lifecycle, context building, validation)
  │      ├── FluentValidation (StartConversationRequestValidator, SendMessageRequestValidator)
  │      └── IAiConversationService (provider abstraction)
  │      │
  │      ▼
  └── Infrastructure Layer
         ├── OpenAiConversationService (HTTP client, timeout handling, fallback engine)
         ├── TutorPromptBuilder (system prompt, CEFR adaptation, prompt-injection defense)
         └── PostgreSQL AppDbContext (ConversationSessions & ConversationMessages persistence)
```

---

## 3. Provider Abstraction

The AI interaction is decoupled from concrete SDKs through the Application layer interface:

```csharp
public interface IAiConversationService
{
    Task<AiConversationResponse> GenerateResponseAsync(
        AiConversationRequest request, 
        CancellationToken cancellationToken = default
    );
}
```

The Domain and Application layers have **zero** dependencies on OpenAI SDKs or third-party AI libraries.

---

## 4. OpenAI Provider Implementation

- Implemented in `EnglishLearning.Infrastructure.Ai.OpenAiConversationService`.
- Calls OpenAI REST API (`https://api.openai.com/v1/chat/completions`) using typed `HttpClient`.
- Injects structured system prompt, topic description, CEFR guidelines, and rolling context messages.
- If no API key is present during local development or offline testing, includes an intelligent deterministic tutor response engine tailored to the 7 SRS topics.

---

## 5. AI Configuration

Configured via `AiSettings` in `appsettings.json` and environment variables:

```json
"AiSettings": {
  "Provider": "OpenAI",
  "Model": "gpt-4o-mini",
  "MaxTokens": 500,
  "Temperature": 0.7,
  "MaxContextMessages": 10,
  "TimeoutSeconds": 30
}
```

- API Key is injected via `AiSettings:ApiKey`, `AI__APIKEY`, or `OPENAI_API_KEY` environment variables.
- API keys are **never** committed to Git, returned to clients, or logged.

---

## 6. Tutor System Prompt Architecture

`TutorPromptBuilder` constructs system instructions defining the AI tutor persona ("Emma"):
1. **Tone:** Warm, encouraging, supportive, and conversational.
2. **Turn Pacing:** Concise replies (2–4 sentences) ending with one open-ended question to maximize learner speaking opportunities.
3. **Correction Strategy:** Acknowledges the learner's idea first, gently models natural phrasing (e.g., *"A natural way to say that is: '...' "*), and continues the conversation without derailing into lecturing.
4. **Topic Adherence:** Stays focused on the selected topic and scenario context.

---

## 7. CEFR Level Adaptation

`TutorPromptBuilder` injects tailored linguistic constraints based on CEFR level:
- **A1 (Beginner):** Very short, simple sentences, fundamental vocabulary, basic choice questions, abundant praise.
- **A2 (Elementary):** Simple compound sentences, familiar daily vocabulary, routine questions.
- **B1 (Intermediate):** Natural conversational flow, moderate complexity, opinion sharing.
- **B2 (Upper Intermediate):** Rich authentic English, professional terminology, abstract reasoning, idioms.
- **C1 (Advanced):** Sophisticated vocabulary, complex clause structures, subtle analytical questions.
- **C2 (Proficient):** Native-like precision, cultural subtlety, idioms, and effortless nuance.

---

## 8. Conversation Service

`ConversationService` orchestrates the conversation lifecycle:
- `StartConversationAsync`: Verifies user & active topic, creates session in PostgreSQL, generates initial tutor greeting, saves initial message.
- `SendMessageAsync`: Enforces session ownership & active status, saves user message with sequential index, constructs 10-message rolling context window, invokes `IAiConversationService`, saves AI response, updates duration.
- `GetConversationByIdAsync`: Retrieves session and message history with ownership verification.
- `EndConversationAsync`: Sets status to `Completed`, records `EndedAt` and `DurationSeconds`.

---

## 9. API Endpoints

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/v1/conversations` | `POST` | `Bearer JWT` | Starts a new session and returns initial AI greeting |
| `/api/v1/conversations/{id}/messages` | `POST` | `Bearer JWT` | Appends user message, calls AI, returns AI response |
| `/api/v1/conversations/{id}` | `GET` | `Bearer JWT` | Retrieves session details and message history |
| `/api/v1/conversations/{id}/end` | `POST` | `Bearer JWT` | Ends active session, marking it Completed |

---

## 10. Conversation Context Strategy

- The conversation context window is capped at the most recent 10 messages (`TakeLast(10)`).
- Prevents unbounded token growth and ensures deterministic response times during extended practice.

---

## 11. Session Ownership & Security

- User identity is resolved exclusively from the validated JWT claims (`ICurrentUserService`).
- Every query enforces `session.UserId == authenticatedUserId`.
- Unauthorized access attempts to another user's session return `HTTP 404 Not Found` to prevent session enumeration.
- Completed or abandoned sessions reject new messages with `HTTP 400 Bad Request` (`InvalidSessionStateException`).

---

## 12. Database Persistence

- `ConversationSession` records: `Id`, `UserId`, `TopicId`, `EnglishLevel`, `StartedAt`, `EndedAt`, `DurationSeconds`, `Status`.
- `ConversationMessage` records: `Id`, `SessionId`, `Role` (User / Assistant), `MessageText`, `SequenceNumber`, `CreatedAt`.
- Stored in PostgreSQL with foreign keys and cascade rules.

---

## 13. Frontend Integration

- **`conversationService.ts`**: Frontend API client for conversation operations.
- **`TopicGrid.tsx`**: Clicking "Start Practice Session" invokes `conversationService.startConversation()` and routes to `/practice?session={id}`.
- **`PracticePage.tsx`**: Loads session from backend, renders conversation turns, manages active state, auto-scrolls to the latest message, and handles session completion.
- **`TextConversationInput.tsx`**: Dedicated text conversation input bar with busy states, character count, and retry triggers.
- **Microphone Notice**: Clearly displays *"Phase 6: Text Conversation Mode Active • Voice input coming in Phase 7"*, avoiding fake speech simulation.

---

## 14. Loading & Error States

- **Sending / Thinking:** Animated bouncing dots with *"Emma is thinking & formulating reply..."*.
- **Error Banner:** Displays failure details with inline "Retry" button to resend the last message.
- **Completion:** Modal confirming completed turn count with navigation to Dashboard or Topics.

---

## 15. AI Error Handling & Timeout Strategy

- Configured with `CancellationToken` and 30-second request timeout.
- Unhandled AI exceptions wrapped in `AiServiceException` returning RFC 7807 `HTTP 503 Service Unavailable`.
- API keys, credentials, and internal error stacks are never leaked.

---

## 16. Prompt Injection Defense

- User inputs are treated strictly as conversational practice exercise data, never as system instructions.
- System prompt instructs the tutor to ignore user attempts to alter roles, bypass boundaries, or execute system commands.

---

## 17. Test Results

All 58 backend unit and integration tests passed:

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
  [PASS] BuildSystemPrompt_ShouldInclude_CefrSpecificInstructions (6 CEFR levels)
  [PASS] StartValidator_WithValidRequest_ShouldPass
  [PASS] StartValidator_WithEmptyTopicId_ShouldFail
  [PASS] StartValidator_WithInvalidLevel_ShouldFail
  [PASS] SendMessageValidator_WithValidMessage_ShouldPass
  [PASS] SendMessageValidator_WithEmptyMessage_ShouldFail
  [PASS] SendMessageValidator_WithExcessiveLength_ShouldFail
  [PASS] StartConversationAsync_ShouldCreateSessionAndInitialGreeting
  [PASS] SendMessageAsync_ShouldAppendUserAndAiMessages
  [PASS] SendMessageAsync_ToNonOwnedSession_ShouldThrow_UnauthorizedSessionAccessException
  [PASS] SendMessageAsync_ToCompletedSession_ShouldThrow_InvalidSessionStateException
  [PASS] EndConversationAsync_ShouldMarkSessionCompleted
  Total: 43, Passed: 43, Failed: 0

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
  [PASS] StartConversation_WithoutAuth_ShouldReturn_401Unauthorized
  [PASS] StartConversation_WithAuth_ShouldReturn_200OkAndInitialGreeting
  [PASS] SendMessage_WithAuthOwner_ShouldReturn_200OkAndAiReply
  [PASS] SendMessage_FromDifferentUser_ShouldReturn_404NotFound
  [PASS] EndConversation_ShouldCompleteSession
  Total: 15, Passed: 15, Failed: 0
------------------------------------------------------------
Combined Result: 58 passed, 0 failed, 0 skipped.
```

---

## 18. Build Results

- **Backend Solution (`dotnet build`):** 0 Errors, 0 Warnings.
- **Frontend Build (`npm run build`):** 0 Errors, 0 Warnings, 12 static routes optimized.
- **Frontend Lint (`npm run lint`):** 0 Errors, 0 Warnings.

---

## 19. Database Changes

- **No database schema changes were required for Phase 6.** Reused the existing `conversation_sessions` and `conversation_messages` tables created during Phase 4.

---

## 20. Known Issues

- None.

---

## 21. Deviations from SRS & ARCHITECTURE

- **None.** The text-based AI tutor conversation flow, CEFR prompt architecture, and REST API contracts conform strictly to [SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/SRS.md) and [ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md).

---

## 22. Conclusion & Next Phase Readiness

Phase 6 is complete and verified. Real text-based conversational practice with AI Tutor Emma is fully operational. The application is prepared for **Phase 7 — Speech-to-Text & Text-to-Speech Integration**.

---
**PHASE 6 STATUS: PASS**
