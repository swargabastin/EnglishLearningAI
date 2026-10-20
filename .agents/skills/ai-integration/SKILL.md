---
name: ai-integration
description: >-
  Protects all OpenAI, Whisper Speech-to-Text (STT), Text-to-Speech (TTS), and Assessment integrations.
  Use whenever modifying speech recording, transcription pipelines, AI tutor prompt engineering,
  or assessment evaluation services.
---

# AI Integration — English Learning AI

This skill governs all artificial intelligence, speech recognition, speech synthesis, and assessment capabilities.

## The Voice Pipeline

```text
Browser Microphone
       │
       ▼ (User speaks into mic)
MediaRecorder API (Chunks audio)
       │
       ▼ (multipart/form-data with WebM/WAV/MP3)
ASP.NET Core API (`POST /api/v1/conversations/{id}/speech`)
       │
       ▼
SpeechConversationService (Validates audio format & 10MB limit)
       │
       ▼
ISpeechToTextService (`WhisperSpeechToTextService` -> OpenAI Whisper `whisper-1`)
       │
       ▼ (Actual transcribed text)
IConversationService (`ConversationService` -> Appends User message to PostgreSQL)
       │
       ▼
IAiConversationService (`OpenAiConversationService` -> OpenAI `gpt-4o-mini`)
       │
       ▼ (Tutor message generated & saved)
ITextToSpeechService (`OpenAiTextToSpeechService` -> OpenAI TTS `tts-1` / voice `nova`)
       │
       ▼ (Base64 audio payload)
Browser (Audio playback & interactive message feed)
```

## Mandatory AI Integration Rules

### 1. No Fake Transcripts in Live Runtime
- **Strict Prohibition:** DO NOT silently fabricate or return hardcoded mock transcripts in live Development or Production environments when the user speaks.
- If the OpenAI API key is missing or invalid, or if Whisper fails, return a clear, user-facing error message (`AiServiceException`).
- Deterministic test doubles are restricted strictly to automated unit and integration tests.

### 2. Credential Protection
- OpenAI API keys must exist **ONLY** in backend environment configuration (`OPENAI_API_KEY` or `AiSettings:ApiKey`).
- Never expose API keys to the browser.
- Never use `NEXT_PUBLIC_` environment variables for backend secrets.
- Never hardcode, log, or commit API keys into source control.

### 3. Prompt Isolation & Injection Protection
- User-supplied dialogue must always be enclosed in isolated contextual blocks within prompts.
- Assessment prompts must enforce strict schema evaluation (`response_format: json_object`) and reject user prompt injection attempts.

### 4. Resilient Error Handling
- Explicitly handle timeouts, rate limits (`429`), network interruptions, and malformed JSON responses.
- If TTS fails partially, preserve the text transcript and AI conversation turn rather than crashing the session.
