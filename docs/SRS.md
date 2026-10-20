# AI English Learning App — Software Requirements Specification (SRS)

**Document Version:** 1.0  
**Status:** Initial MVP Specification  
**Project Type:** AI-powered English learning and speaking practice application

---

## 1. Purpose

The AI English Learning App is intended to help users improve their English communication skills through interactive, AI-powered practice.

The initial product will focus on speaking practice. Users will select their English level and a conversation topic, communicate with an AI tutor using voice, and receive feedback on their performance.

The system is designed to evolve into a personalized English learning platform with grammar, vocabulary, fluency, pronunciation, progress tracking, and adaptive learning.

---

## 2. Product Vision

The long-term goal is to provide a personalized AI English tutor that can:

- Conduct natural English conversations.
- Adapt conversations to the learner's English level.
- Identify recurring learning weaknesses.
- Provide useful corrections and explanations.
- Recommend personalized practice.
- Track learning progress over time.

### Core learning loop

```text
Select Level
    ↓
Select Topic
    ↓
Start Practice
    ↓
Speak with AI
    ↓
Speech-to-Text
    ↓
AI Conversation
    ↓
Text-to-Speech
    ↓
Continue Conversation
    ↓
End Session
    ↓
AI Assessment
    ↓
Personalized Feedback
```

---

## 3. Scope

### 3.1 MVP Scope

Version 1 shall include:

1. User registration
2. User login/logout
3. User profile
4. English-level selection
5. Conversation-topic selection
6. AI speaking conversation
7. Speech-to-text integration
8. AI-generated responses
9. Text-to-speech integration
10. Conversation history
11. Post-session assessment
12. Grammar feedback
13. Vocabulary feedback
14. Fluency feedback
15. Overall performance score
16. Basic progress tracking

### 3.2 Future Scope

The following features are planned for later releases:

- Detailed pronunciation analysis
- Word-level pronunciation scoring
- Grammar lessons
- Vocabulary exercises
- Flashcards
- Daily challenges
- Streaks
- Personalized learning plans
- Adaptive difficulty
- Notifications
- Subscription/payment
- Mobile application
- Advanced analytics
- Admin dashboard
- Multiple AI providers
- Offline learning where technically feasible

---

## 4. Target Users

### 4.1 Learner

The primary application user.

The learner can:

- Register and log in.
- Set an English level.
- Set learning goals.
- Select conversation topics.
- Practice speaking with the AI tutor.
- Review corrections.
- View performance.
- Track progress.

### 4.2 Administrator

The administrator is a future role.

An administrator may eventually manage:

- Users
- Topics
- Lessons
- Vocabulary
- Learning content
- AI configuration
- Application settings
- Reports

Admin functionality is not required for the initial MVP unless specifically added later.

---

## 5. English Proficiency Levels

The application shall support the following CEFR-inspired levels:

| Level | Description |
|---|---|
| A1 | Beginner |
| A2 | Elementary |
| B1 | Intermediate |
| B2 | Upper Intermediate |
| C1 | Advanced |
| C2 | Proficient |

The selected level should influence the complexity of AI conversations.

---

## 6. Learning Goals

Users may select one or more learning goals in future versions.

Initial examples:

- Improve speaking
- Improve grammar
- Improve vocabulary
- Improve fluency
- English for workplace
- English for interviews
- English for travel

For MVP, speaking improvement should be the primary goal.

---

## 7. Conversation Topics

Initial topics:

1. Daily Conversation
2. Job Interview
3. Travel
4. Shopping
5. Restaurant
6. Workplace
7. Introducing Yourself

The topic system should be designed so that new topics can be added without changing application architecture.

---

# 8. Functional Requirements

## FR-001 — User Registration

The system shall allow a new user to register.

Required information:

- Name
- Email
- Password

The system shall:

- Validate required fields.
- Validate email format.
- Enforce password requirements.
- Prevent duplicate email registration.
- Store passwords securely using a password-hashing mechanism.

Plain-text passwords must never be stored.

---

## FR-002 — User Login

The system shall allow registered users to log in using their credentials.

Successful login shall provide an authenticated session/token.

Invalid credentials shall return an appropriate error message without exposing sensitive information.

