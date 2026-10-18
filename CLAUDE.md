# BudgetLens — Personal Budget & Investment Tracker

## Purpose
Portfolio project. Users set their monthly **net income**, split it into budget
jars by amount (the app derives the percentages), record expenses, track savings goals, debts and
investments (CDTs, neobank savings pockets), and get optimization insights.
An Angular SPA talks to a NestJS BFF over REST; the BFF talks to the .NET API over gRPC. An MCP server
exposes the same use cases so any AI client can analyze the user's finances.

## Working agreement
- Talk to the developer in **English** (they may ask in Spanish; answer in English unless asked otherwise).
  Code, comments, commits and docs in **English**. End-user UI text is bilingual (see Conventions).
- **Plan first.** For any task touching more than one project, propose a plan and wait for approval.
- **Small steps.** One feature per branch. Commit after each green step using Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `chore:`, `refactor:`).
- **Review before commit.** Never commit, push or open a PR without the developer's explicit approval. First show
  the changed files and the proposed commit message(s) and PR title/description, then wait for the go-ahead.
- **Tests are part of done.** Every domain rule gets a unit test. Never mark a task done with failing tests.
- **Record decisions.** Any architectural decision → new ADR in `docs/adr/` (next number, same format as 0001).
- Prefer editing over rewriting files. Keep solutions simple. No speculative abstractions.
- If unsure about a library API or version, check the official docs before writing code.

## Architecture (hexagonal)
```
protos/                      gRPC contracts (.proto). Single source of truth for BFF ↔ Api.
src/
  BudgetLens.Domain           Entities, value objects, domain services. No dependencies.
  BudgetLens.Application      Use cases (commands/queries + handlers) and ports. Depends on Domain only.
  BudgetLens.Infrastructure   Driven adapters: EF Core + SQL Server (writes), Dapper / MongoDB (reads),
                             outbox + projector, AI client, clock.
  BudgetLens.Api              Driving adapter: gRPC services. Composition root. Hosts MCP at /mcp.
  BudgetLens.Mcp              Driving adapter: MCP tools that call Application use cases.
  BudgetLens.AppHost          Aspire orchestration for local development.
  BudgetLens.ServiceDefaults  OpenTelemetry, health checks, resilience.
  bff/                       NestJS Backend-for-Frontend (TypeScript).
  web/                       Angular SPA.
tests/
  BudgetLens.Domain.Tests
  BudgetLens.Application.Tests
  BudgetLens.Api.IntegrationTests   (Testcontainers: SQL Server, MongoDB)
  BudgetLens.ArchitectureTests      (enforces the rules below)
```

Request flow:
- **Web:** Angular → (REST/JSON) → NestJS BFF → (gRPC) → .NET Api. Angular never calls the Api directly.
- **AI:** MCP clients → .NET Api at `/mcp`. MCP does not go through the BFF.

Dependency rules — enforced by `BudgetLens.ArchitectureTests`:
- Domain references nothing.
- Application references Domain only.
- Api and Mcp call Application use cases only. Infrastructure is wired exclusively in the Api composition root.

## BFF rules (see ADR 0005)
- The BFF **aggregates and shapes** data for screens. It holds **no business rules** and no direct database access.
- One endpoint per screen need (e.g. `GET /dashboard` composes budget, goals, investments and insights).
- Once auth exists, the BFF owns the session (HTTP-only cookie); tokens never reach the browser.
- Api contract types are generated from `protos/` with ts-proto — never hand-copied.
- NestJS: modules per feature, DTOs with class-validator, gRPC clients via `@nestjs/microservices`.
- Map gRPC status codes to HTTP `ProblemDetails`-style responses (e.g. `NOT_FOUND` → 404, `INVALID_ARGUMENT` → 400).

## gRPC rules (see ADR 0007)
- **Contract first.** Change the `.proto`, then regenerate both sides. Never edit generated code.
- Proto packages are versioned: `budgetlens.v1`. Never reuse or renumber fields; mark removed ones `reserved`.
- **Money never as `double`/`float` in Protobuf.** Use the shared `DecimalValue { int64 units; sfixed32 nanos; }`
  message plus a currency code. Conversions live in one tested mapper on each side.
- Dates: `google.protobuf.Timestamp` for instants, `google.type.Date` (or ISO string) for calendar dates.
- Every client call sets a **deadline**. Errors use proper status codes, never OK-with-error-payload.
- gRPC health checks (`grpc.health.v1`) and server reflection enabled in Development only.
- Kestrel serves HTTP/2; internal calls may use h2c (HTTP/2 without TLS) behind the platform ingress.
  Without TLS, HTTP/2 cannot be negotiated: locally gRPC gets an HTTP/2-only endpoint and MCP/health
  checks an HTTP/1.1 endpoint.
