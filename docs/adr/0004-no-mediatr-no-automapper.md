# 0004 — No MediatR, no AutoMapper

- **Status:** Accepted
- **Date:** 2026-10-12

## Context
MediatR and AutoMapper moved to commercial licensing in 2025. Both also add indirection
that a small codebase does not need.

## Decision
- Use plain handler interfaces (`ICommandHandler<,>`, `IQueryHandler<,>`) resolved through DI.
- Cross-cutting concerns (logging, validation) via decorators where needed.
- Map between layers by hand.

## Consequences
- No license risk and no hidden magic: the call path from endpoint to handler is explicit.
- Slightly more boilerplate, mitigated by keeping handlers small.
