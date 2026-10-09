import { check } from './checker';
export function main(args: string[], out: (line: string) => void, err: (line: string) => void): number {
  if (!args.length) {
    err('usage: linkcheck URL [URL ...]');
    return 2;
  }
  let code = 0;
  for (const url of args) {
    const result = check(url);
    if (result.is_valid) out(`ok ${result.normalized}`);
    else {
      out(`invalid ${url} ${result.error_code}`);
      code = 1;
    }
  }
  return code;
}
