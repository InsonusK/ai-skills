import { DS_TENANTS } from './tenants';

describe('DS_TENANTS', () => {
  it('is the frozen list of shipped tenant ids', () => {
    expect([...DS_TENANTS]).toEqual(['acme', 'globex']);
  });

  it('every id is a lowercase kebab string (matches its _<id>.scss file + [data-tenant] selector)', () => {
    for (const id of DS_TENANTS) {
      expect(id).toMatch(/^[a-z][a-z0-9-]*$/);
    }
  });
});
