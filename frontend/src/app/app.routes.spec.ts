import { appRoutes } from './app.routes';

describe('appRoutes', () => {
  it('defines the primary frontend routes', () => {
    const paths = appRoutes.map((route) => route.path);

    expect(paths).toEqual(['reports', 'reports/:id', 'users', '', '**']);
  });
});
