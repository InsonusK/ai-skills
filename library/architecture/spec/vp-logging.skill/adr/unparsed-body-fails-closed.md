---
name: unparsed body fails closed
description: What the logging variation point writes for a payload part it cannot read — the bytes as text, a placeholder, or a placeholder with a raised record level
problem: The mask finds secrets by field marks and key names; a body that is not JSON (a form, plain text, binary) or a proto Any of an unknown type has neither, so nothing in it can be masked. What is written instead, and how does anyone learn that it happened?
decision: Such a part is written as "<unparsed body, N bytes>" and never as it is; the record keeps the level its code chose, and one WARN record "log payload not parsed" (record, field path, bytes, request id) is written the first time a record of one message has one at one path.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Go variation point wrote a body that is not JSON as text. A form post `user=ann&password=s3cret` reached the log whole, and so did the bytes of a proto `Any`, a secret field among them. A text body the mask cannot read is one it cannot mask.

# Selected variant
[[#Placeholder, one separate warning]]
- Owner decision, 2026-10-22: write every unreadable body as unparsed and make it visible as a warning, so the developers find and fix the payload; the warning a separate record written once, not the record's own level raised.

# Searched variants

## Placeholder, one separate warning

**Selected.**

### Description
The mask writes `<unparsed body, N bytes>` and reports the part's JSONPath (`$`, `$.body`, `$.extra`) and size. The handler writes the record as it was asked to, then a WARN record `log payload not parsed` with `record`, `field`, `bytes`, and the request id, once per (record message, path) for the life of the process.

### Benefits
- Nothing the mask cannot read reaches the log.
- An endpoint taking a non-JSON body shows up once in the warnings, with where to look.
- The level of a record stays its code's choice, so an alert on the share of WARN records is not set off by every request of one endpoint.

### Costs
- A plain-text body is no longer readable in the log; the endpoint needs a JSON body, or the readable part as a field of its own.
- The handler keeps one entry per (message, path) it warned about.

## Placeholder, record raised to WARN

### Description
The record holding the unparsed body is written at WARN or above.

### Benefits
- The record and its warning are one line.

### Costs
- Every request of the endpoint is a WARN record: alerts on WARN fire constantly, and a successful call reads like a problem.
- The handler, not the code, chooses the level of the record.

## Bytes written as text

### Description
The behaviour before: anything that is not JSON is written as a string.

### Benefits
- A plain-text body can be read in the log.

### Costs
- Every secret in a form, a text body, or an `Any` is written as it is.
