// Centralizes UI-level configuration used by the Angular frontend.
import { InjectionToken } from '@angular/core';

export interface FrontendConfig {
  readonly apiBaseUrl: string;
  readonly expenseDescriptionMaxLength: number;
  readonly dateLocale: string;
}

export const FRONTEND_CONFIG = new InjectionToken<FrontendConfig>('FRONTEND_CONFIG');

export const defaultFrontendConfig: FrontendConfig = {
  apiBaseUrl: '/api',
  expenseDescriptionMaxLength: 50,
  dateLocale: 'fr-FR',
};
