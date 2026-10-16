import en from '../../../public/i18n/en.json';
import esCo from '../../../public/i18n/es-CO.json';

type Tree = { [key: string]: string | Tree };

function keys(tree: Tree, prefix = ''): string[] {
  return Object.entries(tree).flatMap(([key, value]) =>
    typeof value === 'string' ? [prefix + key] : keys(value, `${prefix}${key}.`),
  );
}

describe('translation files', () => {
  it('es-CO has exactly the same keys as en', () => {
    expect(keys(esCo).sort()).toEqual(keys(en).sort());
  });
});
