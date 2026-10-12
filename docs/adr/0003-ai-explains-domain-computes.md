# 0003 — AI explains, the domain computes; MCP first

- **Status:** Accepted
- **Date:** 2026-10-12

## Context
LLMs are unreliable at arithmetic, and financial figures must be exact and auditable.
Paying for LLM calls is also undesirable for a portfolio project.

## Decision
- Every number (allocations, drift, returns, payoff schedules) is computed by the domain.
- The LLM only receives computed results and produces explanations and suggestions.
- AI capability is exposed primarily through an **MCP server**: the user's own AI client
  (Claude, VS Code, etc.) does the reasoning, at no cost to the application.
- In-app insights use `Microsoft.Extensions.AI` `IChatClient` behind an Application port,
  with Ollama locally. The provider is configuration.
- All AI output is labeled as informational, not financial advice.

## Consequences
- Insights are traceable to deterministic, unit-tested calculations.
- The app works fully without any LLM; AI is an enhancement.
- Exposing financial data over MCP requires authentication before any public deployment.
