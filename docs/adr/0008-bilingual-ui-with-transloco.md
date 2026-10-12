# 0008 — Bilingual UI (English and Colombian Spanish) with Transloco

- **Status:** Accepted
- **Date:** 2026-10-12

## Context
BudgetLens targets Colombian users (COP, CDTs, E.A. rates) and is also a portfolio piece
reviewed by an international audience. The UI must therefore read naturally in both
Colombian Spanish and English. Money, numbers, percentages and dates must follow the
active locale (e.g. `$ 1.234.567` in `es-CO` vs. `COP 1,234,567` in `en`).

Angular offers two approaches: the built-in `@angular/localize` (compile-time, one build
per locale) and runtime libraries such as Transloco.

## Decision
- Supported UI languages: `en` and `es-CO`. The language is detected from the browser
  (any `es-*` maps to `es-CO`); English is the fallback.
- Use **Transloco** for runtime translation, with one JSON file per language in
  `src/web/public/i18n/`. User-facing strings are never hardcoded in templates or components.
- Register Angular locale data for `es-CO` and format money, numbers, percentages and
  dates only through Angular pipes with the active locale.
- Colombian finance terms (CDT, E.A.) stay untranslated in both languages and carry an
  English explanatory tooltip.
- The backend (Api, BFF) returns data and error codes, not translated text; translation
  happens only in the SPA.

## Alternatives considered
- **`@angular/localize`:** official and zero-runtime-cost, but requires one build and
  deployment per locale and a page reload to switch language. Too heavy for a single
  Static Web App deployment with a language toggle.
- **ngx-translate:** similar runtime model; Transloco is more actively maintained and has
  better tooling (missing-key detection, scoped translations, testing helpers).

## Consequences
- One build serves both languages, and the language can switch without reloading.
- Every new screen needs keys in both JSON files; a missing key falls back to English.
- Error messages from the BFF must carry stable codes that the SPA maps to translations.
