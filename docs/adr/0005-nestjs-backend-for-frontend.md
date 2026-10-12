# 0005 — NestJS Backend-for-Frontend

- **Status:** Accepted
- **Date:** 2026-10-12

## Context
The Angular dashboard needs data from several use cases at once: budget status, savings
goals, investment performance and AI insights. Calling each from the browser means
multiple round trips and UI-specific composition logic living in the SPA.

Authentication is planned. For browser-based applications, keeping OAuth tokens out of
the browser is the recommended approach, and the Backend-for-Frontend pattern is the
standard way to achieve it.

The .NET Api exposes business use cases over gRPC (ADR 0007) and is also consumed by MCP
clients, so its contract should not be shaped around one particular UI. Browsers cannot call
gRPC services natively.

## Decision
- Introduce a **NestJS BFF** between Angular and the .NET Api. Angular talks only to the BFF.
- The BFF exposes REST/JSON to the browser and calls the Api over gRPC, translating
  between both.
- The BFF aggregates and shapes responses per screen; it contains no business rules and
  has no database access.
- When authentication arrives, the BFF owns the user session through an HTTP-only cookie
  and forwards requests to the Api with the access token server-side.
- Api contract types in the BFF are generated from the shared `.proto` files.
- NestJS is chosen over a .NET alternative (e.g. YARP) because it is the BFF technology
  used in production by the author, and TypeScript end-to-end on the web side keeps
  front-end and BFF models close.

## Alternatives considered
- **Angular calling the Api directly:** simpler, but pushes composition into the browser,
  couples the Api contract to the UI and exposes tokens to the browser once auth exists.
- **YARP-based BFF in .NET:** valid and keeps a single backend language; not chosen for the
  reasons above.

## Consequences
- One extra network hop and one more deployable to run and monitor.
- The Api stays UI-agnostic and serves both the BFF and MCP clients unchanged.
- Contract drift between BFF and Api is caught at build time through generated types.
- The BFF is the protocol boundary: REST outside, gRPC inside.
