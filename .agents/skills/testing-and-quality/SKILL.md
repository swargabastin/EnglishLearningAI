---
name: testing-and-quality
description: >-
  Governs test coverage, baseline preservation, regression testing, and build verification across
  backend and frontend suites. Use whenever running tests, verifying bug fixes, adding new features,
  or preparing reports.
---

# Testing & Quality — English Learning AI

This skill enforces rigorous automated testing, regression verification, and zero-defect build standards.

## Baseline Test Suite
- **Current Total:** **105 Automated Tests** (60 Unit Tests + 45 Integration Tests).
- **Status:** 100% passing across all domain, service, security, speech, assessment, and progress suites.

## Mandatory Testing Rules

### 1. Test Preservation
- **Never Delete Tests:** Never remove, skip, or comment out existing tests merely to make a build pass.
- **Never Disable Assertions:** Fix the root cause rather than weakening validation checks.

### 2. Coverage Requirements
- Write unit tests for new service orchestrations, business algorithms, and validators.
- Write integration tests for new API endpoints, security bounds, and cross-user isolation.
- Test authorization boundaries:
  - User A -> User A session -> `ALLOW (200 OK)`
  - User A -> User B session -> `DENY (404 Not Found)`
  - Unauthenticated -> protected endpoint -> `DENY (401 Unauthorized)`
- Test edge cases: empty files, oversized audio (>10MB), invalid CEFR levels, empty messages, bounded pagination.

### 3. Verification Commands

Before completing any task or claiming success, execute the following verification commands:

```bash
# 1. Clean build and run backend unit & integration tests
dotnet build backend/EnglishLearning.sln
dotnet test backend/EnglishLearning.sln

# 2. Verify frontend linting and type-safety
npm --prefix frontend/EnglishLearningWeb run lint
npm --prefix frontend/EnglishLearningWeb run build
```

### 4. Zero-Warning Standard
- All compiler warnings must be investigated and resolved.
- Never claim success without running the actual commands and inspecting their terminal output.
