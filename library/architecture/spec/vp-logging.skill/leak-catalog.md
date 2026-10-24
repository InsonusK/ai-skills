# Leak catalog of the logging variation point

Every way a defect of the logging variation point can put a secret into the log, or stop the log, and the shared scenario that proves an implementation has not got it. The scenarios are in [conformance/](./conformance/); every stack's implementation runs them unchanged (see [vp-logging](./vp-logging.skill.md)). A new form of payload, or a defect found in any stack, is added here first, then as a scenario, and so reaches every stack at once.

How the catalog is proven:
- **Every class has a scenario** — a reviewer reads this table against the scenarios; a class without one is a gap.
- **Every scenario can fail** — a mutation run over the stack's masking code kills every mutant, or the survivor is listed under its stack below with the reason it cannot leak.

## Masking a payload — [masking.feature](./conformance/masking.feature)

| Id | Defect that would leak | The mask's answer |
|---|---|---|
| L01 | A field marked secret is written, or only its scalar part is masked | Written `***`, whatever its value — a text, a number, an object, a list |
| L02 | A field marked not to log is written | Left out |
| L03 | A field the type leaves out of its JSON form (`json:"-"`, `[JsonIgnore]`) is written under its code name | Left out: the log shows no more than the type's JSON form |
| L04 | A secret nobody marked is written because the safety net does not know its spelling | A field, map key, or JSON key is masked when, lower-cased and without `_` and `-`, it contains a safety-net name |
| L05 | A key named like a secret holding an object or a list is walked, and its inner values written | The whole value is `***` |
| L06 | Marks are honoured at the top of the payload only | Marks and names hold at every depth: in objects, lists, and maps |
| L07 | A JSON body is written as the client sent it | Parsed and masked by its keys at every depth |
| L08 | A text field holding JSON (a stored document, a forwarded body) is written as it is | A text that parses as a JSON object or array is masked by its keys; any other text is written as it is |
| L09 | A body that is not JSON (a form, plain text, binary) is written as it is | **Fail-closed**: written `<unparsed body, N bytes>` and reported |
| L10 | An empty body is reported as unparsed | `""` — nothing to read, nothing to warn about |
| L11 | A proto field marked `debug_redact` is written, at the top or in a nested or repeated message | `***` at every depth |
| L12 | A proto map key or a field whose JSON name (`apiKey`) is a secret name is written | `***` |
| L13 | A proto `bytes` field is written as text | Like a body: parsed as JSON, else unparsed |
| L14 | A `google.protobuf.Any` is written as the raw bytes of its message, secrets included | Unpacked and masked like its message when its type is known (written with `@type`, as proto JSON does); unparsed otherwise |
| L15 | A header list (`Authorization: [Bearer …]`) is masked value by value, or not at all | The header is `***` with all its values |

## Writing a record — [payload-policy.feature](./conformance/payload-policy.feature)

| Id | Defect | The handler's answer |
|---|---|---|
| H01 | The payload reaches the line without the mask | Every payload is masked before it is written |
| H02 | A record of a call misses its request id | The request id of the call's context is on every record, the warnings too |
| H03 | `LOG_PAYLOAD` keeps a payload it should drop | `always`, `errors`, `off` per record outcome |
| H04 | The payload is cut before it is masked, so a secret near its start is written, or a body cut in the middle is no longer JSON and is written as text | Masked first, then cut, as JSON, at `LOG_PAYLOAD_MAX_BYTES`, saying how long the whole was |
| H05 | An unparsed body goes unnoticed, or floods the log, or raises its record's level | The record keeps its level; one WARN record `log payload not parsed` (record, field, bytes, request id — never the body) the first time a record of one message has one at one path |
| H06 | A stack has another way to hand the handler a payload (Go's `logger.With`) that skips the mask | Each stack lists its own ways and proves each in a scenario of its own (below) |

## Failing safely

| Id | Defect | The mask's answer |
|---|---|---|
| F01 | A payload that refers to itself (an object graph with a cycle) or is nested without bound overflows the stack and stops the process | A value more than 32 levels below the payload is written `<too deep>`, for maps, lists, objects, and proto messages alike |

## Masking switched off — [payload-policy.feature](./conformance/payload-policy.feature), [masking.feature](./conformance/masking.feature)

`LOG_PAYLOAD_UNMASKED=true` writes payloads as they are, to debug a service; it is a leak on purpose, so its defects are the ways it could be on without anyone knowing or wanting it.

| Id | Defect | The answer |
|---|---|---|
| U01 | The switch is left on in a deployment, or read at a level where nobody debugs | Off by default; refused at start unless `LOG_LEVEL=debug`, naming both variables; a WARN `log payload unmasked` at start, before the first record with a payload of each request, and before every such record outside a request |
| U02 | Unmasked writes more than the data of the call, or less | Marks, secret names, `debug_redact` ignored; a body that is not JSON written as its text; a field marked not to log or left out of its JSON form stays out; depth limit and cut hold |

## Settings — [settings.feature](./conformance/settings.feature)

| Id | Defect | The answer |
|---|---|---|
| S01 | A default other than `always` and 4096 | Defaults hold; usable values are taken |
| S02 | An unusable value starts the service with a guess | The service does not start; the error names the variable |

## Not covered — risks accepted

| Id | Risk | Why it stays |
|---|---|---|
| R01 | A secret in free text under an innocent name (`"note": "my password is …"`) | No mask reads prose; mark the field |
| R02 | A secret in an attribute other than the payload (an error message, a path like `/reset/{token}`) | Only the payload is data of the call; the code chooses every other attribute and must not put a secret in it |
| R03 | A text that starts like JSON but is not valid JSON is written as it is | Treating it as unparsed would hide every title such as `[urgent] buy milk` |
| R04 | A secret whose name is in no safety-net name | The names are the second line; marking is the first |
| R05 | A value that writes itself as text (Go's `encoding.TextMarshaler`, a type with its own JSON converter) is written as that text, its fields not walked | The type chose how it is shown; it must not show a secret |

## Survivors of the mutation runs

| Stack | Mutant | Why it cannot leak |
|---|---|---|
| Go | `handler.go`: the error of writing a warning ignored | The warning is written after its record; failing to write it loses the warning, never masks less |
| Go | `mask.go`: `case tag == "omit"` / `case tag == "mask"` reported not covered | gremlins does not attribute coverage to `case` expressions; both negations were run by hand and are killed |
| Go | `mask.go`: the depth of a struct field not increased, reported timed out | Killed: the self-referring link of F01 then never ends |
| Go | `handler.go`: the bound of the request ids remembered for U01 (4096) moved by one | Only how many requests are remembered: an older request is warned about again, never less |

Run in Go from the example (2026-10-24: 59 killed, 1 timed out, 2 lived, 2 not covered, as listed):

```bash
gremlins unleash --integration --workers 2 --timeout-coefficient 30 \
  --coverpkg ./internal/logging/masked \
  -E '^internal/logging/logging\.go$' -E '^(cmd|internal/(api|command|config|domain|version))/' ./
```