---

## FR-003 — User Logout

The application shall provide a logout mechanism.

The frontend shall remove/expire the locally stored authentication state according to the selected authentication strategy.

---

## FR-004 — User Profile

Users shall be able to view and update their profile.

Initial profile information:

- Name
- Email
- English level
- Learning goal

---

## FR-005 — English Level Selection

Users shall be able to select their English proficiency level.

The selected level shall be stored against the user profile.

The conversation engine shall use the selected level as part of the conversation context.

---

## FR-006 — Topic Selection

Users shall be able to select a conversation topic before starting a session.

Each topic should contain:

- Topic name
- Description
- Recommended level/difficulty where applicable

---

## FR-007 — Start Conversation

The user shall be able to start a speaking practice session.

The session shall record:

- User
- Topic
- English level
- Start time
- Session status

---

## FR-008 — Microphone Input

The conversation screen shall provide a microphone control.

The user shall be able to:

- Start recording.
- Stop recording.
- See recording/listening state.
- Receive an appropriate error if microphone access fails.

The application shall request microphone permission through the supported platform/browser mechanism.

---

## FR-009 — Speech-to-Text

User speech shall be converted into text through a speech-to-text service.

The resulting text shall be associated with the conversation session.

The system should preserve sufficient information for later assessment.

Speech service failures shall be handled gracefully.

---

## FR-010 — AI Conversation

The AI conversation engine shall:

- Understand the user's message.
- Maintain conversation context during a session.
- Respond naturally.
- Ask relevant follow-up questions.
- Respect the user's selected English level.
- Use topic context.
- Avoid unnecessary complexity for lower-level learners.
- Encourage continued conversation.

The AI should behave as an English tutor rather than as a generic chatbot.

---

## FR-011 — Conversation Context

The system shall maintain conversation history during an active session.

Each conversation message shall identify:

- Session
- Speaker role
- Message content
- Timestamp

Speaker roles should distinguish at minimum:

- User
- AI

---

## FR-012 — Text-to-Speech

The AI response shall optionally be converted into speech using a text-to-speech service.

The user shall be able to hear the AI response.

The implementation should allow the TTS provider to be replaced later.

---

## FR-013 — End Conversation

The user shall be able to end a conversation.

When a session ends, the system shall record:

- End time
- Duration
- Conversation messages
- Assessment status

The application shall then provide or initiate the post-session assessment.

---

## FR-014 — Grammar Assessment

The system shall analyze the user's conversation text for grammar issues.

For identified issues, the system should provide:

- Original sentence
- Corrected sentence
- Short explanation

Example:

```text
Original:
I am working here since 2022.

Correction:
I have been working here since 2022.

Explanation:
Use the present perfect continuous for an action
that started in the past and continues to the present.
```

Corrections should be useful and concise.

---

## FR-015 — Vocabulary Assessment

The system shall identify useful vocabulary from the conversation.

The system may identify:

- Words used correctly
- Repeated/simple vocabulary
- Suggested alternative vocabulary
- New words relevant to the topic

Future versions may store vocabulary for personalized practice.

---

## FR-016 — Fluency Assessment

The system shall provide a basic fluency assessment.

Potential indicators include:

- Speaking duration
- Number of words
- Speaking rate
- Filler words
- Repetition
- Long pauses where measurable
- Sentence completion

The initial implementation may use only metrics reliably available from the selected speech/AI services.

The system shall not present inaccurate measurements as precise scientific scores.

---

## FR-017 — Pronunciation Assessment

Detailed pronunciation assessment is a future feature.

The architecture shall provide an extension point for pronunciation analysis without requiring a redesign of the conversation module.

---

## FR-018 — Session Score

The system shall provide an overall session score.

Initial score categories:

- Grammar
- Vocabulary
- Fluency
- Pronunciation (when available)
- Overall score

Scores should be clearly identified as application-generated learning indicators rather than formal language certification.

---

## FR-019 — Feedback

After a session, the user shall receive:

- Overall score
- Strengths
- Areas for improvement
- Grammar corrections
- Vocabulary suggestions
- Fluency feedback
- Recommended practice

Example:

