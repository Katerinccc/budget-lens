import { CurrencyPipe, DOCUMENT, DecimalPipe, PercentPipe } from '@angular/common';
import { DEFAULT_CURRENCY_CODE, LOCALE_ID, inject } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { AppLang } from './language';
import { provideI18n } from './provide-i18n';

// Locale formats may use non-breaking spaces; compare with plain spaces.
const plain = (text: string | null) => text?.replace(/\s/g, ' ');

function setUp(lang: AppLang) {
  TestBed.configureTestingModule({ providers: provideI18n(lang) });
  return TestBed.runInInjectionContext(() => ({
    currency: new CurrencyPipe(inject(LOCALE_ID), inject(DEFAULT_CURRENCY_CODE)),
    number: new DecimalPipe(inject(LOCALE_ID)),
    percent: new PercentPipe(inject(LOCALE_ID)),
    document: inject(DOCUMENT),
  }));
}

describe('provideI18n', () => {
  it('formats money, numbers and percentages for Colombian Spanish', () => {
    const { currency, number, percent } = setUp('es-CO');

    expect(plain(currency.transform(1234567.5))).toBe('$ 1.234.567,50');
    expect(number.transform(1234567.891)).toBe('1.234.567,891');
    expect(percent.transform(0.125, '1.0-2')).toBe('12,5%');
  });

  it('formats money, numbers and percentages for English, defaulting to COP', () => {
    const { currency, number, percent } = setUp('en');

    expect(currency.transform(1234567.5)).toBe('COP1,234,567.50');
    expect(number.transform(1234567.891)).toBe('1,234,567.891');
    expect(percent.transform(0.125, '1.0-2')).toBe('12.5%');
  });

  it('sets the document language', () => {
    const { document } = setUp('es-CO');

    expect(document.documentElement.lang).toBe('es-CO');
  });
});
