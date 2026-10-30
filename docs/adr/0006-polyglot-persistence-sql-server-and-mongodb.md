# 0006 — Polyglot persistence: SQL Server for writes, MongoDB for read models

- **Status:** Accepted (amended by ADR 0010: summary numbers are computed by the domain)
- **Date:** 2026-10-12

## Context
Financial writes (expenses, contributions, investment movements) must be consistent and
transactional. Screens like the monthly dashboard, however, combine data from many
aggregates and are read far more often than they change. Computing them with joins on
every request couples read performance to the write model.

## Decision
- **SQL Server is the single source of truth.** All commands persist through EF Core
  inside a transaction.
- **MongoDB stores read models**: one denormalized document per screen view.
- Writes to SQL Server also insert integration events into a **transactional outbox**
  table in the same transaction. A projector (`BackgroundService`) reads the outbox and
  upserts the MongoDB documents. Projections are idempotent.
- Read models are disposable and can be rebuilt from SQL Server at any time.
- Phase 1 reads from SQL Server with Dapper; MongoDB read models are introduced in Phase 2,
  once there are enough aggregates to justify them.

## Alternatives considered
- **SQL Server only (Dapper reads):** simpler and fully consistent; kept for Phase 1.
- **MongoDB as primary store:** rejected; multi-document financial consistency is better
  served by a relational store.
- **Azure Service Bus between outbox and projector:** unnecessary at this scale and not
  permanently free; can be added later without changing the outbox.

## Consequences
- Dashboard reads become a single document fetch.
- Read models are **eventually consistent**: the UI may lag behind a write by a short time.
- The outbox gives an event-driven backbone without a paid message broker.
- Two databases to run locally (handled by Aspire) and in the cloud
  (Azure SQL free offer + MongoDB Atlas free cluster or Azure Cosmos DB free tier).
