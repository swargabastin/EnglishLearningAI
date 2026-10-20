---
name: phase-control
description: >-
  Enforces phase governance, scope discipline, and workflow controls to prevent scope creep, unintended
  refactoring, or accidental phase advancement. Use on every interaction to keep changes focused and verified.
---

# Phase Control — English Learning AI

This skill governs the development lifecycle, preventing uncontrolled modifications and maintaining scope integrity.

## Project Milestone Context
- **Phases 1–10 Status:** Completed, verified, and marked **PASS**.
- **Current Milestone:** **Post-MVP Localhost Fixes, Polish & Hardening**.
- **Future Phases (v1.1, v2.0):** Strictly on hold until explicitly authorized by the user.

## Mandatory Phase Control Rules

### 1. No Automatic Phase Advancement
- Never start a new development phase or introduce unrequested roadmap items (e.g. Gamification, XP, Badges, Leaderboards, Subscriptions, Redis, RabbitMQ, WebSockets).
- If the user asks for a specific bug fix or polish item, implement ONLY that item.

### 2. Systematic 7-Step Workflow
Every task on this project must strictly execute these seven steps:

```text
1. INSPECT   ──► Inspect the existing files, architecture, and reports first.
     │
2. PLAN      ──► Outline the exact minimal changes required without redesign.
     │
3. CONFIRM   ──► Confirm requirements if any ambiguity or scope risk exists.
     │
4. IMPLEMENT ──► Modify ONLY the approved files within the requested scope.
     │
5. TEST      ──► Run backend unit/integration tests and frontend lint/build.
     │
6. REPORT    ──► Present exact results, test metrics, and modified files clearly.
     │
7. STOP      ──► Stop and wait for user review before proceeding.
```

### 3. Stability Over Rewrite
- Protect working functionality at all times.
- Make changes incrementally and preserve existing tests and interfaces.
- If an issue is discovered outside the requested scope, document it as a **Known Limitation / Future Consideration** rather than expanding scope without authorization.
