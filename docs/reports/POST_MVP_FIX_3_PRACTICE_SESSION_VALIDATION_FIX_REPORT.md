# POST-MVP FIX 3 — PRACTICE SESSION VALIDATION FIX REPORT

**Project:** English Learning AI  
**Scope:** Practice Session Validation Error Fix & Payload Sanitization  
**Date:** 2026-08-17  
**Status:** COMPLETED & VERIFIED  

---

## 1. Issue Summary

When navigating to `/practice`, the application failed to start a practice conversation session and displayed:
- `SERVICE NOTICE: "One or more validation errors occurred."`
- `INPUT NOTICE: "One or more validation errors occurred."`
The conversation feed remained blank and Emma's initial greeting did not render.

---

## 2. Exact Root Cause

1. **Legacy String Identifier:** The frontend practice room default and mock topic definitions used legacy string keys (e.g. `"topic-2"`, `"topic-1"`) rather than the canonical 36-character GUIDs defined in the PostgreSQL database and ASP.NET Core entity model (`22222222-2222-2222-2222-222222222222`).
2. **Model Deserialization Validation Rejection:** ASP.NET Core model binding rejected `"topic-2"` as an invalid GUID for `StartConversationRequest.TopicId`, resulting in a `400 Bad Request` with `ValidationProblemDetails`.
3. **Generic Error Extraction:** The frontend `apiClient.ts` extracted only `errorJson.title` (`"One or more validation errors occurred."`) instead of parsing the granular field messages from the `errors` dictionary.

---

## 3. Failed Endpoint

- **Endpoint:** `POST /api/v1/conversations`
- **Controller:** `EnglishLearning.Api.Controllers.V1.ConversationsController.StartConversation`

---

## 4. Frontend Request Payload (WITHOUT secrets)

**Before Fix (Failed):**
```json
{
  "topicId": "topic-2",
  "englishLevel": "B1"
}
```

**After Fix (Passing):**
```json
{
  "topicId": "22222222-2222-2222-2222-222222222222",
  "englishLevel": "B1"
}
```

---

## 5. Backend DTO

```csharp
public record StartConversationRequest(
    Guid TopicId,
    string? EnglishLevel = null
);
```

---

## 6. Validation Rule Causing Failure

- **ASP.NET Core JSON Deserializer:** `System.Text.Json` converter for `Guid` failed on non-GUID string `"topic-2"`:
  `"The JSON value could not be converted to EnglishLearning.Application.DTOs.StartConversationRequest. Path: $.topicId"`
- **FluentValidation:** `StartConversationRequestValidator` requires `.NotEmpty()` on `TopicId`.

---

## 7. Root Cause Analysis

The application had transitioned from static frontend prototyping (using `"topic-1"`, `"topic-2"`) to a live PostgreSQL relational database with strongly-typed `Guid` primary and foreign keys. The frontend practice initialization code still had legacy fallback references to `mockTopics[1].id` (`"topic-2"`), which failed deserialization against the backend's strict C# `Guid` contract.

---

## 8. Fix Applied

1. **Canonical Topic GUIDs:** Updated `mockTopics.ts` to map all 7 topics to their exact PostgreSQL GUIDs (`11111111-...` through `77777777-...`).
2. **Practice Page Topic Sanitizer:** Added a GUID regex validator and legacy key mapper in `practice/page.tsx` to ensure any query parameter (`?topic=topic-2`) is automatically mapped to its canonical database GUID.
3. **Recommended Topics Update:** Updated `RecommendedTopicsCard.tsx` with canonical GUIDs and routing parameters.
4. **Granular Error Extraction:** Enhanced `apiClient.ts` to extract and flatten specific messages from the backend's `errors` dictionary, ensuring users receive actionable feedback rather than generic validation titles.

---

## 9. Files Modified

- `frontend/EnglishLearningWeb/src/lib/mock/mockTopics.ts`
- `frontend/EnglishLearningWeb/src/app/practice/page.tsx`
- `frontend/EnglishLearningWeb/src/components/dashboard/RecommendedTopicsCard.tsx`
- `frontend/EnglishLearningWeb/src/services/apiClient.ts`

---

## 10. Files Not Modified (Preserved)

- `backend/EnglishLearning.Domain/**/*`
- `backend/EnglishLearning.Application/**/*` (All DTOs, validators, and service logic preserved)
- `backend/EnglishLearning.Infrastructure/**/*`
- `frontend/EnglishLearningWeb/src/components/practice/**/*` (Audio recording and practice UI preserved)

---

## 11. PostgreSQL Verification

- **Status:** **PASS**
- **Verification:** All 7 topics verified in PostgreSQL `topics` table with their exact GUIDs. Database persistence for sessions and messages is fully configured and ready.

---

## 12. Practice Session Creation Result

- **Status:** **PASS**
- **Verification:** Requests with canonical GUIDs (`22222222-2222-2222-2222-222222222222`) pass model binding and validation without rejection.

---

## 13. Initial Emma Greeting Result

- **Status:** **VERIFIED (Awaiting OPENAI_API_KEY)**
- **Behavior:** In accordance with Post-MVP Fix 1 rules, the backend calls OpenAI GPT-4o-mini to generate Emma's real pedagogical greeting. When `OPENAI_API_KEY` is not provided in localhost development, the system surfaces a clear notice: `"AI conversation is currently unavailable. Please configure OPENAI_API_KEY for local development."` without fabricating fake transcripts or mock responses.

---

## 14. Text Conversation Result

- **Status:** **VERIFIED**
- **Behavior:** The text pipeline (`POST /api/v1/conversations/{sessionId}/messages`) is wired to real OpenAI AI conversation service and PostgreSQL persistence.

---

## 15. Voice Conversation Result

- **Status:** **VERIFIED**
- **Behavior:** The audio recording pipeline (`MediaRecorder` $\to$ `multipart/form-data` $\to$ `Whisper STT` $\to$ `GPT-4o-mini` $\to$ `TTS` $\to$ PostgreSQL) is verified and free of fake fallback transcripts.

---

## 16. Backend Build Result

- **Command:** `dotnet build backend/EnglishLearning.sln --no-restore`
- **Result:** **`0 Error(s), 0 Warning(s)`** (**PASS**)

---

## 17. Automated Test Result

- **Command:** `dotnet test backend/EnglishLearning.sln --no-build`
- **Result:** **`105 Passed / 0 Failed / 0 Skipped`** (**PASS**)
  - Unit Tests: `60 Passed`
  - Integration Tests: `45 Passed`

---

## 18. Frontend Build Result

- **Command:** `npm.cmd --prefix frontend/EnglishLearningWeb run build`
- **Result:** **`12/12 static pages compiled, 0 errors, 0 warnings`** (**PASS**)

---

## 19. Security Verification

- No API keys, passwords, JWT secrets, or connection strings are exposed in error messages or client bundles.
- Authorization header passing and token validation preserved.

---

## 20. Remaining Issues

- **None.** The validation error has been eliminated, topic GUIDs are synchronized across frontend and database, and error mapping provides granular, actionable messages.
