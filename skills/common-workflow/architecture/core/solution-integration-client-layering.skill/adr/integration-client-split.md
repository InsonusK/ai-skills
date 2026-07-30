---
name: integration-client-split
description: Where to put API response parsing/error handling versus application-facing composition when consuming an external API
problem: How to structure the code that talks to an external API so a contract change or API replacement touches the smallest possible surface
decision: Split the integration surface into two units — Integration (call + typed parsing + contract-error handling) and Client (composition of 1..n Integration calls into an application model)
---

# Problem

Any code that consumes an external API has to do three different things: call the API, turn its raw response (usually a string) into a typed object, and turn that typed object into something the rest of the application can use without knowing the API's shape. If all three responsibilities live in one place, a change to the API's contract (a renamed field, a new pagination scheme, a full API replacement) forces changes across every caller and every test that touches that code, because callers are coupled to the raw API shape instead of to a stable application-facing boundary.

We need a structure that:
- isolates "does the response still match what we expect" from "what does the application do with this data";
- lets the application depend on a stable, predictable model instead of the API's response shape;
- lets tests mock a single, stable boundary instead of the raw HTTP/API call, so contract changes don't ripple into unrelated test suites.

# Selected variant

**Selected variant:** [[#Two layers — Integration + Client]]

Split the integration surface into two units per external API: an Integration unit that owns the raw call and typed/validated parsing of its response, and a Client unit that composes 1..n Integration calls into the application's own model. The application only ever depends on Client.

# Searched variants

## Two layers — Integration + Client (selected)

### Description
Integration calls the API and maps the raw response (string/JSON) into a typed object that mirrors the API's contract, raising a typed error when the response doesn't match the expected shape. Client calls one or more Integration methods and maps their typed output into the application's own model, hiding the API's shape from the rest of the application.

### Benefits
- A contract change or full API replacement only touches Integration (parsing) and Client (composition) — the rest of the application is untouched.
- Tests for application code mock Client only; they don't need to change when the API contract changes, since Client's output contract is the application model, not the API's.
- Each unit has one reason to change: Integration changes when the API's response shape changes, Client changes when the application's data needs change or when it needs to recompose data from a different/additional Integration call.

### Costs
- Two units and two files per API surface instead of one — more files to navigate for a very small API.
- Requires a clear naming/ownership convention so callers don't accidentally reach past Client into Integration.

## Single layer per API (call + parse + application mapping together)

### Description
One class per API does the HTTP call, response parsing, and application-model mapping in the same methods.

### Benefits
- Fewer files for very small integrations.
- No indirection to trace when reading the code end to end.

### Costs
- A contract change forces edits in the same place application logic depends on, increasing the chance of an unrelated regression.
- Tests that exercise application logic through this class are coupled to the API's response shape; every contract change risks breaking unrelated test suites.
- The class has two reasons to change (API shape, application data needs), violating single responsibility.

## No dedicated layer — call the API directly from business/application code

### Description
Business logic or application services call the HTTP/API client and parse the response inline, wherever the data is needed.

### Benefits
- Fastest to write for a one-off call.

### Costs
- API response parsing and error handling get duplicated at every call site.
- A contract change or API replacement requires finding and editing every call site.
- Business logic becomes untestable without hitting (or fully mocking) the raw API client.
