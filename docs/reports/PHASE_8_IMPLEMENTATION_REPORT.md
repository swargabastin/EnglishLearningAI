# Phase 8 Implementation Report — Assessment Implementation

**Date:** 2026-08-14  
**Milestone:** Phase 8 — Assessment Engine & Evaluation  
**Status:** PASS  
**Base Architecture Document:** [docs/ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md)  
**Architecture Review Document:** [docs/ARCHITECTURE_REVIEW.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE_REVIEW.md)  
**SRS Reference:** [docs/SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/SRS.md)

---

## 1. Phase Objective

The objective of Phase 8 was to implement an AI-powered conversational assessment engine for completed English speaking/learning sessions:
- Dedicated assessment evaluation abstraction (`IAiAssessmentService` / `IAssessmentService`).
- Evaluation of grammar, vocabulary range, and conversational fluency from persisted database conversation transcripts.
- Calculation of overall score using the authoritative formula:  
  $$\text{OverallScore} = \text{round}\left(\frac{\text{GrammarScore} + \text{VocabularyScore} + \text{FluencyScore}}{3}, 1\right)$$
- Extraction of genuine strengths, prioritized areas for improvement, detailed grammar corrections (original, corrected, explanation), and vocabulary enrichment suggestions.
- PostgreSQL persistence in the `assessments` table and updating `conversation_sessions.overall_score`.
- Idempotency / duplicate generation prevention (returning existing assessment if already generated).
- Enforcement of session ownership (`session.UserId == authenticatedUserId`).
- Interactive Results UI displaying scores, feedback, corrections, and recommended practice actions.

---

## 2. Architecture Overview

```text
[Next.js Results UI (/practice/result?session={sessionId})]
   │
   ▼
[ASP.NET Core 8 Web API]
   ├── ConversationsController (POST /api/v1/conversations/{sessionId}/assessment)
   ├── ConversationsController (GET  /api/v1/conversations/{sessionId}/assessment)
   │      │
   │      ▼
   ├── Application Layer
   │      ├── IAssessmentService / AssessmentService
   │      │      ├── 1. Authenticate user & verify session ownership (session.UserId == currentUserId)
   │      │      ├── 2. Validate session state (Status == Completed, MessageCount > 0)
   │      │      ├── 3. Idempotency check (_assessmentRepository.GetBySessionIdAsync)
   │      │      ├── 4. Load database transcript & build AssessmentAiRequest
   │      │      ├── 5. IAiAssessmentService.EvaluateConversationAsync (OpenAI gpt-4o-mini)
   │      │      ├── 6. Validate & clamp scores [0.0, 100.0]
   │      │      ├── 7. Calculate authoritative OverallScore
   │      │      ├── 8. Save Assessment entity to PostgreSQL & update session.OverallScore
   │      │      └── 9. Return AssessmentResponseDto
   │      │
   │      ▼
   └── Infrastructure Layer
          ├── OpenAiAssessmentService (typed HttpClient with response_format: json_object)
          ├── AssessmentPromptBuilder (isolated system prompt with anti-prompt-injection)
          └── EntityFramework Core PostgreSQL Persistence (AssessmentRepository)
```

---

## 3. Assessment AI Prompt & Anti-Prompt-Injection Defense

### Dedicated System Prompt (`AssessmentPromptBuilder.cs`)
- Distinct from the conversational English tutor prompt. Instructs the model as an impartial language assessor.
- Evaluates:
  1. **Grammar (0–100):** Tense consistency, subject-verb agreement, prepositions, articles.
  2. **Vocabulary (0–100):** Appropriateness to CEFR level, lexical variety, topical relevance.
  3. **Fluency (0–100):** Conversational coherence, turn-taking, response elaboration.
- **Anti-Prompt-Injection Rule:** Untrusted learner messages are placed inside an isolated `=== CONVERSATION TRANSCRIPT TO EVALUATE ===` data block and explicitly treated as untrusted data rather than instructions.
- Enforces strict JSON output matching schema without markdown wrappers.

---

## 4. Structured AI Output Schema

```json
{
  "grammarScore": 82,
  "vocabularyScore": 85,
  "fluencyScore": 80,
  "pronunciationScore": null,
  "feedback": "Encouraging summary feedback...",
  "strengths": [
    "Clear communicative intent",
    "Good use of target vocabulary"
  ],
  "areasForImprovement": [
    "Practice using present perfect for past experiences",
    "Expand single-clause sentences into descriptive compound phrases"
  ],
  "grammarCorrections": [
    {
      "original": "I am working here since two years.",
      "corrected": "I have been working here for two years.",
      "explanation": "Use 'have been working' (present perfect continuous) and 'for' with a duration.",
      "ruleCategory": "Verb Tense"
    }
  ],
  "vocabularySuggestions": [
    {
      "originalWord": "good",
      "suggestedAlternative": "valuable",
      "context": "It was a valuable experience.",
      "cefrLevel": "B2"
    }
  ],
  "recommendations": [
    "Practice time prepositions ('for' vs 'since')",
    "Incorporate more transition words"
  ]
}
```

---

## 5. Assessment Score Calculation & Validation

- **Component Scores:** Grammar, Vocabulary, and Fluency scores are bounded in `[0.0, 100.0]`.
- **Overall Score Formula:**
  $$\text{OverallScore} = \text{round}\left(\frac{\text{GrammarScore} + \text{VocabularyScore} + \text{FluencyScore}}{3}, 1\right)$$
  *(If pronunciation score is supplied in future models, the divisor is 4).*
