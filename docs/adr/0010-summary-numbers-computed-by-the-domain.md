# 0010 — Summary numbers are computed by the domain, not by SQL

- **Status:** Accepted
- **Date:** 2026-10-29
- **Amends:** ADR 0006 (Phase 1 reads with Dapper)

## Context
ADR 0006 planned Phase 1 queries, including the monthly summary, as Dapper SQL over SQL Server.
The monthly summary is mostly derived numbers: spent, remaining and % used per jar and per planned
item, unassigned income and share of income. Those rules already live in the domain
(`MonthlyBudget.StatusOf`, `ShareOfIncome`) and are unit-tested there. Re-implementing them as SQL
aggregates would duplicate business rules in a second language, where they can drift apart, and
would break the principle that the domain computes every number (ADR 0003).

## Decision
- Queries whose numbers come from domain rules (e.g. the monthly summary) **load the aggregate
  through its repository port and let the domain compute**; the handler maps the results to DTOs.
- Dapper stays for list-style reads that only filter and page stored data (e.g. an expense history).
- From Phase 2, the MongoDB projector builds screen documents with the same domain logic, so the
  read models never re-implement the rules either.

## Alternatives considered
- **Dapper SQL for the summary (ADR 0006 as written):** fast and simple to query, but duplicates the
  spending rules in SQL and needs its own tests for the same behavior.
- **Store computed totals on write:** avoids recomputation but adds denormalized state to keep in sync.

## Consequences
- One tested implementation of every budget number.
- The summary loads one month's budget with its expenses: a small aggregate (one user, one month),
  so the cost is negligible at this scale. Revisit if a month's expenses grow into the thousands.
- The roadmap item "Dapper for the monthly summary query" becomes "EF Core for writes and reads of
  the monthly budget".