- ts-proto runs with `forceLong=string` so 64-bit integers never lose precision in JavaScript.

## CQRS and persistence (see ADR 0006)
- **SQL Server is the source of truth.** Commands go through Application handlers and persist
  aggregates via EF Core. Money needs ACID.
- **Phase 1 queries** read from SQL Server with Dapper and return DTOs directly.
- **From Phase 2, screen read models live in MongoDB**: one denormalized document per view
  (e.g. monthly dashboard). They are fed by a transactional **outbox** in SQL Server and a projector
  (`BackgroundService`). Read models are disposable: they must be fully rebuildable from SQL Server.
- Never write the same business data to both stores from a handler. SQL first; Mongo only via projection.
- **No MediatR, no AutoMapper** (commercial licenses since 2025, see ADR 0004). Use plain
  `ICommandHandler<TCommand, TResult>` / `IQueryHandler<TQuery, TResult>` registered in DI. Map by hand.

## Domain rules
- **Money is `decimal`, never `double`/`float`.** `Money` value object (Amount, Currency). Default currency COP.
  Rounding is always explicit. Rate math may use `double` internally for `Math.Pow`, converted back to `decimal` with an explicit rounding rule.
- **Percentage** value object (0–100).
- **Monthly budget** = the month's net income plus jars. A jar has one or more planned items
  (e.g. Fixed expenses: rent, debts, utilities, gym, cellphone); its amount is the sum of its items.
  Users type amounts; percentages are always derived (amount / income), never stored.
  The total planned may not exceed the income; the difference is shown as *unassigned*.
- **Monthly net income** is recorded per month. A new month is pre-filled with the previous month's
  income and jars; when the income changes, the user enters the new distribution (no automatic redistribution).
- **Six jars preset** (the only preset): Necessities 55, Financial freedom 10, Long-term savings 10,
  Education 10, Play 10, Give 5. It is a suggestion only: it turns its percentages into suggested amounts
  for the user's income, which the user then edits, and compares the user's percentages with its own
  (never enforced). Jars link to a suggestion by a stable key, not by their (translatable) name.
  Users can skip it and create their own jars. Extract a Strategy only if a second preset is added.
- **Interest rates** are entered as effective annual (E.A.). Monthly rate = (1 + EA)^(1/12) − 1.
- **Never hardcode** market rates, tax rates or withholding percentages. They are always user input.
- **Investments**: principal, E.A. rate, start date, term (CDT) or open-ended (neobank pocket),
  expected return vs. actual return registered by the user, optional withholding %.
- **Debts**: balance, rate, minimum payment. Payoff strategies: avalanche and snowball (Strategy pattern).

## AI principles
- **The LLM never calculates.** The domain computes every number; the LLM only explains and suggests
  based on already-computed results.
- AI is a port (`IFinancialInsights`) in Application; the adapter in Infrastructure uses
  `Microsoft.Extensions.AI` `IChatClient`. Local provider: Ollama (free). Provider is configuration, not code.
- Outputs are informational, not financial advice. Say so in the UI and in MCP tool descriptions.
- MCP tools are read-mostly: summaries, jar status, goal progress, investment performance,
  what-if simulations. Any write tool requires explicit confirmation.

## Commands
- Run everything (Api, SQL Server, MongoDB, BFF, front, dashboard): `aspire run`
- Build: `dotnet build`
- Test: `dotnet test`
- BFF only: `cd src/bff; npm run start:dev` · BFF tests: `npm test`
- Front only: `cd src/web; npm start`

## Conventions
- .NET 10. Nullable enabled, warnings as errors, file-scoped namespaces.
- Central Package Management (`Directory.Packages.props`), shared settings in `Directory.Build.props`.
- gRPC services grouped by feature (one service per bounded area: Budget, Goals, Investments, Debts).
- Angular: standalone components, signals, Angular Material. Keep the UI simple and clean.
- UI is bilingual (see ADR 0008): English (`en`) and Colombian Spanish (`es-CO`), detected from the browser,
  English as fallback. Use Transloco; never hardcode user-facing strings in templates or components.
- Format money, numbers, percentages and dates only through Angular pipes with the active locale.
- Keep Colombian finance terms (CDT, E.A.) untranslated, with an English tooltip.
- Tests: xUnit. Integration tests with Testcontainers. Architecture tests with ArchUnitNET.

## Glossary (Colombian finance)
- **CDT** — Certificado de Depósito a Término (term deposit).
- **E.A.** — Efectiva Anual (effective annual rate).
- **Neobank pocket** — daily-interest savings pocket (e.g. "cajitas" in Nu).
- **Retención en la fuente** — withholding tax on yields.
- **Salario neto** — gross salary minus payroll deductions.
