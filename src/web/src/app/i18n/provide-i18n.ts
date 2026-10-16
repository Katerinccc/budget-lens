import { DOCUMENT, registerLocaleData } from '@angular/common';
import localeEsCo from '@angular/common/locales/es-CO';
import { provideHttpClient } from '@angular/common/http';
import {
  DEFAULT_CURRENCY_CODE,
  EnvironmentProviders,
  LOCALE_ID,
  Provider,
  inject,
  isDevMode,
  provideAppInitializer,
} from '@angular/core';
import { provideTransloco } from '@jsverse/transloco';

import { AVAILABLE_LANGS, AppLang, DEFAULT_LANG } from './language';
import { TranslocoHttpLoader } from './transloco-loader';

// Number, currency, percent and date formats for Colombian Spanish ('en' is built in).
registerLocaleData(localeEsCo);

/**
 * Translations (Transloco) and formatting locale (Angular pipes) for the given language.
 * Money defaults to COP; English is the fallback for any missing translation key.
 */
export function provideI18n(lang: AppLang): (Provider | EnvironmentProviders)[] {
  return [
    provideHttpClient(),
    provideTransloco({
      config: {
        availableLangs: [...AVAILABLE_LANGS],
        defaultLang: lang,
        fallbackLang: DEFAULT_LANG,
        missingHandler: { useFallbackTranslation: true },
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader,
    }),
    { provide: LOCALE_ID, useValue: lang },
    { provide: DEFAULT_CURRENCY_CODE, useValue: 'COP' },
    // Screen readers and the browser's spell checker use <html lang>.
    provideAppInitializer(() => {
      inject(DOCUMENT).documentElement.lang = lang;
    }),
  ];
}
