---
name: app-layers
description: How an application's code is split into layers — api, command (with an optional command factory), domain (entities and domain services), service, and infrastructure — what each layer owns, what it may call, where an interface stands between them, how each is tested
whenToUse: when you design an application or a feature, when you add or change an entry point, a command or command factory, an entity or domain service, a service, or an infrastructure adapter, or when you review such a change — in any stack
updated: 20261017
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/service-wraps-one-infrastructure.md
---

# Goal
- **Inputs parsed once** - Every input enters through the api layer, which turns it into plain parameters; no layer below knows the input's format.
- **Commands reusable from any input** - A command takes only the plain values it needs, so the same command runs from a bot, an HTTP call, a CLI, or a test.
- **Infrastructure replaceable** - Replacing a store or a transport (PostgreSQL by Redis, HTTP by gRPC) changes the infrastructure adapter and at most the service over it; no command, domain service, or entity changes.
- **One home per business rule** - Each business rule lives in one entity or domain-service method, and every command that needs it calls that method.
- **Each layer tested alone** - A layer's tests replace only the layer directly below it, so a change below never breaks a test above.

# Core Principle
- **Calls go down only** - api → command factory → command → domain service → service → infrastructure; an entity is called by commands and domain services; nothing calls up.
- **Parse at the edge, decide in the middle** - The input's format is known only to api; which command runs is decided by the factory when the input alone cannot decide; the work is the command's.
- **Services speak the application's language** - A service method names what the application wants (`AddUser`, `SaveQuizAnswer`) and hides how it is done: the calls, the requests, the transaction.
- **Interfaces where I/O starts** - A caller reaches a service, and a service reaches its infrastructure, through an interface; code with no I/O behind it is called directly.

# Scope
- Covers the layer split of one application in any stack. The stack-specific packages and interface placement are described in its stack-specialized extensions: `app-layers-in-go`; `app-layers-in-dotnet` and `app-layers-in-python` are planned.

# Rule

## MUST

### Keep each part in its layer and its calls downward
Place every part of the application in one of these layers and let it call only what its row allows.

| Layer | Owns | May call | Never calls |
|---|---|---|---|
| api | parsing an input into plain parameters; turning a result or an error into the response | command factories, commands | domain, services, infrastructure |
| command factory (optional) | choosing the command from plain parameters and the application's state | domain services, services | infrastructure; never parses an input format |
| command | the main logic of one operation | domain services, entities, services, other commands (nested, through their handlers) | infrastructure, api types |
| domain service | business logic reused by several commands | entities, services | infrastructure, commands |
| entity | the rules of one object of the domain | other entities | anything with I/O |
| service | methods in the application's language over one infrastructure, hiding its technique | its own infrastructure | other services, domain services, commands |
| infrastructure | technical access: queries, HTTP/gRPC calls, parsing a response, contract errors | external systems | every layer above |

- Violation: a command that runs an SQL query, or a service that calls another service to finish its method.
- Risk: a call skipping a layer spreads the knowledge of that layer's technique upward, so replacing it means editing every caller.
- Fix: route the call through the layer in between; when two services must cooperate, a domain service or the command calls both. Decision recorded in [service-wraps-one-infrastructure](./adr/service-wraps-one-infrastructure.md).

### API parses, never decides
Make api turn its input into plain parameters and pass them to a command, or to a command factory when the input alone cannot pick the command.
- Violation: an HTTP handler that checks a business rule, reads the application's state, or calls a service.
- Risk: the rule and the choice of command spread over every entry point, and the next entry point repeats them.
- Fix: api only parses, calls, and maps the result or error to its own response.

### A command takes plain parameters
Give a command only the plain values its operation needs — ids, names, texts, amounts — never an object of the input's transport.
- Violation: a command field holding a Telegram update, an HTTP request, or a context object whose only purpose is to carry the transport's data.
- Risk: the command runs only from the input whose type it holds, so reusing it from HTTP (`quiz?chatId=111&userId=222`) or a test means faking that input.
- Fix: api unpacks the values (`chatId`, `userId`) and fills the command with them.

