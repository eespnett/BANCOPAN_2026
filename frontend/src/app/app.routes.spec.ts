import { routes } from './app.routes';

describe('routes', () => {
  it('should define root route and wildcard redirect', () => {
    expect(routes.length).toBeGreaterThanOrEqual(2);
    expect(routes[0].path).toBe('');
    expect(routes[1].path).toBe('**');
    expect((routes[1] as any).redirectTo).toBe('');
  });
});
