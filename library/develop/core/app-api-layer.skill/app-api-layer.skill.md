---
name: app-api-layer
description: The transport-independent API layer contract — parse input, dispatch commands without business orchestration, and map outcomes; shared by HTTP, gRPC, CLI and job entry points
whenToUse: when adding or reviewing an entry point, changing its command dispatch or response mapping, or defining an API transport variation point
updated: 20261128
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/api-runs-independent-commands.md
---

# Goal
- Entry points translate inputs and outcomes without owning the application's business process.
- Independent dispatch and dependent orchestration have distinct owners.

# Core Principle
- **Common entry contract** - Apply [app-layers](../app-layers.skill/app-layers.skill.md) for allowed dependencies; transport-specific VPs refine this contract with wire formats and runtime behavior.

# Rule

## MUST

### API parses, never decides
Make api turn its input into plain parameters and pass them to a command, or to a command factory when the input alone cannot pick the command.
- Violation: an HTTP handler that checks a business rule, reads the application's state, or calls a service.
- Risk: the rule and the choice of command spread over every entry point, and the next entry point repeats them.
- Fix: api only parses, calls, and maps the result or error to its own response.

### API runs several commands only when they are independent
Let api call several commands for one input only when none of them uses another's result or waits for another's success; when the commands must run in an order, let api call one orchestrating command that runs them as nested commands.
- Violation: an HTTP handler that creates an order and then reserves stock with the new order's id; a job that skips sending reminders when archiving failed.
- Risk: an order of commands is a business process — written in api, it is repeated by every entry point that needs it and runs outside one unit of work, so a failure halfway leaves the first steps done.
- Fix: independent commands — one command per message of a batch, each answered by its own result or error; an order — `PlaceOrderCommand` runs `CreateOrder` and `ReserveStock` per [nested command dispatch](../app-command-layer.skill/app-command-layer.skill.md#run-a-nested-command-through-the-command-pipeline). Decision recorded in [api-runs-independent-commands](./adr/api-runs-independent-commands.md).

### Map transport results and failures at the entry point
Translate command results and failures into the entry point's response contract without exposing entities or transport objects across that boundary.
- Risk: A domain entity escapes through an endpoint or a transport-specific failure leaks into command logic.
- Fix: Keep input/output types and error presentation in API; let the transport VP define status codes, exit codes, serialization and failure recovery.

### Test API with command boundaries replaced
Test API parsing, dispatch and response/error mapping with its command handlers and any command factory replaced by fakes.
- Risk: Entry tests repeat domain behavior and fail when unrelated business rules change.
- Fix: Check inputs sent to command boundaries and responses returned to callers; keep protocol-specific cases in the transport VP.

# Check list
- [ ] Check [input translation](#api-parses-never-decides) and [dispatch independence](#api-runs-several-commands-only-when-they-are-independent).
- [ ] Check [outcome mapping](#map-transport-results-and-failures-at-the-entry-point).
- [ ] Check [API tests](#test-api-with-command-boundaries-replaced).
