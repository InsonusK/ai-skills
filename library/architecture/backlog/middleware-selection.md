# Select applicable middleware when building the chain

Status: Needs decision

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

The catalog currently wraps calls in ordered middleware lists. Middleware
that does not apply to a command or endpoint can pass every call through
unchanged. The proposal is to decide applicability once when building the chain,
while keeping checks of request values at call time. The former scheduling note
"before DB unit of work" is obsolete: UoW already exists in all three stacks.

## Sources and scope

- [Shared registration and middleware contract](../spec/base-plateau-extension.skill)
- [Go registry and access example](../go/base-plateau-extension-in-go.skill)
- [Python registry and access example](../python/base-plateau-extension-in-python.skill)
- [.NET registry and access example](../dotnet/base-plateau-extension-in-dotnet.skill)
- [Transaction middleware semantics](../spec/vp-unit-of-work.skill)

Cover command and API middleware in Go, Python and .NET, then propagate the
approved contract to dependent VP examples/templates. Preserve logging and
failure handling, middleware order, nested commands, UoW joining and cancellation.
Do not combine this with a general handler-registry rewrite.

## Dependencies and start condition

Investigation can start now. Resolve the registration contract with the owner
before the migration, and coordinate shared edits with [attach-rule](attach-rule.md).

## Expected result

1. Inventory middleware that always applies, applies by a static command/route
   mark, or decides from a value of each request.
2. Prototype construction-time selection in one stack. Existing proposals are
   Go Decorate using TCommand metadata, Python using the command class, and .NET
   constrained registrations or a behavior factory; none is a settled common API.
3. Agree the API, then update the shared spec, owning ADR, three implementations,
   fixed templates, dependent copies and attach.allow files.
4. Statically inapplicable middleware is not wrapped. Value-dependent middleware
   is still evaluated for each call, and nested dispatch uses decorated handlers.

## Validation

Add scenarios proving that static selection runs at construction, excluded
middleware is not invoked, and two calls with different values still get their
respective runtime decision. Check ordering, access rejection, nested commands,
transaction commit/rollback and cancellation in affected examples. Run shared
checks plus Go tests/vet, Python tests/mypy and .NET tests in changed examples.

## Open decisions

Agree the applicability metadata, per-stack integration and migration scope
before implementing the catalog-wide change. No performance target has been
agreed; do not claim an optimization from chain shape alone.
