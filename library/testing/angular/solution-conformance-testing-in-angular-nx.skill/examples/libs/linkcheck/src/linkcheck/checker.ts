export interface Result {
  is_valid: boolean;
  normalized: string;
  error_code: string;
}
export function check(raw: string): Result {
  const text = raw.trim();
  const parts = /^([a-z][a-z0-9+.-]*):\/\/([^/?#]*)([^?#]*)/i.exec(text);
  const scheme = text.split(':')[0].toLowerCase();
  if (!['http', 'https'].includes(scheme))
    return { is_valid: false, normalized: '', error_code: 'UNSUPPORTED_SCHEME' };
  if (!parts || !parts[2]) return { is_valid: false, normalized: '', error_code: 'MISSING_HOST' };
  if (parts[2].includes('@'))
    return { is_valid: false, normalized: '', error_code: 'CREDENTIALS_NOT_ALLOWED' };
  return { is_valid: true, normalized: `${scheme}://${parts[2].toLowerCase()}${parts[3]}`, error_code: '' };
}
