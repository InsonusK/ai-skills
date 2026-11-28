---
name: app-command-layer
description: Commands and command factories — plain inputs, result contracts, operation naming, nested dispatch through the pipeline, orchestration and consistency
whenToUse: when adding or reviewing a command, handler, result, command factory, nested command call or business-process orchestration
updated: 20261128
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/commands-named-command.md
  - adr/nested-command-through-pipeline.md
---

# Goal
- Business operations can run unchanged from different entry points.
- Nested operations preserve their command middleware and consistency boundaries.

# Core Principle
- **Dependency boundaries** - Apply [app-layers](../app-layers.skill/app-layers.skill.md) for permitted calls and the own-store repository exception.
- **Operation ownership** - A command owns orchestration; entities and domain services own reusable business invariants.

# Rule

## MUST

### One handler per command, one handler contract
Give every command exactly one handler, and give every handler the same contract: it takes a context and the command and returns the result or an error.
- Risk: a handler with its own shape cannot be wrapped by a middleware or served by an entry point without code written for it alone.
- Fix: define the contract once in the command layer and make every handler satisfy it.

### A command takes plain parameters
Give a command only the plain values its operation needs — ids, names, texts, amounts — never an object of the input's transport.
- Violation: a command field holding a Telegram update, an HTTP request, or a context object whose only purpose is to carry the transport's data.
- Risk: the command runs only from the input whose type it holds, so reusing it from HTTP (`quiz?chatId=111&userId=222`) or a test means faking that input.
- Fix: api unpacks the values (`chatId`, `userId`) and fills the command with them.

### Name a command by its role
Name a command type `{Operation}Command`, its result `{Operation}Result`, and a part of a result `{Thing}View`, in every stack — also where the package or namespace already says command.
- Violation: `PlanItems` as the command type; `command.PlanItems` in Go; a result named `PlanItemsResponse`.
- Risk: a reader tells a command from its result or a view only by knowing the codebase's habit (a verb is a command); a command named like a generated rpc method clashes with it and needs an alias.
- Fix: `PlanItemsCommand`, `PlanItemsResult`, `ItemView`, handled by `PlanItemsHandler`; the operation keeps its name `PlanItems` in logs (`"command":"PlanItems"`) and in scenarios, as [commands-named-command](./adr/commands-named-command.md) records.

### Add a command factory only when the input cannot choose
Add a command factory only when the same input leads to different commands depending on the application's state.
- Violation: a factory that maps each HTTP route to its one command; or a "thick" command that reads the state and branches into what should be two commands.
- Risk: a factory with nothing to decide is a layer nobody needs; a branching command hides several operations inside one.
- Fix: route a fixed input straight to its command; for a state-dependent input, the factory asks domain services or services for the state and returns the command to run.

### Results never carry entities
Return from a command only result types of the command layer, never an entity or a domain service.
- Risk: an entry point that receives an entity depends on the domain and can change its state outside a command.
- Fix: map the entity to a result type inside the command.

### Run a nested command through the command pipeline
Let a command run another command only through that command's handler as the command pipeline built it — wrapped by every command middleware — never by building its handler, calling its logic, or sharing a function with it.
- Violation: a handler that constructs `AddItemHandler` itself, or calls a private function both commands use, to add the items it imports.
- Risk: the nested operation runs without its log line, its access check, and its place in the unit of work, so the same operation behaves differently depending on whether an entry point or a command started it.
- Fix: the handler takes the nested command's handler from the command registry (dotnet: sends the command through `ISender`); logic both commands need moves into a domain service. Decision recorded in [nested-command-through-pipeline](./adr/nested-command-through-pipeline.md).

### Surface every possible inconsistency
Whenever a command — new or changed — writes to more than one place, or creates or changes data in another system, design it per [workflow-consistency-design](library/common-workflow/architecture/core/workflow-consistency-design.skill/workflow-consistency-design.skill.md) and show the user, before writing the code, at which points a failure leaves which inconsistency and how each is closed.
- Violation: a command that saves locally and then calls another service, with the partial-failure case neither handled nor mentioned to the user.
- Risk: a rollback of this application's data does not undo a change already made in another system, and the user learns of the gap from an incident.
- Fix: present the consistency table; close each gap (outbox, idempotency, compensation) or record the user's explicit acceptance.

### Test commands and factories through their direct dependencies
Test commands and factories with service boundaries or own-store repository contracts replaced by fakes and with pure domain behavior exercised normally.
- Risk: Tests bypass orchestration or repeat the integration tests of a store or API client.
- Fix: For a nested operation fake its wrapped handler boundary when isolating the outer command, and verify pipeline integration separately; do not fake a remote transport below a real service.

## SHOULD

### Identity resolved in the command layer
Let api authenticate the transport and pass the caller's identity as plain values; resolve it into the application's user and check access in the command factory or the command.
- Risk: an access rule in api must be repeated by every entry point, and an input that bypasses that api bypasses the check.
- Fix: api passes `userId`/`username`; the factory or command calls the service that owns identity and access.

# Check list
- [ ] Check [handler contracts](#one-handler-per-command-one-handler-contract).
- [ ] Check [plain inputs](#a-command-takes-plain-parameters), [names](#name-a-command-by-its-role) and [result isolation](#results-never-carry-entities).
- [ ] Check [factory admission](#add-a-command-factory-only-when-the-input-cannot-choose).
- [ ] Check [nested dispatch](#run-a-nested-command-through-the-command-pipeline) and [consistency design](#surface-every-possible-inconsistency).
- [ ] Review [identity resolution](#identity-resolved-in-the-command-layer) and [tests](#test-commands-and-factories-through-their-direct-dependencies).
