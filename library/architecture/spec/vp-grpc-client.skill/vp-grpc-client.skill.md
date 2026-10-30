---
name: vp-grpc-client
description: The gRPC client variation point — a client of another service's gRPC API attached to the base plateau in its infrastructure, with the service over it, calling code generated from the copied proto contract over one fixed connection (the {API} settings, the timeout, the call log, the request id, the gRPC status read as an outcome, the mock as a gRPC server inside the process) and checked against that contract
whenToUse: when an application built on the base plateau must call a gRPC API of another service, or the HTTP form of a proto contract — adding the client, calling a further method of it, replacing the consumed proto with a new version, or reviewing such a client
updated: 20261030
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **Another service's gRPC API called the catalog's way** - An application calls a gRPC API with every property of [api-client](library/develop/core/api-client.skill/api-client.skill.md); this skill adds what is particular to gRPC.
- **Attached, not edited** - The client joins the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) through its extension points: its client package and its service in `[services]` of the composition root, the commands that use it in the command registry.

# Core Principle
- **Generated from the contract, mapped by hand** - The messages and the stub come from the copied `.proto` by the generator of the stack; the client writes per method only the mapping between them and its own types, so no message is retyped and none reaches the service.
- **One connection, one interceptor** - The connection of each API carries every call through one fixed interceptor that bounds it by the timeout, sends the request id, writes the two log records, and turns the gRPC status into the client's outcome; a method of the client is one call of the generated stub.

```
docs/integration/client/{api-name}/{name}.proto      the provider's contract, copied unchanged
        │ generator: the generated types kept the client's (package option replaced, or internal)
        ▼
messages + stub, never edited                        gen/client/{api-name}/ committed, or compiled at build time
        │
infrastructure/client/{api}/      client:  stub call ─► own types       mock/{api}_mock: the generated server implemented
        │                                  │
        ▼                                  ▼
infrastructure/client/core/{grpc client}   connection: target or in-memory emulator; interceptor:
                                           timeout · x-request-id · 2 records · status → unavailable | status error
```

# Scope
- Covers unary calls to a gRPC API whose contract is a `.proto` file (skill api-develop), and the HTTP form of a proto contract, which is called over gRPC instead.
- A streaming call is not covered: the fixed interceptor bounds, logs, and reports unary calls only.
- The implementation for each stack is described in its stack-specialized extension: `vp-grpc-client-in-go`, `vp-grpc-client-in-dotnet`, `vp-grpc-client-in-python`.

# Rule

## MUST

### Dial the target from the {API} group
Connect to `{API}_HOST:{API}_GRPC_PORT`, over TLS when `{API}_TLS=true` and in plain text otherwise, as [api-client](library/develop/core/api-client.skill/api-client.skill.md) configures every client, and connect at the first call, not when the client is built.
- Risk: a client that dials at the start stops the service while the other service is down, which is what readiness without the API avoids.
- Fix: the fixed client code builds the connection lazily; a call to an API that is down reports unavailable.

### Generate the stub from the copied contract
Generate the messages and the client stub of every consumed `.proto` from its copy under `docs/integration/client/{api-name}/` — into `gen/client/{api-name}/`, committed, where the stack generates with a tool (Go, Python), or at every build where the build compiles it (dotnet) — and keep the generated types the client's: the package option the provider set for its own code replaced at generation, or the types internal to the client.
- Violation: the provider's generated package imported; messages written by hand; the copied `.proto` edited to change its package option.
- Risk: the application builds against another module's release cycle, or against messages that drift from the contract.
- Fix: the generator's configuration overrides the option or the access per consumed contract; the copy stays the provider's file.

### One method per method, with its own types
Write each method the application calls as one method of the client that calls the generated stub and maps its response to the client's own types, and list the full name of each such method in one place of the client.
- Violation: a generated message returned to the service; a generated stub used outside the client.
- Risk: the service depends on the provider's messages, and a new version of the contract edits the service.
- Fix: the method returns the application's values or the client's own result type, per [Return the application's values from the client](library/develop/core/api-client.skill/api-client.skill.md#return-the-applications-values-from-the-client).

### Report the gRPC status as the outcome
Report a call answered with `UNAVAILABLE` or `DEADLINE_EXCEEDED` as unavailable and any other failed call with its status code and message, from the interceptor every call of the connection passes.
- Risk: a client that reads statuses per method reports the same failure differently per method.
- Fix: the fixed client code converts the status once; the service maps the codes it expects.

### Mock with the generated server inside the process
Write the emulator of an API as an implementation of its generated service server, and serve it, when `{API}_HOST=mock`, inside the process behind the same interceptor, each message through its wire form: a gRPC server inside the process that the same channel reaches - over an in-memory transport where the stack has one (Go `bufconn`), on the loopback where it has none (Python) - or a call invoker of the fixed code (dotnet).
- Risk: a mock that replaces the stub skips the interceptor, so a mocked run neither logs its calls nor reports their outcomes as a real one does.
- Fix: the fixed client code serves the emulator and keeps the interceptor; the emulator answers every listed method.

### Hold the generated code and the methods against the contract
Keep one scenario that finds each listed method of the client in the copied contract and, where the generated code is committed, one that compiles the copied `.proto` and compares it, but for its file options, with the descriptor compiled into the generated code.
- Risk: a contract replaced without regenerating builds and runs against the old messages until a field is lost in production.
- Fix: the scenario fails with "run make proto-gen" until the generated code is the copy's; code compiled at every build cannot be stale.

### Attach without editing the base
Attach the client by adding its fixed code, the consumed contract and its generated code, its client, its emulator, its service, and the commands that use it, and by changing no base file but the composition root, the command registry when a command is added, and the module's dependency, build, and readme files.
- Risk: any other base file changed by the client must be described together with every variation point that touches it.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

# Check list
- [ ] The client keeps every rule of api-client; its target comes from the `{API}` group, and it connects at the first call.
- [ ] The stub is generated from the copied `.proto`, its types the client's; the copy is unchanged.
- [ ] Each method is one method of the client mapping to its own types; no generated message leaves the client.
- [ ] One interceptor reports unavailable apart from the other statuses for every call.
- [ ] `{API}_HOST=mock` serves the generated server's emulator inside the process behind the same interceptor.
- [ ] Scenarios hold the generated code and the client's methods against the copied contract.
- [ ] No base file changed but the composition root, the command registry, and the module's dependency, build, and readme files.
