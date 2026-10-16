import { TestBed } from '@angular/core/testing';
import { TranslocoTestingModule } from '@jsverse/transloco';

import en from '../../public/i18n/en.json';
import esCo from '../../public/i18n/es-CO.json';
import { App } from './app';
import { AVAILABLE_LANGS, AppLang } from './i18n/language';

async function render(lang: AppLang) {
  await TestBed.configureTestingModule({
    imports: [
      App,
      TranslocoTestingModule.forRoot({
        langs: { en, 'es-CO': esCo },
        translocoConfig: { availableLangs: [...AVAILABLE_LANGS], defaultLang: lang },
        preloadLangs: true,
      }),
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(App);
  await fixture.whenStable();
  return fixture.nativeElement as HTMLElement;
}

describe('App', () => {
  it('renders the shell in English', async () => {
    const page = await render('en');

    expect(page.querySelector('h1')?.textContent).toBe('BudgetLens');
    expect(page.querySelector('p')?.textContent).toBe(en.app.tagline);
  });

  it('renders the shell in Colombian Spanish', async () => {
    const page = await render('es-CO');

    expect(page.querySelector('p')?.textContent).toBe(esCo.app.tagline);
  });
});
