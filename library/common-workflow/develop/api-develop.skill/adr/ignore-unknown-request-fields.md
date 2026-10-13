---
name: ignore-unknown-request-fields
description: Decide how the HTTP form of a grpc contract treats a request field it does not know.
problem: The proto-JSON spec says a parser should reject unknown fields by default but may offer an option to ignore them. Which behaviour does the HTTP form use?
decision: Ignore unknown request fields.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A client built against a newer minor version of a contract sends fields an older server does not know. The proto-JSON mapping leaves the choice to the parser option; without a fixed answer each service picks its framework's default.

# Selected variant
**Selected variant:** [[#Ignore unknown fields]]
- A minor bump is backward-compatible on the wire for grpc; the HTTP form must not be stricter.

# Searched variants

## Ignore unknown fields

**Selected.**

### Description
The server parses with the ignore-unknown option (`DiscardUnknown` in Go `protojson`, `ignore_unknown_fields` in Python `json_format`; the grpc-gateway default).

### Benefits
- A newer client keeps working against an older server, matching grpc binary behaviour.
- The grpc-gateway default — no configuration in the common case.

### Costs
- A misspelled field name is silently dropped instead of reported.

## Reject unknown fields

### Description
The parser's strict default: an unknown field fails the request with `INVALID_ARGUMENT`.

### Benefits
- Typos surface immediately.

### Costs
- Every minor addition breaks older servers for HTTP callers only, so HTTP and grpc compatibility differ.
