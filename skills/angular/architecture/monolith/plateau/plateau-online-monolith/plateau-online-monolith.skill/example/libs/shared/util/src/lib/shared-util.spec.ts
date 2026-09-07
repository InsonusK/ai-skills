import { isNonEmpty, positive } from './shared-util';

describe('shared-util', () => {
  it('isNonEmpty rejects null, undefined, and blank strings', () => {
    expect(isNonEmpty('x')).toBe(true);
    expect(isNonEmpty('  ')).toBe(false);
    expect(isNonEmpty(null)).toBe(false);
  });
  it('positive accepts finite positive numbers only', () => {
    expect(positive(1)).toBe(true);
    expect(positive(0)).toBe(false);
    expect(positive(Number.NaN)).toBe(false);
  });
});
