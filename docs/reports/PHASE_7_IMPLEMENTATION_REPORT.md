# Phase 7 Implementation Report — Speech Integration (STT & TTS)

**Date:** 2026-08-14  
**Milestone:** Phase 7 — Speech Integration  
**Status:** PASS  
**Base Architecture Document:** [docs/ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md)  
**Architecture Review Document:** [docs/ARCHITECTURE_REVIEW.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE_REVIEW.md)  
**SRS Reference:** [docs/SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/SRS.md)

---

## 1. Phase Objective

The objective of Phase 7 was to implement end-to-end speech integration for spoken English practice:
- Browser microphone capture using the standard Web `MediaRecorder` API.
- Multipart form-data audio upload to ASP.NET Core Web API (`POST /api/v1/conversations/{sessionId}/speech`).
- Server-side audio validation (format, size limits, integrity).
- Speech-to-Text (STT) transcription via `ISpeechToTextService` / Whisper.
- Reusing the existing Phase 6 conversation pipeline (context windowing, CEFR prompt adaptation, message persistence).
- Text-to-Speech (TTS) voice synthesis via `ITextToSpeechService` / OpenAI TTS.
- Audio delivery and playback in the frontend with autoplay and replay support.
- Preserving text conversation mode alongside voice practice.

---

## 2. Architecture Overview

```text
[Browser Frontend (Next.js)]
   ├── useAudioRecorder (Microphone capture, MediaRecorder Web API, Blob generation)
   ├── InteractiveMicControl (Recording button, waveform pulse, duration timer)
   ├── ConversationFeed (Message bubbles, HTMLAudioElement playback, replay controls)
   └── conversationService.sendSpeech (multipart/form-data with Bearer JWT)
          │
          ▼
[ASP.NET Core 8 Web API]
   ├── ConversationsController (POST /api/v1/conversations/{sessionId}/speech)
   │      │
   │      ▼
   ├── Application Layer
   │      ├── ISpeechConversationService / SpeechConversationService
   │      │      ├── 1. Audio validation (mime type, size <= 10MB)
   │      │      ├── 2. ISpeechToTextService.TranscribeAsync (Whisper)
   │      │      ├── 3. IConversationService.SendMessageAsync (Reusing Phase 6 AI)
   │      │      └── 4. ITextToSpeechService.SynthesizeAsync (OpenAI TTS)
   │      └── ISpeechToTextService & ITextToSpeechService Abstractions
   │      │
   │      ▼
   └── Infrastructure Layer
          ├── WhisperSpeechToTextService (HTTP client calling Whisper API)
          ├── OpenAiTextToSpeechService (HTTP client calling OpenAI TTS API)
          └── PostgreSQL Persistence (reusing ConversationSession & ConversationMessage)
```

---

## 3. Speech-to-Text (STT) Interface & Provider

### Interface (`Application/Interfaces/Infrastructure/ISpeechToTextService.cs`)
```csharp
public record SpeechRecognitionResult(
    string Transcript,
    double Confidence = 1.0,
    TimeSpan? Duration = null
);

public interface ISpeechToTextService
{
    Task<SpeechRecognitionResult> TranscribeAsync(
        Stream audioStream,
        string contentType,
        string languageCode = "en-US",
        CancellationToken cancellationToken = default
    );
}
```

### Provider Implementation (`Infrastructure/Ai/WhisperSpeechToTextService.cs`)
- Communicates with `POST https://api.openai.com/v1/audio/transcriptions` via typed `HttpClient`.
- Uses `model: whisper-1` and `language: en`.
- Determines extension and MIME type safely from incoming content types (`webm`, `wav`, `mp3`, `mp4`, `m4a`, `ogg`, `aac`).
- Includes deterministic local fallback for testing and development environments without API keys.

---

## 4. Text-to-Speech (TTS) Interface & Provider

### Interface (`Application/Interfaces/Infrastructure/ITextToSpeechService.cs`)
```csharp
public record VoiceOptions(
    string Voice = "alloy",
    float Speed = 1.0f,
    string Format = "audio/mpeg"
);

public record TextToSpeechResult(
    byte[] AudioData,
    string ContentType,
    TimeSpan? Duration = null
);

public interface ITextToSpeechService
{
    Task<TextToSpeechResult> SynthesizeAsync(
        string text,
        VoiceOptions? options = null,
        CancellationToken cancellationToken = default
    );
}
```

### Provider Implementation (`Infrastructure/Ai/OpenAiTextToSpeechService.cs`)
- Communicates with `POST https://api.openai.com/v1/audio/speech`.
- Uses `model: tts-1`, voice `"nova"` (friendly English tutor voice), speed `1.0f`, format `mp3`.
- Returns binary audio payload with MIME type `audio/mpeg`.
- Includes fallback synthetic frame generation for offline development and unit tests.

---

## 5. Speech Conversation Flow & Reusing Phase 6

`SpeechConversationService` coordinates speech turns without duplicating conversation logic:
1. Validates audio upload format and size.
2. Invokes `_sttService.TranscribeAsync` to obtain recognized text.
3. Passes the recognized text directly into `_conversationService.SendMessageAsync` (which checks session ownership, active status, updates sequence, loads rolling context, calls `IAiConversationService`, persists messages, and updates duration).
4. Calls `_ttsService.SynthesizeAsync` on the AI tutor's response text.
5. Returns `SpeechConversationResponseDto` containing recognized text, AI message, and Base64-encoded audio for playback.

