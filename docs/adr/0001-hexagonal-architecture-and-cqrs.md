# 0001 — Hexagonal architecture with CQRS

- **Status:** Accepted
- **Date:** 2026-10-12

## Context
The application has two kinds of clients from the start: a web front end (through a BFF
over gRPC) and AI assistants (MCP). Both need the same business rules. Financial calculations must be
testable in isolation and independent of frameworks.

## Decision
- Hexagonal architecture: Domain → Application (use cases + ports) → adapters.
- gRPC services and the MCP server are **two driving adapters over the same use cases**.
- CQRS: commands persist aggregates with EF Core; queries return DTOs from a separate read
  path (Dapper first, MongoDB read models later — see ADR 0006).
- Dependency rules are enforced by automated architecture tests.

## Consequences
- Adding a new client (MCP, a future mobile app) does not touch the domain.
- Read models can be optimized with raw SQL without compromising the write model.
- More projects than a single-project API; acceptable for a system meant to grow.
