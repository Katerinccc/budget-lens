import { resolveLang } from './language';

describe('resolveLang', () => {
  it.each([
    ['es-CO', 'es-CO'],
    ['es', 'es-CO'],
    ['es-MX', 'es-CO'],
    ['ES-es', 'es-CO'],
    ['en-US', 'en'],
    ['fr-FR', 'en'],
    [undefined, 'en'],
  ])('maps browser language %s to %s', (browserLang, expected) => {
    expect(resolveLang(browserLang)).toBe(expected);
  });
});
