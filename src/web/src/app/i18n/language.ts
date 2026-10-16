/** UI languages supported by BudgetLens (see ADR 0008). */
export const AVAILABLE_LANGS = ['en', 'es-CO'] as const;

export type AppLang = (typeof AVAILABLE_LANGS)[number];

export const DEFAULT_LANG: AppLang = 'en';

/** Maps a browser language tag (e.g. `navigator.language`) to a supported UI language. */
export function resolveLang(browserLang: string | undefined): AppLang {
  return browserLang?.toLowerCase().startsWith('es') ? 'es-CO' : DEFAULT_LANG;
}