---

## 6. Audio Upload Endpoint

```http
POST /api/v1/conversations/{sessionId}/speech
Authorization: Bearer <jwt-token>
Content-Type: multipart/form-data

Form Field: audio (binary file)
```

**Response (`200 OK`):**
```json
{
  "success": true,
  "message": "Speech processed successfully.",
  "data": {
    "session": { "id": "...", "status": "Active", ... },
    "userMessage": { "id": "...", "role": "user", "messageText": "I visited Paris.", ... },
    "aiMessage": { "id": "...", "role": "assistant", "messageText": "Paris is wonderful! ...", ... },
    "recognizedText": "I visited Paris.",
    "audioBase64": "SUQzBAAAAAAAI1RTU0UAAAAPAAADTGF2ZjU4Ljc2...",
    "audioContentType": "audio/mpeg"
  }
}
```

---

## 7. Browser MediaRecorder & Permission Handling

- Implemented via `useAudioRecorder` React hook (`src/hooks/useAudioRecorder.ts`).
- Requests audio stream with echo cancellation, noise suppression, and auto gain control.
- Automatically selects the best browser-supported format (`audio/webm;codecs=opus`, `audio/webm`, `audio/mp4`, `audio/ogg`, `audio/aac`).
- Cleans up and releases all microphone tracks (`track.stop()`) on recording stop, cancellation, or component unmount.
- Distinguishes permission errors (`NotAllowedError`), device errors (`NotFoundError`), and browser support limitations with user-friendly messages.

---

## 8. Supported Audio Formats & Size Limits

- **Supported MIME Types:** `audio/webm`, `audio/webm;codecs=opus`, `audio/wav`, `audio/x-wav`, `audio/wave`, `audio/mp3`, `audio/mpeg`, `audio/mp4`, `audio/m4a`, `audio/ogg`, `audio/aac`.
- **Supported Extensions:** `.webm`, `.wav`, `.mp3`, `.m4a`, `.mp4`, `.ogg`, `.aac`.
- **Maximum Audio Upload Size:** **10 MB** (10,485,760 bytes), configured in `AiSettings:MaxAudioSizeBytes`.

---

## 9. Error Handling & Partial Failure Strategy

- **Empty / Corrupt Audio:** Returns `HTTP 400 Bad Request` with `InvalidAudioException`.
- **Oversized Audio:** Returns `HTTP 400 Bad Request` explaining maximum file limits.
- **Cross-User Intrusion:** Returns `HTTP 404 Not Found` for non-owned sessions.
- **TTS Failure Handling:** If Text-to-Speech synthesis fails, the service logs a warning and returns the recognized text and AI message with `audioBase64 = null`. The conversation is never lost because of a TTS transient failure.
- **STT Failure Handling:** If speech transcription produces empty output or errors, returns client-friendly error asking the user to speak clearly.

---

## 10. Frontend Audio Playback

- `ConversationFeed.tsx` incorporates `HTMLAudioElement` playback using data URIs (`data:audio/mpeg;base64,...`).
- Visual indicator displays active playing state on Emma's avatar.
- Allows users to pause and replay any voice message at any time.
- Handles browser autoplay policies gracefully.

---

## 11. Dual Mode Operation (Text & Voice)

- Users can seamlessly switch between **Microphone (Voice & Audio)** and **Keyboard (Text Input)** modes via the mode switcher on the practice screen.
- Both modes feed into the same continuous conversation session in PostgreSQL.

---

## 12. Test Results

All 68 backend unit and integration tests passed:

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
  Total: 48, Passed: 48, Failed: 0

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
  Total: 20, Passed: 20, Failed: 0
------------------------------------------------------------
Combined Result: 68 passed, 0 failed, 0 skipped.
```

---

## 13. Build Results

- **Backend Solution (`dotnet build`):** 0 Errors, 0 Warnings.
- **Frontend Build (`npm run build`):** 0 Errors, 0 Warnings, 12 static routes optimized.
- **Frontend Lint (`npm run lint`):** 0 Errors, 0 Warnings.

---

## 14. Database Changes

- **No database schema changes were required for Phase 7.** Reused the existing `conversation_sessions` and `conversation_messages` tables.

---

## 15. Known Issues

- None.

---

## 16. Deviations from SRS & ARCHITECTURE

- **None.** The speech-to-text pipeline, text-to-speech voice synthesis, and REST audio endpoints adhere strictly to [SRS.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/SRS.md) and [ARCHITECTURE.md](file:///c:/Users/swarga.bastin/Documents/EnglishLearningAI/docs/ARCHITECTURE.md).

---

## 17. Conclusion & Next Phase Readiness

Phase 7 is complete and verified. The application now supports full voice conversation practice (speech recording -> STT transcription -> AI conversation -> TTS voice synthesis -> browser playback). The application is prepared for **Phase 8 — Assessment Engine Implementation**.

---
**PHASE 7 STATUS: PASS**