### Add a command factory only when the input cannot choose
Add a command factory only when the same input leads to different commands depending on the application's state.
- Violation: a factory that maps each HTTP route to its one command; or a "thick" command that reads the state and branches into what should be two commands.
- Risk: a factory with nothing to decide is a layer nobody needs; a branching command hides several operations inside one.
- Fix: route a fixed input straight to its command; for a state-dependent input, the factory asks domain services or services for the state and returns the command to run.

### Results never carry entities
Return from a command only result types of the command layer, never an entity or a domain service.
- Risk: an entry point that receives an entity depends on the domain and can change its state outside a command.
- Fix: map the entity to a result type inside the command.

### Commands never touch infrastructure
Let a command reach storage and other systems only through services, never through infrastructure.
- Violation: a command that calls the database client or the HTTP client itself.
- Risk: replacing the store or the transport means editing the command, and the same technique is rewritten in every command.
- Fix: add the method to the service over that infrastructure and call it from the command.

### One service per infrastructure, in the application's language
Wrap each infrastructure adapter in exactly one service whose methods name what the application wants and hide how it is done.
- Violation: `UserService.PostUsersV2(body)`, or a command that opens a database transaction itself around three calls of a service.
- Risk: the technique leaks into commands, and replacing the infrastructure changes the callers too.
- Fix: `UserService.Add(name)` hides the HTTP calls; `StorageService.UpdateXYZ` hides the statements it runs.

### One business rule, one method
Keep each business rule in exactly one entity or domain-service method and call that method from every command that needs it.
- Violation: "a user name is unique" checked in `AddUser` and again, differently, in `RenameUser`.
- Risk: the copies drift, and the same event is handled differently depending on the path.
- Fix: move the rule into one domain-service method and call it from both commands.

### Interfaces at service and infrastructure boundaries
Make every caller of a service, and every service calling its infrastructure, depend on an interface; call entities and domain services directly.
- Violation: a command holding a concrete `UserHTTPClient`, or an interface written for an entity.
- Risk: without the interface, the layer above cannot be tested without the real system or replaced at startup; an interface over code with no I/O only adds a file.
- Fix: the interface lists the methods the caller uses; the composition root passes the implementation.

### Test each layer at its own boundary
Test each layer with the layer directly below it replaced by a fake, and test infrastructure against the real system or its emulator.

| Layer under test | Replaced by a fake |
|---|---|
| command, command factory | services |
| domain service | services |
| entity | nothing |
| service | its infrastructure |
| infrastructure | nothing — the real engine (embedded) or an emulator of the external API |

- Risk: a test that fakes a layer further down breaks every time that layer's contract changes, though the behaviour under test did not.
- Fix: fake only the interface directly below; build domain services for a command's test with their own services faked.

## SHOULD

### Irreversible external changes go through a workflow
Never change another system irreversibly from inside a command; send the change through an outbox task, or design the workflow so the change can be compensated.
- Risk: a rollback of this application's own data does not undo a change already made in another system, so a failure after the external call leaves the systems inconsistent.
- Fix: send the external change as an outbox task saved together with the command's data, or pair it with a compensating step.

### Identity resolved in the command layer
Let api authenticate the transport and pass the caller's identity as plain values; resolve it into the application's user and check access in the command factory or the command.
- Risk: an access rule in api must be repeated by every entry point, and an input that bypasses that api bypasses the check.
- Fix: api passes `userId`/`username`; the factory or command calls the service that owns identity and access.

## MAY

### Pure helpers usable from any layer
Use a package with no I/O and no business rule — formatting, layout, conversion — from any layer above infrastructure.

# Check list
- [ ] Every part sits in one layer of [Keep each part in its layer and its calls downward](#keep-each-part-in-its-layer-and-its-calls-downward), and every call goes down the table.
- [ ] api only parses, calls a command or a command factory, and maps the result or error.
- [ ] Every command holds plain values, no transport object.
- [ ] Every command factory chooses by application state; no factory maps a fixed input to one command.
- [ ] No command returns an entity; no command, domain service, or entity calls infrastructure.
- [ ] Each infrastructure adapter is wrapped by one service whose methods use the application's language; no service calls another service.
- [ ] Each business rule touched by the change lives in one entity or domain-service method.
- [ ] Callers of services and services calling infrastructure depend on interfaces; entities and domain services are called directly.
- [ ] Each layer's tests fake only the layer directly below it.
- [ ] No command changes another system irreversibly; such a change goes through an outbox task or has a compensating step.
