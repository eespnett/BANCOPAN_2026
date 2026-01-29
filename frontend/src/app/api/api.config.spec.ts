import { API_BASE_URL } from './api.config';

describe('API_BASE_URL', () => {
  it('should point to backend localhost', () => {
    expect(API_BASE_URL).toContain('localhost');
    expect(API_BASE_URL).toContain('5284');
  });
});
