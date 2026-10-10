import { check } from '../checker';
export function summarize(urls: Iterable<string>) {
  const summary = { valid: 0, invalid: 0, errors: {} as Record<string, number> };
  for (const url of urls) {
    const result = check(url);
    if (result.is_valid) summary.valid++;
    else {
      summary.invalid++;
      summary.errors[result.error_code] = (summary.errors[result.error_code] ?? 0) + 1;
    }
  }
  return summary;
}