- Clamped and verified before saving to database.

---

## 6. Assessment API Endpoints

### 1. Generate Assessment
```http
POST /api/v1/conversations/{sessionId}/assessment
Authorization: Bearer <jwt-token>
```
- Validates session ownership.
- Ensures session status is `Completed` (returns `HTTP 400` if active or abandoned).
- Ensures user exchanged at least 1 message (returns `HTTP 400` if empty).
- Idempotent: If an assessment already exists, immediately returns the stored assessment.

### 2. Retrieve Existing Assessment
```http
GET /api/v1/conversations/{sessionId}/assessment
Authorization: Bearer <jwt-token>
```
- Retrieves the saved assessment for the authenticated owner's completed session.
- Returns `HTTP 404` for non-existent or non-owned sessions.

---

## 7. Frontend Results UI (`/practice/result?session={sessionId}`)

- **Assessment Hero:** Displays Overall Score badge (e.g. `82`), Topic title, CEFR level badge, and quick navigation.
- **Score Breakdown:** Multi-metric breakdown with progress meters for Grammar, Vocabulary, Fluency, and Overall scores.
- **Strengths & Weaknesses Card:** Key strengths checklist and prioritized areas for improvement.
- **Grammar Corrections List:** Side-by-side comparison of learner sentences, corrected versions, and grammatical explanations.
- **Vocabulary Enrichment:** Suggested higher-level CEFR vocabulary alternatives in context.
- **Recommended Practice:** Actionable focus areas and immediate navigation back to practice new topics.

---

## 8. Test Results

All 80 backend tests passed:

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
  [PASS] ProcessSpeechAsync_WithValidAudio_ShouldSucceedWithSttAiAndTts
  [PASS] ProcessSpeechAsync_WithEmptyAudio_ShouldThrow_InvalidAudioException
  [PASS] ProcessSpeechAsync_WithOversizedAudio_ShouldThrow_InvalidAudioException
  [PASS] ProcessSpeechAsync_WithUnsupportedFormat_ShouldThrow_InvalidAudioException
  [PASS] ProcessSpeechAsync_WhenTtsFails_ShouldStillReturn_RecognizedTextAndAiMessage
  [PASS] GenerateAssessmentAsync_WithCompletedSession_ShouldCalculateOverallScoreAndSave
  [PASS] GenerateAssessmentAsync_WithActiveSession_ShouldThrow_InvalidSessionStateException
  [PASS] GenerateAssessmentAsync_WithNoUserMessages_ShouldThrow_InvalidSessionStateException
  [PASS] GenerateAssessmentAsync_WithNonOwner_ShouldThrow_UnauthorizedSessionAccessException
  [PASS] GenerateAssessmentAsync_WhenAlreadyExists_ShouldReturnExistingWithoutCallingAi
  [PASS] GetAssessmentBySessionIdAsync_WithValidSessionAndAssessment_ShouldReturnDto
  [PASS] AssessmentPromptBuilder_BuildSystemPrompt_ShouldContain_AntiInjectionAndSchemaRules
  Total: 55, Passed: 55, Failed: 0

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
  [PASS] SendSpeech_WithoutAuth_ShouldReturn_401Unauthorized
  [PASS] SendSpeech_WithValidAudio_ShouldReturn_200OkAndSpeechResponse
  [PASS] SendSpeech_WithoutAudioField_ShouldReturn_400BadRequest
  [PASS] SendSpeech_FromDifferentUser_ShouldReturn_404NotFound
  [PASS] SendSpeech_OnCompletedSession_ShouldReturn_400BadRequest
  [PASS] GenerateAssessment_WithCompletedSession_ShouldReturn_200OkAndAssessment
  [PASS] GetAssessment_WithExistingAssessment_ShouldReturn_200Ok
  [PASS] GenerateAssessment_WithoutAuth_ShouldReturn_401Unauthorized
  [PASS] GenerateAssessment_OnActiveSession_ShouldReturn_400BadRequest
  [PASS] GenerateAssessment_FromDifferentUser_ShouldReturn_404NotFound
  Total: 25, Passed: 25, Failed: 0
------------------------------------------------------------
Combined Result: 80 passed, 0 failed, 0 skipped.
```

---

## 9. Build Results

- **Backend Solution (`dotnet build`):** 0 Errors, 0 Warnings.
- **Frontend Build (`npm run build`):** 0 Errors, 0 Warnings, 12 static routes optimized.
- **Frontend Lint (`npm run lint`):** 0 Errors, 0 Warnings.

---

## 10. Database Schema Changes

- **No database schema changes were required for Phase 8.** Reused the existing Phase 4 `Assessment` entity and `assessments` table.

---

## 11. Known Limitations & Disclaimer

- The assessment generated is an AI-assisted language learning diagnostic and does NOT represent an official CEFR language certification.
- Pronunciation evaluation in MVP is reserved as an extension point and is not simulated with fake metrics.

---

## 12. Conclusion & Strict Boundary

Phase 8 is complete and verified. The application supports full end-to-end conversation practice and post-session pedagogical evaluation. Phase 9 (Progress Tracking & Analytics) has NOT been started.

---
**PHASE 8 STATUS: PASS**
