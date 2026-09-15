---
name: client-mocking
description: For every client that calls an external API, implement a mock stub next to the real client and switch to it at runtime via an ENV variable, so the service can always start without any real integration reachable
whenToUse: when implementing or reviewing a client (see [[skills/common-workflow/architecture/core/solution-integration-client-layering.skill/solution-integration-client-layering.skill.md|solution-integration-client-layering]]) that calls an external API
tags:
  - skill/develop
  - mocking
  - integration
  - stack
  - concern/coding
---

# Goal
- A mock implementation next to every client that calls an external API, matching the real client's interface.
- A single ENV variable per external API that, when set to `mock`, switches the service to the mock client instead of the real one.
- A warning-level log line every time a mock client is used, naming which integration is mocked.
- The service startable end-to-end with zero real external integrations reachable.

# Core Principle
- **The mock is a peer of the real client, not a test double** - it lives next to the real client class, implements the same interface, and ships in the same deployable artifact; it exists so the service can run, not just so tests can pass.
- **Switching is data-driven, never a code branch scattered through the app** - the same ENV variable that already configures the external API's address also selects the mock, so application code never knows which one it got.
- **Mock usage is never silent** - a warning log makes it visible in any environment that a real integration has been swapped for a stub, so nobody mistakes mocked output for real data.

# Rule

## MUST

### Implement a mock stub next to every client
For every client class that calls an external API (see [[skills/common-workflow/architecture/core/solution-integration-client-layering.skill/solution-integration-client-layering.skill.md|solution-integration-client-layering]]), implement a mock class that satisfies the same interface/contract, placed alongside the real client class.
- Violation: a client class for an external API with no corresponding mock anywhere in the codebase.
- Risk: the service cannot be started or developed against without the real external API being reachable, blocking local development, demos, and any environment where that dependency is unavailable.
- Fix: add a `Mock{Api}Client` (or equivalent) implementing the client's interface, returning stable sample data shaped like the real client's return values.

### Select the mock via the API address ENV variable
Use the ENV variable that already holds the external API's address as the switch: when its value equals `mock`, resolve the mock client instead of the real one; any other value resolves the real client against that address.
- Violation: introducing a separate `USE_MOCK_X` flag, or hardcoding the choice in application code instead of reading the existing address variable.
- Risk: two variables can disagree (an address set while the separate flag is left on, or vice versa), and every new client repeats its own ad-hoc switch instead of one consistent convention.
- Fix: at the composition point where the client is registered/constructed, check the address ENV variable; if it equals `mock`, construct the mock client, otherwise construct the real client with that address.

### Log a warning when the mock is selected
When a mock client is resolved instead of the real one, emit a warning-level log naming the integration that is mocked, per [[skills/common-workflow/develop/logging-principle.skill.md|logging-principle]].
- Violation: the mock activates without any log line, or logs at `info`/`debug`.
- Risk: a mock silently answers requests in an environment where a real integration was expected (e.g., staging, a misconfigured deployment), and the resulting behavior is debugged as if the real API were involved.
- Fix: log a warning such as `"using mock client for {Api}: {ENV_VAR_NAME}=mock"` at the point the mock is selected.

## SHOULD

### Keep mock responses realistic and stable
Return sample data shaped exactly like the real client's return type, with deterministic values, rather than empty or random placeholders.
- Risk: a mock that returns empty/null/random data makes the rest of the service unusable or flaky when run against it, defeating the point of being able to run without real integrations.
- Fix: hand-craft representative sample values (or fixtures) that exercise the application's normal code paths.

### Centralize the client-vs-mock selection
Resolve the real-vs-mock choice in one place per client (a factory, DI registration, or composition root), not with a conditional repeated at every call site.
- Risk: scattering the check duplicates the ENV-variable logic and makes it easy for one call site to skip the mock switch entirely.
- Fix: register the resolved client (real or mock) once, per [[skills/common-workflow/architecture/core/solution-integration-client-layering.skill/solution-integration-client-layering.skill.md|solution-integration-client-layering]]'s Client boundary, and inject it everywhere else.

# Check list
- [ ] Every client that calls an external API has a mock implementation next to it, satisfying the same interface.
- [ ] Setting the API's address ENV variable to `mock` switches the service to the mock client, with no other flag required.
- [ ] A warning log fires, naming the mocked integration, whenever the mock client is selected.
- [ ] Mock responses are realistic, stable sample data shaped like the real client's return values.
- [ ] The real-vs-mock choice is resolved once per client, not repeated at call sites.
- [ ] The service starts and runs its normal flows with every client's address ENV variable set to `mock`.
