# Attach rule: decision brief

Status: investigation complete; replacement and migration await owner decision.
Baseline: `bcb325e4` (develop, 2026-11-15). This brief proposes a direction;
it does not change the extension contract or approve a migration.

## Recommendation

Retain explicit `attach.allow` declarations as the file-change gate, permit
natural edits to the owning code, and describe the meaning of shared edits
separately from their permission. Keep execution order explicit in the existing
composition root and middleware lists. Introduce a stage method only for a
demonstrated lifecycle responsibility; do not require a method or metadata
registration for every VP.

This is option B below. It addresses the conflict that motivated the task
without replacing the already ordered middleware contract. File permission
alone cannot establish composability, and a startup stage system would not
solve changes to entity mapping or command registries.

## Evidence and inventory

[INVENTORY.md](INVENTORY.md) records every changed/deleted tracked base path,
addition counts, and all same-base intersections for 39 allow files across
18 distinct bases (including each stack's `attach-proto.allow`). Regenerate it
from the repository root:

```sh
python3 library/architecture/agent/attach-rule/inventory.py > library/architecture/agent/attach-rule/INVENTORY.md
```

The report includes changed/deleted intersections and shared additions even
when a `sample` directive exempts them from the gate. It does not compare
different ancestry levels as if they were siblings, infer method semantics,
or claim that applying two patches in opposite orders yields identical bytes.
The following classification is a source review, not a result of that script.

| Actual edit | Meaning and composition consequence |
|---|---|
| Every stack's base siblings change its composition root | Mixed: independent infrastructure construction can coexist, but middleware order, resource lifetime and replacement of the startup entry require an explicit combined design. Sharing a file is not itself a conflict. |
| HTTP/gRPC client siblings change Go/Python handler registries and .NET application references | They demonstrate alternative implementations of the same `TodoApi` sample role. Do not concatenate both versions of the sample or interpret `sample` as proof they compose. Separate client identities are needed if an application uses both. |
| Go/Python row-version and UoW change the PostgreSQL base's handler registry | `CompleteItem` and `ImportItems` add different commands and can coexist semantically; integrate both fields/build lines and the extended service contract. Preserve the UoW example's decorated `AddItem` before constructing `ImportItems`. Their textual patches need not apply cleanly in either order. |
| .NET row-version changes `Item`, `ItemConfiguration`, command handlers and specifications | Natural changes where the entity and its mapping live. `Version`/`IsRowVersion()` belongs there; moving it elsewhere to avoid an allow-list entry is not justified. The UoW sibling changes no mapping file. |
| Package/project/solution files, Makefiles, generated contracts and README files | Dependency additions with distinct identities can be combined, but versions, target recipes, contract fields and entry instructions still need reconciliation. Regenerate lock/generated files from the merged source; do not treat arbitrary edits in these files as commuting. |
| HTTP proto examples extend their gRPC base in all three stacks | A dependent extension deliberately edits the existing proto, endpoint and sample commands. It is not an independent sibling, and cannot be attached to the plain base with no gRPC contract. |
| Health adds an entry; API VPs replace the startup entry | Preserve every selected entry once and remove the startup entry once. Independent entries may coexist; two whole-root replacements are not additive patches. |
| Retry changes the HTTP-client base's root | Wrap the selected client before passing it to the service/handler. The retry example depends on that client, not on all base-plateau siblings. |

Inspect the exact paths under the corresponding base heading in the inventory.
Representative owning sources:

- [Go UoW registry](../../go/vp-unit-of-work-in-go.skill/examples/internal/command/handlers.go)
  and [row-version registry](../../go/vp-row-version-in-go.skill/examples/internal/command/handlers.go).
- [Python UoW registry](../../python/vp-unit-of-work-in-python.skill/examples/src/todo/command/handlers.py)
  and [row-version registry](../../python/vp-row-version-in-python.skill/examples/src/todo/command/handlers.py).
- [.NET UoW root](../../dotnet/vp-unit-of-work-in-dotnet.skill/examples/src/App/App.Host/Program.cs)
  and [row-version mapping](../../dotnet/vp-row-version-in-dotnet.skill/examples/src/Modules/Todo/Todo.Domain/Configurations/ItemConfiguration.cs).

### Ordered logic already has mechanisms

The [extension contract](../../spec/base-plateau-extension.skill/base-plateau-extension.skill.md)
defines ordered middleware lists, first outermost. The .NET database chain shows
`Logging → StoreFailures → UnitOfWork → handler`: failures from ending a
transaction must reach the failure translator, and logging must observe the
translated result. The fixed API request log and answer-failures wrappers remain
outside the configurable list, as their
[owning ADR](../../spec/base-plateau-extension.skill/adr/fixed-middleware-inside-the-entry.md)
requires. They must not be rediscovered or reordered by a stage registry.

Construction order and call order are different. Building a wrapped nested
handler before its consumer is a dependency constraint; the middleware list is
the order of executing a call. Startup migration before serving entries is a
lifecycle constraint; releasing a database after entries stop is another.
One stage number cannot safely stand for all three relationships.

### What the current gate proves

[check-vp-attach.sh](../../../../test/check-vp-attach.sh) checks that every changed
or deleted base file matches a declaration and compares sibling additions
against the first adder unless either allow file marks the path as a sample.
It does not compare sibling modifications to an existing base file, validate
runtime ordering, enforce the spec's narrow list of permitted file categories,
or prove compatibility among all non-sample adders when the first adder is a
sample. Those limits must remain explicit when discussing a replacement.

There is also prose drift: the spec's description, attach-only rule and
allow-list rule still prohibit edits that the catalog's
[interim rule](../../README.md#layout) permits. The gate already accepts
explicitly listed natural edits, including .NET domain/configuration changes.
An approved migration must reconcile that prose, not tighten the implementation
back to the older restriction.

## Options for the owner

| Option | Concrete effect | Benefits | Costs |
|---|---|---|---|
| A. Keep allow-lists and the interim rule only | Continue listing `ItemConfiguration.cs`, roots and registries as changed; explain each edit in PR prose | No runtime or metadata migration; detects undeclared file changes | Shared semantics remain distributed across PRs; contradictory spec prose still needs correction |
| B. Allow-lists plus a composition explanation (recommended) | For a shared edit, state its owning element, semantic operation, prerequisite and any required relative order; reference existing ordered lists or constructor dependencies | Natural structure; review distinguishes independent edits, ordered logic and alternative sample implementations; existing gate retained | Requires review of semantics; an explanation is not a mechanical proof of compatibility |
| C. Mandatory stage methods | Split roots into explicitly invoked setup/start/stop methods and place VP calls there | Useful where a real lifecycle needs a named phase; invocation order is visible | Does not address shared domain/registry edits; forces small methods for unrelated edits; still needs resource/error handling and explicit middleware order |
| D. Stage metadata with a dispatcher | Register stage/order attributes in .NET and equivalent metadata in Go/Python, discover and invoke stages | Can validate stage names and missing/duplicate registrations | New common contract and three dispatchers; ties/cycles/dependencies/cancellation/disposal need rules and tests; order becomes less visible; still does not solve mapping edits |

For B, a concrete review entry would read: “`NewHandlers`: add `CompleteItem`
beside `ImportItems`; their registration order is immaterial; build decorated
`AddItem` before `ImportItems`; pass the same middleware list to all handlers.”
For the .NET root: “insert `UnitOfWork` inside `StoreFailures`, so commit failure
is translated before logging; resolve both within the command scope.” These
are proposals for review content, not a new manifest syntax.

The [middleware-selection task](../../backlog/middleware-selection.md) remains
separate: any future construction-time filtering must preserve these relative
orders and nested dispatch. Do not choose its metadata API as a side effect of
the attach rule.

## Proposed migration after agreement

1. Record the approved choice in an ADR under the owning extension spec and
   reconcile the shared spec with the catalog interim rule.
2. Review all three stack extension skills, inherited examples, templates,
   `attach.allow` and `conformance.copy`; keep independent VP settings and
   optional-to-optional neutral contracts. Change only those affected by the
   agreed contract, without redesigning entities/repositories.
3. Retain the negative file gate; add agreed ordering/combined-composition
   scenarios and run affected examples on their supported runtimes. For B,
   demonstrate the merged Go/Python `CompleteItem` + `ImportItems` registries
   and .NET failure translation around UoW, rather than claiming file permission
   proves those combinations.

No replacement, new manifest syntax, stage API or catalog-wide migration has
been implemented. The task remains `Needs decision`; approving this research
PR alone does not mark the architecture choice approved.

## Validation evidence

- Current attach gate passes on the baseline.
- Gate perturbation: an unlisted natural edit fails; listing that exact file
  makes the same edit pass; an undeclared deletion fails. Fixtures are restored
  after each check, and no allow-list or runtime example is changed by this PR.
- `python3 test/check-architecture-backlog.py` and `bash test/test.sh` pass,
  including the attach, link-direction, diagram, template, conformance and tag
  checks and sync of 809 skills to two targets.
- Inventory regeneration matches the committed report; script syntax, local
  report links and `git diff --check` pass.
- Runtime examples and testing guidance are unchanged, so their runtime suites
  and the testing-guidance check were not run. Existing runtime order is
  inspected above; no new runtime order or combined application is claimed
  tested by this investigation.
