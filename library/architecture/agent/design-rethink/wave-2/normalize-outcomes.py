#!/usr/bin/env python3
"""Wave-2 source migration of transport diagnostics into generic causes."""
from pathlib import Path
import re
ROOT=Path(__file__).resolve().parents[5]
for stack in ('go','python','dotnet'):
 for name in ('vp-http-client','vp-grpc-client','vp-client-retry'):
  folder=ROOT/f'library/architecture/{stack}/{name}-in-{stack}.skill'
  for p in folder.rglob('*'):
   if not p.is_file() or any(x in p.parts for x in ('bin','obj','.venv','__pycache__')):continue
   if p.suffix not in ('.go','.py','.cs'):continue
   s=p.read_text(); before=s
   if stack=='go':
    if p.name=='outcome.go':
     s=s.replace('// HTTPStatus is the HTTP status of the answer; 0 over gRPC.\n\tHTTPStatus int', '// Cause retains transport diagnostics without exposing a transport field.\n\tCause error')
     s=re.sub(r'func \(e \*StatusError\) Error\(\) string \{[\s\S]*\Z', '''func (e *StatusError) Error() string {
    if e.Cause != nil {
        return fmt.Sprintf("api answered %s: %s (%v)", e.Code, e.Message, e.Cause)
    }
    return fmt.Sprintf("api answered %s: %s", e.Code, e.Message)
}

func (e *StatusError) Unwrap() error { return e.Cause }
''',s)
    s=s.replace('HTTPStatus: status', 'Cause: fmt.Errorf("HTTP status %d", status)')
    s=s.replace('got.HTTPStatus != status', '!strings.Contains(fmt.Sprint(errors.Unwrap(got)), fmt.Sprintf("HTTP status %d", status))')
    if '/client/todoapi/test/' in str(p):s=s.replace('../../../../../','../../../../')
   elif stack=='python':
    if p.name=='outcomes.py':
     s=s.replace('http_status: int | None = None', 'cause: Exception | None = None')
     s=s.replace('        answered = f"{http_status}, " if http_status is not None else ""\n','')
     s=s.replace('{api} answered {answered}{code.name}', '{api} answered {code.name}')
     s=s.replace('        self.http_status = http_status\n        """The HTTP status of the answer; None over gRPC."""', '        self.__cause__ = cause\n        """Optional protocol diagnostics; not part of the logical API status."""')
    if p.name=='client.py' and '/httpclient/' in str(p):
     s=s.replace('str(parsed.get("message", "")), status)', 'str(parsed.get("message", "")), RuntimeError(f"HTTP status {status}"))')
     s=s.replace('StatusCode.UNKNOWN, "", status)', 'StatusCode.UNKNOWN, "", RuntimeError(f"HTTP status {status}"))')
    s=s.replace('assert (world.error.http_status, world.error.code, world.error.message) == (status, expected, message)', 'assert f"HTTP status {status}" in str(world.error.__cause__)\n    assert (world.error.code, world.error.message) == (expected, message)')
    if '/todo_api/test/' in str(p):
     s=re.sub(r'\bItem\b','ItemDTO',s)
     s=s.replace('Path(__file__).parents[6]', 'Path(__file__).parents[5]')
   else:
    if p.name=='ApiOutcomes.cs':
     s=s.replace('int? httpStatus = null', 'Exception? innerException = null')
     s=s.replace(': Exception($"{api} answered {(httpStatus is { } status ? $"{status}, " : "")}{code}: {statusMessage}")', ': Exception($"{api} answered {code}: {statusMessage}", innerException)')
     s=s.replace('    /// <summary>The HTTP status of the answer; null over gRPC.</summary>\n    public int? HttpStatus { get; } = httpStatus;\n','')
    if p.name=='HttpApiClient.cs':
     s=s.replace('message, status)', 'message, new HttpRequestException($"HTTP status {status}", null, (HttpStatusCode)status))').replace('ApiStatusCode.Unknown, "", status)', 'ApiStatusCode.Unknown, "", new HttpRequestException($"HTTP status {status}", null, (HttpStatusCode)status))')
    s=s.replace('failed.HttpStatus!.Value', '(int)Assert.IsType<HttpRequestException>(failed.InnerException).StatusCode!')
   if s!=before:p.write_text(s)
print('Common outcomes now expose generic causes; moved test paths/DTO imports repaired.')
