export interface CheckResult {
  isValid: boolean;
  normalized?: string;
  errorCode?: string;
}

export function checkLink(rawUrl: string): CheckResult {
  const trimmed = rawUrl.trim();
  const match = /^([a-zA-Z][a-zA-Z0-9+.-]*):\/\/([^/]*)(.*)$/.exec(trimmed);
  const scheme = match ? match[1].toLowerCase() : "";
  if (scheme !== "http" && scheme !== "https") {
    return { isValid: false, errorCode: "UNSUPPORTED_SCHEME" };
  }
  if (!match![2]) {
    return { isValid: false, errorCode: "MISSING_HOST" };
  }
  return { isValid: true, normalized: `${scheme}://${match![2].toLowerCase()}${match![3]}` };
}
