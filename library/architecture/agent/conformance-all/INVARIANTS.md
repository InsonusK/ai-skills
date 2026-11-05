# Shared scenarios for every variation point — invariants

The anchor document for giving every spec skill of the catalog its `conformance/` scenarios, run unchanged by every stack (per [[library/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). The owner's decisions of 2026-11-04 are in `DECISIONS.md`, the plan in `STATUS.md`; `check.sh` checks what can be checked.

**Problem being fixed.** Only `vp-logging`, `vp-postgres`, and `vp-unit-of-work` keep shared scenarios, and even those two DB ones miss scenarios all three stacks wrote on their own. Every other spec skill leaves its scenarios to each stack, and the stacks have drifted: a Go scenario checks that a message *contains* a text where Python and dotnet check it exactly; a scenario of one stack is missing in another.

## 1. Every scenario of an example is shared or named for its stack

A `.feature` file of a stack example is one of two things:

| Kind | Where its source is | Name |
|---|---|---|
| shared | `spec/{vp}.skill/conformance/{unit}/{name}.feature`, copied by `make conformance-sync` | `{name}.feature`, the source's name |
| the stack's own | the example itself | `{name}-in-{stack}.feature` (`kind-in-go.feature`) |

- A scenario is the stack's own only when it tests a mechanism only that stack has (Go's type check of a middleware's value, dotnet's open generic behaviour). Everything else is shared.
- `check.sh` fails on a `.feature` that is neither a copy nor named `-in-{stack}`.

## 2. Layout of a spec skill's scenarios

- `spec/{vp}.skill/conformance/{unit}/` — one folder per place of the code that runs it (`config/`, `entry/`, `grpc-pipeline/`); its files are copied into that place's `features/` folder.
- File names are kebab-case: `plan-items.feature`, `call-log.feature`.
- A folder holds the `.feature` files and any input they share (a `.proto`), nothing else.
- The VP that brings the code owns its scenarios. A unit two VPs share is owned by one and named by the other's manifest (DECISIONS.md lists which).

## 3. Who runs what

- A stack skill's `conformance.copy` lists every unit its example contains: its own VP's units **and** the units of every example it attaches to (base plateau, the VP it is built on), except a unit whose code the VP deletes (the base's `startup` in an API VP).
- `test/check-vp-attach.sh` keeps a copy in a VP example equal to the base's.
- `vp-cli-api` has no dotnet stack by design; its scenarios are run by Go and Python.

## 4. Merging the stacks' texts into one

- **Union**: every scenario of every stack about the shared contract goes in.
- **Strictest**: where the checks differ, the stricter one wins (the exact message over "contains", more columns over fewer); a stack that then fails has its code fixed, not the scenario weakened.
- **Neutral values**: a value one stack spells its own way is written once, as the output every stack writes, and each stack's step maps it: a log level as the JSON line spells it (`INFO`, `WARN`), a gRPC code by its canonical name (`INVALID_ARGUMENT`), an error kind by its text (`not found`), a bool `true` / `false`. Where the output itself differs between stacks, the output is aligned, not the scenario.
- **Unambiguous steps**: a step's text names what it acts on when the same test project runs another unit's steps (`the gRPC call should …`), so dotnet's project-wide bindings never collide.
- The Feature title and the user story stay; tags (`@type/…`, `@category/…`) stay.

## 5. Each spec skill says it

- The spec skill's rule section states that every file of `conformance/` runs unchanged in each stack (as `vp-logging` does), and its checklist has the item.
- `bash test/check-conformance.sh` and `bash test/check-vp-attach.sh` pass; every example in scope passes its tests (`check.sh --ground`).
