import { defaultFrontendConfig } from './frontend-config';

describe('defaultFrontendConfig', () => {
  it('keeps UI constants aligned with the backend and Aspire proxy', () => {
    expect(defaultFrontendConfig.apiBaseUrl).toBe('/api');
    expect(defaultFrontendConfig.expenseDescriptionMaxLength).toBe(50);
    expect(defaultFrontendConfig.dateLocale).toBe('fr-FR');
  });
});