```text
Overall Score: 78/100

Grammar: 75
Vocabulary: 82
Fluency: 76

Strengths:
- Good topic vocabulary
- Clear sentence structure

Improve:
- Verb tense usage
- Sentence expansion

Recommended Practice:
- Present perfect tense
- Workplace vocabulary
```

---

## FR-020 — Conversation History

Users shall be able to view previous practice sessions.

History should include:

- Date
- Topic
- Duration
- Overall score

Detailed message history may be available from a session-detail view.

---

## FR-021 — Progress Tracking

The system shall maintain basic learning progress.

Initial metrics:

- Total practice time
- Number of conversations
- Average score
- Grammar score
- Vocabulary score
- Fluency score
- Pronunciation score when available
- Words learned, when vocabulary tracking is enabled

---

# 9. User Interface Requirements

## 9.1 Main Screens

The MVP should contain:

1. Login
2. Registration
3. Dashboard
4. Profile
5. Topic Selection
6. Conversation
7. Conversation Result
8. Conversation History

---

## 9.2 Dashboard

The dashboard should show:

- Greeting
- English level
- Daily practice target
- Progress
- Start speaking practice button
- Recent sessions
- Basic learning statistics

Example:

```text
Good morning!

English Level
B1 — Intermediate

Today's Goal
12 / 15 minutes

[ Start Speaking Practice ]

Recent Progress
Speaking       78%
Grammar        72%
Vocabulary     81%
Fluency        76%
```

---

## 9.3 Conversation Screen

The conversation screen should prioritize speaking.

It should contain:

- Current topic
- AI message
- AI audio playback
- Microphone button
- Recording/listening indicator
- User speech transcript
- Conversation status
- End conversation button

The UI should not be overloaded with controls.

---

## 9.4 Result Screen

The result screen should clearly present:

- Overall score
- Category scores
- Strengths
- Corrections
- Suggested vocabulary
- Improvement areas
- Recommended next practice

---

# 10. System Architecture

The proposed architecture is:

```text
┌──────────────────────────────────────┐
│            Next.js Frontend          │
│        TypeScript + UI Components    │
└──────────────────┬───────────────────┘
                   │
                  HTTPS
                   │
┌──────────────────▼───────────────────┐
│        ASP.NET Core Web API          │
│                                      │
│ Authentication                       │
│ User Management                      │
│ Conversation Management              │
│ Assessment                           │
│ Progress                             │
└───────────┬───────────────┬──────────┘
            │               │
            ▼               ▼
     ┌────────────┐   ┌───────────────┐
     │ PostgreSQL │   │ AI Services   │
     └────────────┘   │               │
                      │ STT           │
                      │ LLM           │
                      │ TTS           │
                      └───────────────┘
```

---

# 11. Technology Stack

## Frontend

- Next.js
- TypeScript
- Tailwind CSS
- Reusable component architecture

## Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- REST APIs

## Database

- PostgreSQL

## Authentication

- JWT-based authentication

## AI

An external AI/LLM API shall be used initially.

The application shall not train its own foundation model for the MVP.

AI provider integration shall be isolated behind backend service interfaces so that the provider can be replaced later.

## Speech

Speech-to-text and text-to-speech shall be implemented behind service abstractions.

The exact provider may be selected during architecture/design implementation.

---

# 12. Database Requirements

Initial entities:

```text
Users
ConversationSessions
ConversationMessages
Assessments
Vocabulary
Topics
```

## Users

Suggested fields:

- Id
- Name
- Email
- PasswordHash
- EnglishLevel
- LearningGoal
- CreatedAt
- UpdatedAt

## ConversationSessions

Suggested fields:

- Id
- UserId
- TopicId
- EnglishLevel
- StartedAt
- EndedAt
- Duration
- OverallScore
- Status

## ConversationMessages

Suggested fields:

- Id
- SessionId
- Role
- Message
- CreatedAt

## Assessments

Suggested fields:

- Id
- SessionId
- GrammarScore
- VocabularyScore
- FluencyScore
- PronunciationScore
- OverallScore
- Feedback
- CreatedAt

## Vocabulary

Suggested fields:

- Id
- UserId
- Word
- Meaning
- Example
- MasteryLevel
- CreatedAt

## Topics

