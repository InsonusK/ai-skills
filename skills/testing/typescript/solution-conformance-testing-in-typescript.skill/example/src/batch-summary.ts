import { checkLink } from "./link-validator";

export interface BatchSummary {
  valid: number;
  invalid: number;
  errors: Record<string, number>;
}

/** Check every URL and count the valid ones, the invalid ones, and each error code. */
export function summarizeBatch(urls: string[]): BatchSummary {
  const summary: BatchSummary = { valid: 0, invalid: 0, errors: {} };
  for (const url of urls) {
    const result = checkLink(url);
    if (result.isValid) {
      summary.valid += 1;
    } else {
      summary.invalid += 1;
      const code = result.errorCode ?? "UNKNOWN";
      summary.errors[code] = (summary.errors[code] ?? 0) + 1;
    }
  }
  return summary;
}
