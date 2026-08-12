---
name: solution-integration-client-layering
description: Splits the code that consumes an external API into two units — Integration (calls the API and maps its response into a typed object, handling contract errors) and Client (composes 1..n Integration calls into the application's own model) — so a contract change or API replacement only touches Integration and Client
domain: skill
type: architecture
version: 20260730
tags:
  - skill/architecture/solution
  - integration
  - api-consumption
  - decoupling
  - stack
  - concern/architecture

triggers:
  - integrate a new external API
  - add a method that calls an external API
  - replace or version an external API
  - decide where API response parsing should live
  - decide how to mock an external API dependency in tests
creates:
  - "{Api}Integration"
  - "{Api}Client"
extends:
depends_on:
  - "[[skills/common-workflow/develop/solid-decomposition.skill/solid-decomposition.skill.md|solid-decomposition]]"
adr:
  - "[[./adr/integration-client-split.md|Integration/Client split]]"
---

# Goal
- Define the responsibility split between the two layers that sit between the application and any external API: Integration and Client.
- Make sure a contract change or a full API replacement is contained to Integration and Client, and never spreads into application/business code or its tests.

# Capabilities
- Application code depends on a stable, predictable model instead of the external API's response shape.
- A contract change (renamed/added/removed field, new response shape) or a full API replacement only requires changing Integration and Client — no other code or its tests need to change.
- Tests for code that depends on an external API mock Client only; they don't break when the API's contract changes, and testing a new API only requires re-verifying Client's mapping logic.
- Response-shape/contract errors are caught at one boundary (Integration) instead of surfacing deep inside application logic.

# Core Principles
- Every external API is wrapped by exactly one Integration unit and exactly one Client unit.
- Integration owns the call to the API and the mapping of its raw response (typically a string) into a typed object; it also owns detecting and rejecting responses that don't match the expected contract.
- Client owns composing 1..n Integration calls into the application's own model; it hides the API's shape and the existence of Integration from the rest of the application.
- Application/business code depends only on Client. It never calls Integration or the raw API/HTTP client directly.
- Follows [[skills/common-workflow/develop/solid-decomposition.skill/solid-decomposition.skill.md|solid-decomposition]]: Integration and Client are each a unit with exactly one reason to change — the API's contract for Integration, the application's data needs for Client.

# Adr
- [[./adr/integration-client-split.md|Integration/Client split]]
  - Selected variant: two layers (Integration + Client) over a single combined layer or no dedicated layer at all.

# Requirements
SOLUTION:
- [[skills/common-workflow/develop/solid-decomposition.skill/solid-decomposition.skill.md|solid-decomposition]]
  - Used to keep Integration and Client each scoped to a single responsibility when decomposing a new API integration.

STACK-SPECIFIC (choose per target language):
- An HTTP/transport client for the target stack — used by Integration to perform the API call.
- A JSON (or other wire-format) parser for the target stack — used by Integration to turn the raw response body into a typed object.

# Template Skill Mutations
FILES:
- [[./Implementation/{Api}Integration.create.md|{Api}Integration]] - create - calls one external API and maps its response into a typed object, rejecting contract mismatches
- [[./Implementation/{Api}Client.create.md|{Api}Client]] - create - composes 1..n Integration calls into the application's own model

# Workflow

## Integrate a new external API (happy path)
1. Application code calls a method on `{Api}Client`.
2. `{Api}Client` calls one or more methods on `{Api}Integration`.
3. `{Api}Integration` calls the external API and receives a raw response.
4. `{Api}Integration` parses the raw response into a typed object and validates it against the expected contract.
5. `{Api}Integration` returns the typed object to `{Api}Client`.
6. `{Api}Client` maps the typed object(s) into the application's own model and returns it to the caller.

See [happy-path](./diagrams/happy-path.mmd).

## Contract mismatch / API error
1. `{Api}Integration` receives a response that doesn't match the expected shape (missing field, wrong type, unparsable body) or the API call itself fails.
2. `{Api}Integration` raises a typed contract/call error naming the endpoint and what was unexpected — it never returns a partially-filled or best-guess object.
3. `{Api}Client` either propagates the error or translates it into an application-level error; it never swallows it silently.
4. Application code handles the error the same way it handles any other failure from `{Api}Client` — it never has to know the error came from a parsing failure inside Integration.

## Contract change or full API replacement
1. The external API changes its response shape, or is replaced by a different API entirely.
2. Only `{Api}Integration` changes: its response parsing/validation is updated (or replaced) to match the new contract.
3. `{Api}Client`'s mapping to the application model is updated only if the change affects fields the application model actually needs; its method signatures and the application model itself stay the same whenever possible.
4. No other application code changes, because it only ever depended on `{Api}Client`'s application model.

## Testing application code that depends on the API
1. Tests for application/business code mock `{Api}Client`, not `{Api}Integration` or the raw API/HTTP client.
2. A change to the API's contract does not require touching these tests, because `{Api}Client`'s mocked interface is the application model, not the API's shape.
3. Tests for `{Api}Client` itself mock `{Api}Integration`, verifying only the composition/mapping logic.
4. Tests for `{Api}Integration` itself exercise the parsing/validation logic against sample raw responses (including malformed ones), independent of `{Api}Client`.

# Rules

## MUST
- [[./Implementation/{Api}Integration.create.md#MUST|{Api}Integration.create]]
- [[./Implementation/{Api}Client.create.md#MUST|{Api}Client.create]]
- Every external API must be wrapped by exactly one Integration unit and exactly one Client unit.
- Application/business code must depend only on Client; it must never call Integration or the raw API/HTTP client directly.

## SHOULD
- [[./Implementation/{Api}Integration.create.md#SHOULD|{Api}Integration.create]]
- [[./Implementation/{Api}Client.create.md#SHOULD|{Api}Client.create]]

## MUST NOT
- [[./Implementation/{Api}Integration.create.md#MUST NOT|{Api}Integration.create]]
- [[./Implementation/{Api}Client.create.md#MUST NOT|{Api}Client.create]]

# Anti-patterns
- [[./Implementation/{Api}Integration.create.md#Anti-patterns|{Api}Integration.create]]
- [[./Implementation/{Api}Client.create.md#Anti-patterns|{Api}Client.create]]

- **Merging Integration and Client into a single unit**
  - Consequence: a contract change or API replacement now touches the same code the application depends on, increasing the chance of breaking unrelated logic; the unit has two reasons to change instead of one.
  - Instead: keep the call+parsing responsibility (Integration) and the application-mapping responsibility (Client) in separate units, per [[./adr/integration-client-split.md|Integration/Client split]].

- **Mocking the raw HTTP/API client (or Integration) in application-level tests**
  - Consequence: every contract change forces updating mocks in tests that have nothing to do with the API itself, even though the application's behavior didn't change.
  - Instead: mock Client in application-level tests; only Client's own tests should mock Integration, and only Integration's own tests should exercise raw response parsing.

# Check list
- [ ] Every external API has exactly one Integration unit and exactly one Client unit.
- [ ] Integration returns typed objects only, and raises a typed contract error on shape mismatch.
- [ ] Client returns application models only, never Integration response types.
- [ ] No code outside Client calls Integration; no code outside Integration calls the raw API/HTTP client.
- [ ] Tests for application code mock Client, not Integration or the raw API client.
- [ ] A simulated contract change (rename/remove a field in a sample response) only requires edits inside Integration (and, if the application model needs the changed field, Client).
