# 0002 — Local-first development with Aspire, Azure on free tiers

- **Status:** Accepted
- **Date:** 2026-10-12

## Context
The project must demonstrate Azure skills without incurring ongoing cost. Azure
pay-as-you-go has no spending cap once free limits are exceeded.

## Decision
- Develop locally with **Aspire**: SQL Server, the API and the Angular app run as one
  orchestrated system with an observability dashboard. No cloud resources needed to develop.
- Deploy only to services with an always-free allowance: Azure Container Apps,
  Azure SQL Database free offer, Static Web Apps free plan, Log Analytics free ingestion.
- Container images on GitHub Container Registry instead of Azure Container Registry
  (which has no free tier).
- Infrastructure as code with Bicep, generated and deployed through the Aspire/azd tooling.
- A budget alert is configured before any resource is created.

## Consequences
- Anyone can clone and run the project with Docker and the .NET SDK.
- Services with only a 12-month free period (e.g. Azure Service Bus Standard) are optional
  and replaceable; the design does not depend on them.
- Free-tier limits may pause the database under load; acceptable for a demo.
