---
name: postgresql-efcore-database
description: >-
  Governs PostgreSQL database persistence, Entity Framework Core models, migrations, relationships,
  and repository patterns. Use whenever querying, creating, modifying, or migrating database entities
  and tables.
---

# PostgreSQL & EF Core Database — English Learning AI

This skill governs all database interactions, schema design, and entity configurations for the PostgreSQL database (`english_learning_db`).

## Core Technology Stack
- **Database Engine:** PostgreSQL 18
- **ORM:** Entity Framework Core 8
- **Database Provider:** `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.8

## Persistence Rules

### 1. PostgreSQL is Authoritative
- All user profiles, conversation sessions, dialogue messages, assessments, progress records, and vocabulary entries are stored authoritatively in PostgreSQL.
- Never replace real database data with frontend state or in-memory mock datasets in live mode.
- The frontend must never access PostgreSQL directly; all access is routed through backend Infrastructure repositories.

### 2. Repository Pattern & Clean Abstraction
- Repositories reside in `EnglishLearning.Infrastructure/Persistence/Repositories/` and implement interfaces from `EnglishLearning.Application/Interfaces/Repositories/`.
- Repositories use `AppDbContext` and entity configurations (`IEntityTypeConfiguration<T>`).

### 3. EF Core Migrations & Schema Changes
Before making any database schema modification:
1. Inspect the existing Domain entity and `AppDbContext` configurations.
2. Inspect existing migrations in `backend/EnglishLearning.Infrastructure/Migrations/`.
3. Make the smallest required change in the Domain entity and configuration.
4. Generate a new EF Core migration via CLI:
   ```bash
   dotnet ef migrations add <MigrationName> --project backend/EnglishLearning.Infrastructure --startup-project backend/EnglishLearning.Api
   ```
5. Apply migration safely without dropping existing tables or data.
6. Verify foreign key constraints, indexes, cascade deletion behaviors, and run the test suite.

### 4. Query Performance & Tenant Security
- Always scope queries to the authenticated user ID (`UserId`).
- Avoid unbounded queries; enforce pagination parameters (`page`, `pageSize <= 100`).
- Prevent N+1 query patterns by using `.Include()` or explicit projections where appropriate.
