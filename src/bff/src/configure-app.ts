import { INestApplication } from '@nestjs/common';

/** Settings shared by main.ts and the e2e tests, so tests exercise the real routes. */
export function configureApp(app: INestApplication): void {
  // The browser reaches the BFF under /api (Angular dev proxy locally, Static Web Apps in Azure).
  app.setGlobalPrefix('api');
}
