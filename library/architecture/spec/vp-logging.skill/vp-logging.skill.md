---
name: vp-logging
description: The logging variation point — a log handler attached to the base plateau in place of the base one, which writes the payload every part already logs (a command's parameters, a request's query, headers, and body, a call's message) masked by marks and by names, cut to a size, kept on every record or on failed ones only, and with the request id of the call, while no part that logs changes
whenToUse: when the operators of an application built on the base plateau must see the data of calls in the log — attaching the logging variation point, marking a field as secret or not to log, choosing which records keep their payload, or reviewing what the log of such an application may contain
updated: 20261021
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **The data of a call in the log** - A failed request can be read back from the log: the command's parameters, what the entry point received, and later what a client sent.
- **No secret in the log** - A field marked secret, and a field named like one though nobody marked it, is written masked; a field marked not to log is left out.
- **One request, one id** - Every record of one request carries its request id, so its records are found among those of the requests served at the same time.
- **Attached, not edited** - The handler replaces the base handler in one place of the composition root; the command decorator, the entry points, and the clients do not change.

# Core Principle
- **The parts say what, the handler says how** - Every part logs the data of a call as one payload attribute; this handler is the one place that decides how the payload is written, so masking, limits, and correlation change in one place for all of them.
- **Masked by default for names, by marks for the rest** - The code marks what it knows is secret; names that always are (password, token, secret, authorization, cookie, API key, private key, credential) are masked whether marked or not.

# Scope
- Covers the payload, the request id, and the size of the log records. Shipping logs, sampling, and tracing spans are not covered.
- The implementation for each stack is described in its stack-specialized extension: `vp-logging-in-go`; `vp-logging-in-dotnet` and `vp-logging-in-python` are planned.

# Rule

## MUST

### Write the payload masked
Write the payload attribute of every record as plain data with every secret masked: a field marked secret, a proto field marked `debug_redact`, and any field, map key, or JSON key whose name matches the safety-net names; leave out a field marked not to log; parse a JSON body and mask it by its keys.
- Violation: a payload written as the raw request body; masking only fields someone marked.
- Risk: a password, a token, or another user's data reaches the log, which more people read than the data store, and stays there.
- Fix: mark secrets in the command, request, and message types; keep the safety-net names as a second line.

### Keep the payload by LOG_PAYLOAD
Keep the payload on every record with `LOG_PAYLOAD=always` (the default), only on records of failed calls with `errors`, and on none with `off`, and cut a payload longer than `LOG_PAYLOAD_MAX_BYTES` (default `4096`) as JSON, saying how long it was.
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

### Test the mask and the rules
Test a marked field, an omitted field, each safety-net name, a JSON body, a proto field with `debug_redact`, each `LOG_PAYLOAD` mode on failed and handled records, the cut, the request id, and the settings.
- Risk: a mask that misses one form of payload leaks every secret of that form.
- Fix: one scenario per form.

# Check list
- [ ] The composition root builds this handler in place of the base one; no part that logs changed.
- [ ] Fields marked secret, proto fields with `debug_redact`, and safety-net names are masked; fields marked not to log are left out; a JSON body is masked by its keys.
- [ ] `LOG_PAYLOAD` (`always` default, `errors`, `off`) and `LOG_PAYLOAD_MAX_BYTES` (4096) are read by the handler's package.
- [ ] Every record of a call carries its request id.