Suggested fields:

- Id
- Name
- Description
- Difficulty
- IsActive

The exact schema shall be finalized during architecture and database design.

---

# 13. API Requirements

The backend should expose versioned REST APIs.

Example structure:

```text
/api/v1/auth/register
/api/v1/auth/login
/api/v1/auth/me

/api/v1/users/profile

/api/v1/topics
/api/v1/topics/{id}

/api/v1/conversations
/api/v1/conversations/{id}
/api/v1/conversations/{id}/messages
/api/v1/conversations/{id}/end

/api/v1/assessments/{sessionId}

/api/v1/progress
/api/v1/vocabulary
```

AI provider APIs and secrets must never be exposed directly to the frontend.

---

# 14. AI Architecture Requirements

The AI layer shall be isolated from the UI.

Preferred flow:

```text
Frontend
   ↓
Backend API
   ↓
Conversation Service
   ↓
AI Provider Interface
   ↓
AI Provider
```

The application should use interfaces such as:

```text
IAiConversationService
ISpeechToTextService
ITextToSpeechService
IAssessmentService
```

This allows service providers to be changed without rewriting the application.

---

# 15. AI Conversation Rules

The AI tutor should:

- Be encouraging.
- Maintain the selected topic.
- Adapt to the learner's level.
- Ask one useful question at a time.
- Maintain context.
- Avoid unnecessary correction during natural conversation.
- Provide detailed corrections primarily during assessment.
- Avoid claiming that generated scores are official CEFR certification.
- Avoid providing misleading certainty about pronunciation or fluency measurements.

The system prompt/configuration for the AI shall be maintained separately from application UI code.

---

# 16. Security Requirements

The application shall:

- Never store plain-text passwords.
- Never expose API keys in frontend source code.
- Store secrets using environment/configuration management.
- Use HTTPS in production.
- Validate input on the backend.
- Use authorization for protected APIs.
- Protect user conversation data.
- Avoid logging sensitive credentials or tokens.
- Apply appropriate database access controls.

AI provider credentials must remain server-side.

---

# 17. Error Handling

The system shall gracefully handle:

- Invalid login
- Duplicate registration
- Database connection failures
- AI provider failures
- Speech-to-text failures
- Text-to-speech failures
- Microphone permission denial
- Network interruptions
- Invalid API requests
- Session timeout

The UI should provide understandable user-facing messages.

Technical details should be logged server-side without exposing sensitive information to users.

---

# 18. Logging and Monitoring

The backend should provide structured logging for:

- Authentication events
- API errors
- AI service failures
- Speech service failures
- Conversation session lifecycle
- Database errors

Logs must not contain:

- Passwords
- API keys
- Authentication tokens
- Unnecessary sensitive user data

Production monitoring can be added in a later phase.

---

# 19. Performance Requirements

The application should aim for:

- Responsive UI interactions.
- Efficient API calls.
- Minimal unnecessary database queries.
- Reasonable AI response latency.
- Efficient audio handling.
- Graceful handling of slow external services.

The exact performance targets should be defined after selecting the AI and speech providers.

---

# 20. Scalability Requirements

The architecture should support future growth in:

- Number of users
- Number of conversations
- AI providers
- Speech providers
- Learning topics
- Learning content
- Analytics
- Mobile clients

The frontend, backend, database, AI, and speech integrations should remain logically separated.

---

# 21. Testing Requirements

Testing shall be introduced throughout development rather than only at the end.

## Unit Testing

Test:

- Authentication services
- Business logic
- Assessment calculations
- Validation
- Data transformations

## API Testing

Test:

- Registration
- Login
- User profile
- Topics
- Conversations
- Assessments
- Progress

## Integration Testing

Test:

- Backend + PostgreSQL
- Backend + AI service
- Backend + speech services

## UI Testing

Test:

- Navigation
- Forms
- Login
- Conversation flow
- Result display
- Error states

## Manual Testing

Verify:

- Microphone permissions
- Speech recognition
- AI conversation quality
- Audio playback
- Different English levels
- Different conversation topics

---

# 22. Development Phases

## Phase 1 — Requirements and Architecture

Deliverables:

- SRS.md
- ARCHITECTURE.md
- Architecture review

