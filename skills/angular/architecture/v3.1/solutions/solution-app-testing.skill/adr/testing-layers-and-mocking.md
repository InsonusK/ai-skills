---
name: testing-layers-and-mocking
description: Which tool mocks HTTP at which business layer, given the Facade/Client/Signal Store architecture already established
problem: Using both HttpTestingController and MSW without a clear rule for which layer each belongs to risks duplicated mocks and confusion about which one is the source of truth
decision: TestBed for non-DOM business-layer units (Client, Facade, Signal Store); HttpTestingController only at the Client's own unit tests; MSW only for integration tests that span multiple layers or need mocks shared with dev tooling
tags:
  - solution/app-testing
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The `solution-api-http-layer` already establishes three internal layers per feature: Signal Store → Facade → Client → `libs/shared/http-core`. Each layer can be tested by faking the layer directly below it, or by mocking further down (e.g. at the network boundary). Two different HTTP-mocking tools are both reasonable choices in general (`HttpTestingController`, built into Angular; MSW, a network-level service worker interceptor), but using both without a rule for which layer each applies to would let the same HTTP call end up mocked two different ways in two different tests, with no clear source of truth.

# Selected variant

**Selected variant:** [[#Layered strategy: TestBed for non-DOM business-layer units; HttpTestingController only at Client tests, MSW only for cross-layer/integration tests]]

- **Non-DOM business-layer tests** (Client, Facade, Signal Store, any plain service): `TestBed` for DI wiring, asserting behavior via the class's public methods/signals directly.
- **HTTP mocking**: `HttpTestingController` is used exclusively inside `spec/{feature}.client.spec.ts` — the unit test for a feature's own `{feature}.client.ts` — to assert the exact request shape and DTO mapping. Every layer above the Client (Facade, Signal Store) fakes the layer directly beneath it instead of mocking HTTP at all — a Facade test fakes its `Client`; a Signal Store test fakes its `Facade`. MSW is reserved for tests that deliberately span multiple layers together (e.g. a feature-level integration test exercising Signal Store → Facade → Client → an intercepted network boundary) or where mocks need to be shared with non-test tooling (local dev against a mocked backend).

This ADR does not cover how a component itself is tested — a component's own behavior is tested independently of this business layer entirely; see [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill|solution-ui-testing]].

# Searched variants

## Layered strategy: TestBed for non-DOM business-layer units; HttpTestingController only at Client tests, MSW only for cross-layer/integration tests

### Description

See "Selected variant" above — this is the strategy being adopted.

### Benefits

- Each HTTP mock has exactly one place it is defined as the source of truth: DTO/request-shape correctness lives only in `spec/{feature}.client.spec.ts` via `HttpTestingController`; nothing above the Client re-mocks HTTP, so there is no risk of two different tests asserting two different shapes for the same request
- Non-DOM unit tests (`TestBed` for Client/Facade/Store) stay fast and precise, asserting exact method calls and return values without the overhead of rendering a template
- MSW's presence is deliberately scoped to genuine cross-layer/integration scenarios, and doubles as a natural source of shared mocks if local dev tooling needs the same backend mocks

### Costs

- Requires discipline to keep to the rule — an engineer under time pressure might be tempted to use `HttpTestingController` in a Facade or Store test "just this once," reintroducing the duplication this ADR exists to avoid
- Two different mocking tools (`HttpTestingController`, MSW) still exist in the codebase, which is marginally more for a new engineer to learn than a single tool everywhere — mitigated by each having a narrow, well-defined scope

## Single tool everywhere: HttpTestingController at every layer

### Description

Use `HttpTestingController` for Client tests and also for any integration test that needs to simulate a backend response, instead of faking the Facade/Store above the Client.

### Benefits

- One tool to learn, no MSW dependency needed at all
- Very tightly integrated with `TestBed`, no separate service worker setup

### Costs

- Integration tests would need to know the exact HTTP request shape several layers below what they're actually testing, coupling them to Client-level implementation details they shouldn't need to know about
- No natural way to share the same mocked backend behavior with local dev tooling, since `HttpTestingController` only works inside Angular's test environment

## Single tool everywhere: MSW at every layer, including Client unit tests

### Description

Use MSW's network-level interception even for the Client's own narrow unit tests, instead of `HttpTestingController`.

### Benefits

- One tool to learn; mocks are reusable across every test layer and with dev tooling uniformly
- Tests are closer to "real" network behavior even at the lowest layer

### Costs

- Loses `HttpTestingController`'s precision for asserting the exact request Angular's `HttpClient` produced (headers, method, body shape) — MSW mocks at a lower, more black-box network layer, which is a worse fit for a Client test whose entire purpose is verifying that exact request/DTO-mapping correctness
- Adds MSW's service-worker setup overhead even to the narrowest, most frequently-run tests in the suite (Client unit tests), where `HttpTestingController` is already a lighter-weight, built-in fit
