---
name: security
description: >-
  Enforces authentication security, authorization matrices, user data isolation, audio upload safety,
  prompt protection, and secret management. Use whenever reviewing or modifying authentication, API endpoints,
  file upload handlers, or configuration.
---

# Security — English Learning AI

This skill defines the mandatory security policies, authorization rules, and data protection standards for the application.

## Security Rules

### 1. Secret & Key Protection
- **No Secrets in Frontend:** Never place OpenAI API keys, JWT secret keys, or database connection strings in Next.js frontend code or browser-visible files.
- **No Secrets in Git:** Never hardcode secrets in source code or commit `.env`, `appsettings.json`, or credential files containing real credentials.
- **No Secrets in Logs:** Never log authorization headers, passwords, JWT tokens, complete user transcripts, or API keys.

### 2. Authentication & Authorization Matrix
- All API endpoints under `/api/v1/conversations`, `/api/v1/progress`, and `/api/v1/auth/me` require valid JWT Bearer tokens (`[Authorize]`).
- Password hashing must use PBKDF2 with HMAC-SHA512 and unique cryptographic salts (`Microsoft.AspNetCore.Identity.PasswordHasher`).
- Tokens must validate issuer, audience, lifetime, and signing key with zero clock skew tolerance.

### 3. Tenant Boundary & User Isolation
- Every query, conversation retrieval, audio processing turn, assessment generation, and history fetch MUST verify that the target session belongs to the caller:
  ```csharp
  if (session.UserId != currentUserId)
  {
      throw new UnauthorizedSessionAccessException(sessionId);
  }
  ```
- Cross-user access attempts must return `HTTP 404 Not Found` (via `GlobalExceptionHandlingMiddleware`) to prevent revealing resource existence to attackers.

### 4. Input & File Upload Validation
- **Audio Uploads:** Enforce a maximum file size limit of 10MB (`10,485,760` bytes). Validate MIME types (`audio/webm`, `audio/wav`, `audio/mp3`, `audio/mp4`, `audio/ogg`, `audio/aac`) and extensions.
- **Path Traversal Protection:** Sanitize file names; process uploads in-memory via streams without executing arbitrary files on disk.
- **Input Bounds:** Enforce FluentValidation limits (e.g. message length <= 2,000 characters, valid CEFR enums).
- **Pagination Bounds:** Clamp `page >= 1` and `pageSize` within `[1, 100]` to avoid Denial of Service via unbounded allocations.

### 5. Safe Error Responses
- Return standardized RFC 7807 Problem Details for all API errors.
- Never expose internal database connection strings, filesystem paths, or exception stack traces to clients.
