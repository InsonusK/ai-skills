import {Result} from './checker';
export class RecordError extends Error { constructor(public code: string) { super(code); } }
export function toRecord(result: Result): Record<string, unknown> { return {...result}; }
export function fromRecord(record: Record<string, unknown>): Result {
  if (!('is_valid' in record)) throw new RecordError('MISSING_FIELD');
  return {is_valid:Boolean(record.is_valid), normalized:String(record.normalized ?? ''), error_code:String(record.error_code ?? '')};
}
