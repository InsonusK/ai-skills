export const isNonEmpty = (v: string | null | undefined): v is string =>
  typeof v === 'string' && v.trim().length > 0;

export const positive = (n: number): boolean => Number.isFinite(n) && n > 0;