No application feature implementation should begin before the architecture is reviewed.

## Phase 2 — Project Skeleton

Deliverables:

- Next.js project
- ASP.NET Core project
- PostgreSQL configuration
- Basic API/frontend connectivity
- Environment configuration

## Phase 3 — Frontend UI

Implement:

- Login
- Registration
- Dashboard
- Profile
- Topic Selection
- Conversation
- Results
- History

Use mock data initially.

## Phase 4 — Backend Foundation

Implement:

- Database
- Entities
- EF Core
- Migrations
- APIs
- Error handling
- Logging

## Phase 5 — Authentication

Implement:

- Registration
- Password hashing
- Login
- JWT
- Authorization
- User profile

## Phase 6 — AI Conversation

Implement:

- Conversation service
- AI provider integration
- Conversation context
- AI responses

## Phase 7 — Speech

Implement:

- Speech-to-text
- Text-to-speech
- Microphone handling
- Audio playback

## Phase 8 — Assessment

Implement:

- Grammar assessment
- Vocabulary assessment
- Fluency assessment
- Overall scoring
- Feedback

## Phase 9 — Progress

Implement:

- Session history
- Progress metrics
- Dashboard statistics

## Phase 10 — Testing and Hardening

Perform:

- Unit testing
- Integration testing
- UI testing
- Security review
- Error handling review
- Performance testing

## Phase 11 — Future Features

Possible additions:

- Pronunciation analysis
- Adaptive learning
- Vocabulary exercises
- Grammar lessons
- Daily challenges
- Streaks
- Subscription
- Mobile application

---

# 23. MVP Acceptance Criteria

The MVP shall be considered functional when a new user can:

```text
Register
   ↓
Login
   ↓
Select English Level
   ↓
Select Conversation Topic
   ↓
Start Speaking Practice
   ↓
Allow Microphone
   ↓
Speak
   ↓
Speech converted to text
   ↓
AI understands the message
   ↓
AI responds
   ↓
User hears AI response
   ↓
Continue conversation
   ↓
End session
   ↓
Receive assessment
   ↓
View feedback
   ↓
View session in history
```

The complete flow must work without requiring manual database changes or developer intervention.

---

# 24. Development Rules

The development team and coding agents shall follow these rules:

1. Read this SRS before implementing a phase.
2. Do not implement features outside the approved scope without documenting them.
3. Do not modify unrelated modules when implementing a feature.
4. Keep frontend and backend responsibilities separated.
5. Keep AI and speech providers behind service abstractions.
6. Never expose secrets in frontend code.
7. Build and test after each development phase.
8. Document significant architectural changes.
9. Do not replace working architecture merely for convenience.
10. Keep the project maintainable and production-oriented.
11. Do not use mock data in production functionality.
12. Remove or isolate temporary mock implementations before production release.

---

# 25. Project Folder Direction

The intended high-level structure is:

```text
EnglishLearningAI/
│
├── docs/
│   ├── SRS.md
│   ├── ARCHITECTURE.md
│   └── reports/
│
├── frontend/
│   └── EnglishLearningWeb/
│
├── backend/
│   └── EnglishLearningAPI/
│
├── tests/
│
├── .gitignore
└── README.md
```

The exact internal structure shall be finalized in `ARCHITECTURE.md`.

---

# 26. Future Product Vision

The application should eventually evolve from a simple AI conversation tool into a personalized English tutor.

Long-term learning loop:

```text
Conversation
     ↓
Assessment
     ↓
Identify Weaknesses
     ↓
Update Learner Profile
     ↓
Generate Personalized Practice
     ↓
Practice
     ↓
Re-assess
     ↓
Track Improvement
```

The ultimate goal is:

> **An AI English tutor that understands how each learner communicates, identifies their weaknesses, adapts the difficulty, and continuously personalizes their learning experience.**

---

## 27. Document Status

**Version:** 1.0  
**Scope:** MVP + future architecture direction  
**Implementation Status:** Not yet implemented  
**Next Document:** `ARCHITECTURE.md`

**Next development action:**

1. Review this SRS.
2. Create `ARCHITECTURE.md`.
3. Review architecture.
4. Only then begin implementation.
