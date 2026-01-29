import { appConfig } from './app.config';

describe('appConfig', () => {
  it('should have providers configured', () => {
    expect(appConfig.providers?.length).toBeGreaterThan(0);
  });
});
