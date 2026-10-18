# Roadmap

Each phase ends with something working, tested and pushed. Never start a phase before
the previous one is green in CI.

## Phase 0 — Foundations
Goal: an empty but professional repository that builds and tests in CI.

Scaffold (PowerShell, from the repo root):
```powershell
dotnet new install Aspire.ProjectTemplates
dotnet new sln -n BudgetLens
dotnet new classlib -n BudgetLens.Domain         -o src/BudgetLens.Domain
dotnet new classlib -n BudgetLens.Application    -o src/BudgetLens.Application
dotnet new classlib -n BudgetLens.Infrastructure -o src/BudgetLens.Infrastructure
dotnet new classlib -n BudgetLens.Mcp            -o src/BudgetLens.Mcp
dotnet new grpc     -n BudgetLens.Api            -o src/BudgetLens.Api
dotnet new aspire-apphost         -n BudgetLens.AppHost         -o src/BudgetLens.AppHost
dotnet new aspire-servicedefaults -n BudgetLens.ServiceDefaults -o src/BudgetLens.ServiceDefaults
dotnet new xunit -n BudgetLens.Domain.Tests       -o tests/BudgetLens.Domain.Tests
dotnet new xunit -n BudgetLens.Application.Tests  -o tests/BudgetLens.Application.Tests
dotnet new xunit -n BudgetLens.ArchitectureTests  -o tests/BudgetLens.ArchitectureTests
dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj).FullName
npx @angular/cli@latest new web --directory src/web --style=scss --ssr=false --skip-git
npx @nestjs/cli@latest new bff --directory src/bff --package-manager npm --skip-git
```

Then:
- [x] `protos/` folder with `budgetlens/v1/common.proto` (`DecimalValue`, `Money`) referenced by the Api csproj
- [x] BFF generates TypeScript types from `protos/` with ts-proto (npm script)
- [x] Project references following the dependency rules in CLAUDE.md
- [x] `Directory.Build.props` (nullable, warnings as errors) and `Directory.Packages.props`
- [x] AppHost wires: SQL Server and MongoDB containers, Api, BFF, Angular app
- [x] Angular i18n foundation: Transloco with `en` + `es-CO` (browser detection, English fallback), `es-CO` locale data registered for pipes
- [x] Architecture tests enforcing the dependency rules
- [x] GitHub Actions: build + test (.NET, BFF and Angular) on every push and PR, badge in README
- [ ] *(Later, once an Anthropic API key with a spend limit exists)* Claude Code GitHub Action: on-demand `@claude` reviews and help in PRs/issues (Sonnet, `--max-turns`, job timeout)

## Phase 1 — Budget MVP
Goal: a user can plan a month's income across jars, register expenses and see drift per jar.
- [ ] Domain: `Money`, `Percentage`, `BudgetMonth`, `MonthlyBudget`, `Jar`, `PlannedItem`, `Expense`
- [ ] Plan presets via Strategy (suggested amounts the user edits; suggested limits compared, not enforced): personal method, Six Jars, 50/30/20
- [ ] Use cases: save monthly budget (income + jars, pre-filled from the previous month), register expense, monthly summary
- [ ] Deterministic alerts: jar spending over its planned amount, jar at 80% of its planned amount
- [ ] EF Core for writes, Dapper for the monthly summary query
- [ ] `budget.proto`: BudgetService (save monthly budget, register expense, monthly summary)
- [ ] Unit-tested `DecimalValue` ↔ `decimal` mapper (C#) and ↔ string/decimal mapper (TS)
- [ ] BFF: `GET /dashboard` aggregation endpoint calling the Api over gRPC with deadlines
- [ ] Angular (via BFF only): income + jar setup, expense entry, dashboard with jar progress, every string translated in `en` and `es-CO`
- [ ] Single-user demo mode (auth comes later)

## Phase 2 — Savings, investments and debts
Goal: show where money is working and where it is costing.
- [ ] Savings goals: target, deadline, required monthly contribution, progress
- [ ] Investments: CDT and neobank pocket, E.A. → monthly conversion, expected vs. actual return, withholding %
- [ ] Debts: avalanche vs. snowball simulation
- [ ] Dashboard: net worth over time
- [ ] Transactional outbox in SQL Server + projector `BackgroundService`
- [ ] MongoDB read model for the dashboard; rebuild-from-SQL command
- [ ] BFF dashboard switches to the MongoDB-backed query

## Phase 3 — AI and MCP
Goal: talk to your finances from Claude.
- [ ] `BudgetLens.Mcp` tools: budget summary, jar status, goal progress, investment performance, what-if allocation
- [ ] Host MCP in the Api with HTTP transport at `/mcp`
- [ ] Connect from Claude Code: `claude mcp add --transport http budget-lens http://localhost:<port>/mcp`
- [ ] `IFinancialInsights` port + `IChatClient` adapter with Ollama, shown as a panel in the dashboard
- [ ] Demo GIF in README

## Phase 4 — Azure
Goal: a live demo at zero (or near-zero) cost.
- [ ] Set a budget alert in Azure Cost Management **before** creating any resource
- [ ] Bicep infrastructure in `infra/` (via `azd` / `aspire deploy`)
- [ ] Api on Azure Container Apps, images on GitHub Container Registry
- [ ] Azure SQL Database free offer
- [ ] MongoDB: Atlas free cluster or Azure Cosmos DB free tier
- [ ] BFF on Azure Container Apps; Api with **internal** ingress and HTTP/2 transport for gRPC
- [ ] Angular on Azure Static Web Apps (free plan)
- [ ] Application Insights for telemetry
- [ ] Continuous deployment from GitHub Actions
- [ ] Authentication (Microsoft Entra External ID) with the session held by the BFF in an HTTP-only cookie
- [ ] Optional: Azure Service Bus between outbox and projector (emulator locally)
