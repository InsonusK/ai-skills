---
name: testing-layers-and-mocking
description: How different kinds of tests interact with a component/service, and which tool mocks HTTP at which layer, given the Facade/Client/Signal Store architecture already established
problem: Using both HttpTestingController and MSW without a clear rule for which layer each belongs to risks duplicated mocks and confusion about which one is the source of truth; similarly, testing every component through TestBed's componentInstance risks coupling tests to implementation details
decision: TestBed for non-DOM unit tests (Client, Facade, Signal Store); Testing Library for component/DOM-level tests; HttpTestingController only at the Client's own unit tests; MSW only for integration/component tests that span multiple layers or need mocks shared with Storybook/dev tooling
---

# Problem

The "API/HTTP-слой" solution already establishes three internal layers per feature: Signal Store → Facade → Client → `libs/shared/http-core`. Each layer can be tested by faking the layer directly below it, or by mocking further down (e.g. at the network boundary). Two different HTTP-mocking tools are both reasonable choices in general (`HttpTestingController`, built into Angular; MSW, a network-level service worker interceptor), but using both without a rule for which layer each applies to would let the same HTTP call end up mocked two different ways in two different tests, with no clear source of truth. Separately, component-level tests can either reach into `TestBed`'s `componentInstance` directly, or interact with the rendered DOM the way a user would (Testing Library) — these produce different coupling to implementation detail.

# Selected variant

**Selected variant:** [[#Layered strategy: TestBed for non-DOM units, Testing Library for components; HttpTestingController only at Client tests, MSW only for cross-layer/integration tests]]

- **Non-DOM unit tests** (Client, Facade, Signal Store, any plain service): `TestBed` for DI wiring, asserting behavior via the class's public methods/signals directly.
- **Component tests** (anything rendering a template): Angular Testing Library, interacting through the rendered DOM the way a user would (`screen.getByRole`, `fireEvent`/`userEvent`), not through `fixture.componentInstance`.
- **HTTP mocking**: `HttpTestingController` is used exclusively inside a feature's own `{feature}.client.ts` unit tests, to assert the exact request shape and DTO mapping. Every layer above the Client (Facade, Signal Store, components) fakes the layer directly beneath it instead of mocking HTTP at all — a Facade test fakes its `Client`; a Signal Store test fakes its `Facade`; a component test fakes its `Signal Store`. MSW is reserved for tests that deliberately span multiple layers together (e.g. a feature-level integration test exercising Signal Store → Facade → Client → an intercepted network boundary) or where mocks need to be shared with non-test tooling (Storybook, local dev against a mocked backend).

# Searched variants

## Layered strategy: TestBed for non-DOM units, Testing Library for components; HttpTestingController only at Client tests, MSW only for cross-layer/integration tests

### Description

See "Selected variant" above — this is the strategy being adopted.

### Benefits

- Each HTTP mock has exactly one place it is defined as the source of truth: DTO/request-shape correctness lives only in `{feature}.client.ts` tests via `HttpTestingController`; nothing above the Client re-mocks HTTP, so there is no risk of two different tests asserting two different shapes for the same request
- Component tests interacting through the DOM (Testing Library) verify what a user actually experiences, and remain valid even if a component's internal implementation (e.g. which signal holds which piece of state) is refactored, as long as the rendered behavior is unchanged
- Non-DOM unit tests (`TestBed` for Client/Facade/Store) stay fast and precise, asserting exact method calls and return values without the overhead of rendering a template
- MSW's presence is deliberately scoped to genuine cross-layer/integration scenarios, and doubles as a natural source of shared mocks if the future "Дизайн-система"/Storybook tooling needs the same backend mocks for local development

### Costs

- Requires discipline to keep to the rule — an engineer under time pressure might be tempted to use `HttpTestingController` in a component test "just this once," reintroducing the duplication this ADR exists to avoid
- Two different mocking tools (`HttpTestingController`, MSW) still exist in the codebase, which is marginally more for a new engineer to learn than a single tool everywhere — mitigated by each having a narrow, well-defined scope

## Single tool everywhere: HttpTestingController at every layer

### Description

Use `HttpTestingController` for Client tests and also for any component or integration test that needs to simulate a backend response, instead of faking the Facade/Store above the Client.

### Benefits

- One tool to learn, no MSW dependency needed at all
- Very tightly integrated with `TestBed`, no separate service worker setup

### Costs

- Component and integration tests would need to know the exact HTTP request shape several layers below what they're actually testing, coupling them to Client-level implementation details they shouldn't need to know about
- No natural way to share the same mocked backend behavior with Storybook or local dev tooling, since `HttpTestingController` only works inside Angular's test environment

## Single tool everywhere: MSW at every layer, including Client unit tests

### Description

Use MSW's network-level interception even for the Client's own narrow unit tests, instead of `HttpTestingController`.

### Benefits

- One tool to learn; mocks are reusable across every test layer and with Storybook/dev tooling uniformly
- Tests are closer to "real" network behavior even at the lowest layer

### Costs

- Loses `HttpTestingController`'s precision for asserting the exact request Angular's `HttpClient` produced (headers, method, body shape) — MSW mocks at a lower, more black-box network layer, which is a worse fit for a Client test whose entire purpose is verifying that exact request/DTO-mapping correctness
- Adds MSW's service-worker setup overhead even to the narrowest, most frequently-run tests in the suite (Client unit tests), where `HttpTestingController` is already a lighter-weight, built-in fit
