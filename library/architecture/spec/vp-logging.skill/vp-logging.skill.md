---
name: vp-logging
description: The logging variation point — a log handler attached to the base plateau in place of the base one, which writes the payload every part already logs (a command's parameters, a request's query, headers, and body, a call's message) masked by marks and by names, cut to a size, kept on every record or on failed ones only, and with the request id of the call, while no part that logs changes
whenToUse: when the operators of an application built on the base plateau must see the data of calls in the log — attaching the logging variation point, marking a field as secret or not to log, choosing which records keep their payload, or reviewing what the log of such an application may contain
updated: 20261022
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/conformance-scenarios-copied-into-each-stack.md
  - adr/unparsed-body-fails-closed.md
---

# Goal
- **The data of a call in the log** - A failed request can be read back from the log: the command's parameters, what the entry point received, and later what a client sent.
- **No secret in the log** - A field marked secret, and a field named like one though nobody marked it, is written masked; a field marked not to log is left out.
- **One request, one id** - Every record of one request carries its request id, so its records are found among those of the requests served at the same time.
- **Attached, not edited** - The handler replaces the base handler in one place of the composition root; the command decorator, the entry points, and the clients do not change.

# Core Principle
- **The parts say what, the handler says how** - Every part logs the data of a call as one payload attribute; this handler is the one place that decides how the payload is written, so masking, limits, and correlation change in one place for all of them.
- **Masked by default for names, by marks for the rest** - The code marks what it knows is secret; names that always are (password, passwd, pwd, passphrase, secret, token, authorization, cookie, API key, private key, credential, session, cvv) are masked whether marked or not.
- **Fail-closed** - What the mask cannot read is never written as it is: a body that is not JSON, an `Any` of an unknown type, a value nested too deep is written as a placeholder, and the log says so once.
- **One set of scenarios for every stack** - The [leak catalog](./leak-catalog.md) lists every way a secret can reach the log; the scenarios in [conformance/](./conformance/) prove each, and every stack runs them unchanged ([ADR](./adr/conformance-scenarios-copied-into-each-stack.md)).

# Scope
- Covers the payload, the request id, and the size of the log records. Shipping logs, sampling, and tracing spans are not covered.
- The implementation for each stack is described in its stack-specialized extension: `vp-logging-in-go`; `vp-logging-in-dotnet` and `vp-logging-in-python` are planned.

# Rule

## MUST

### Write the payload masked
Write the payload attribute of every record as plain data with every secret masked: a field marked secret, a proto field marked `debug_redact`, and any field, map key, or JSON key whose name matches the safety-net names, at every depth; leave out a field marked not to log and a field the type leaves out of its JSON form; parse a JSON body, a text holding a JSON object or array, and a proto `Any` of a known type, and mask them by their keys.
- Violation: a payload written as the raw request body; masking only fields someone marked; masking the top of the payload only.
- Risk: a password, a token, or another user's data reaches the log, which more people read than the data store, and stays there.
- Fix: mark secrets in the command, request, and message types; keep the safety-net names as a second line.

### Write what cannot be read as unparsed
Write a body that is not JSON, and a proto `Any` of a type the binary does not know, as `<unparsed body, N bytes>`, never as it is, and a value more than 32 levels below the payload as `<too deep>` ([ADR](./adr/unparsed-body-fails-closed.md)).
- Violation: a form body or plain text written as text because it might be readable; an `Any` written as its bytes; a payload that refers to itself walked until the stack overflows.
- Risk: nothing in a body the mask cannot read can be masked, so writing it writes every secret in it; a cycle stops the process.
- Fix: a JSON body, or a field of its own for the readable part.

### Warn once about an unparsed body
Write one WARN record `log payload not parsed` with `record` (the message of the record), `field` (the path of the body, `$.body`), `bytes`, and the request id the first time a record of one message holds an unparsed body at one path, and keep the record itself at the level its code chose.
- Violation: the record raised to WARN; a warning on every request; the body or a part of it in the warning.
- Risk: an alert on the share of WARN records fires on every request of one endpoint, or a payload nobody can read stays unnoticed.
- Fix: the handler remembers each (message, path) it warned about for the life of the process.

### Keep the payload by LOG_PAYLOAD
Keep the payload on every record with `LOG_PAYLOAD=always` (the default), only on records of failed calls with `errors`, and on none with `off`, and cut a payload longer than `LOG_PAYLOAD_MAX_BYTES` (default `4096`, at least `64`) once masked and written as compact JSON, saying how long it was.
- Risk: on a busy service, full bodies on every request multiply the volume of the log; with no payload, a failed request cannot be understood.
- Fix: `errors` for a service whose volume matters, `always` otherwise; the limit bounds any single record.

### Write the request id of the call
Add the request id the entry point put into the call's context to every record of that call.
- Risk: the records of one request cannot be told from those of the requests served at the same time.
- Fix: the entry points set it — the client's when sent, else a new one — and send it back; the handler only writes it.

### Replace only the base handler
Attach the variation point by replacing the construction of the base log handler in the composition root with this handler and its settings, changing no part that logs and no other base file than the composition root, the module's dependency files, and the readme.
- Risk: a part that logs differently once the handler is attached must be described together with it.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

### Read its own settings
Read `LOG_PAYLOAD` and `LOG_PAYLOAD_MAX_BYTES` in the handler's own package, rejecting an unusable value with an error naming its variable.
- Risk: settings added to the base settings outlive the variation point and make it edit a base file.
- Fix: a settings loader beside the handler, called by the composition root before it builds the logger.

### Pass the shared scenarios
Run every file of [conformance/](./conformance/) unchanged in the stack's example — copied by `make conformance-sync`, named in the stack skill's `conformance.copy` — with step definitions of the stack's own, and add a scenario of the stack's own for every other way it hands the handler a payload (H06 of the [leak catalog](./leak-catalog.md)).
- Violation: a stack's own rewording of a shared scenario; a copy edited in place; a scenario skipped because the stack fails it.
- Risk: a mask that misses one form of payload leaks every secret of that form, in the one stack whose tests did not ask for it.
- Fix: a defect found in any stack becomes a row of the leak catalog and a shared scenario first; `bash test/check-conformance.sh` fails while a copy differs from its source.

### Prove the scenarios can fail
Run a mutation test over the stack's masking code and kill every mutant, or list the survivor in the [leak catalog](./leak-catalog.md) with the reason it cannot leak.
- Risk: a scenario that passes whatever the mask does proves nothing; the leak it was written for ships.
- Fix: the stack skill gives the command; its result is recorded with its date.

# Check list
- [ ] The composition root builds this handler in place of the base one; no part that logs changed.
- [ ] Every file of `conformance/` runs unchanged in the stack's example; `bash test/check-conformance.sh` passes.
- [ ] Every other way the stack hands the handler a payload has a scenario of its own.
- [ ] A mutation run over the masking code leaves no survivor missing from the leak catalog.
- [ ] An unreadable body is written `<unparsed body, N bytes>` and warned about once per record message and path.
- [ ] `LOG_PAYLOAD` (`always` default, `errors`, `off`) and `LOG_PAYLOAD_MAX_BYTES` (4096) are read by the handler's package.
- [ ] Every record of a call carries its request id.
