# Revisit the variation-point attach rule

Status: Needs decision

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

The allow-list check makes changes to a VP's base explicit. Its earlier
interpretation encouraged unnatural .NET database structure merely to avoid
editing base files. The owner questioned that rule on 2026-11-07. The current
interim rule permits natural base edits when listed in attach.allow; preserve
that behavior while evaluating the replacement.

## Sources and scope

- [Catalog Layout and interim rule](../README.md)
- [Extension contract and ADRs](../spec/base-plateau-extension.skill)
- [Current attach gate](../../../test/check-vp-attach.sh)
- [Entities integration invariants](../agent/dotnet-entities/INVARIANTS.md)

All three stack catalogs and their attach.allow files are in scope. Inspect
shared-file intersections between VPs before proposing a rule. No redesign of
the entities model or repository contracts belongs to this task.

## Dependencies and start condition

Start with a bounded investigation before the next VP edits a shared method.
Coordinate with [middleware selection](middleware-selection.md) if its proposed
registration changes the same extension contract.

## Expected result

1. Inventory actual shared edits and distinguish edits that commute from those
   whose execution order is part of the application's semantics.
2. Present the owner with concrete examples and trade-offs: retain explicit
   allow-lists, introduce stage methods, use stage metadata, or another justified
   option. Stage methods/attributes are proposals, not an approved design.
3. After the choice is agreed, record the owning ADR and update the spec, stack
   guidance, examples, allow-lists and check consistently.
4. Keep unintended edits detectable; do not pass by disabling the gate or
   inventing infrastructure solely to avoid an ordinary base edit.

## Validation

Run the attach gate and shared checks from the execution guide. Prove that an
unlisted base edit fails, an explicitly allowed natural edit passes, and each
approved ordered composition executes in the declared order. Run tests for all
examples whose registration or runtime behavior changes.

## Open decisions

Owner approval is required for the replacement rule, how it models ordering,
and the extent of any migration. Investigation can begin without that choice;
implementation of the replacement cannot.

## Investigation (2026-11-15)

[Decision brief](../agent/attach-rule/REVIEW.md) and its reproducible inventory
cover all 39 allow files across the three stacks. The recommendation is to keep
the explicit file gate, permit natural edits and describe shared-edit semantics
using the existing visible order/dependency mechanisms. Mandatory stage methods
and metadata dispatch are compared as alternatives. No replacement is approved
or implemented; this card remains `Needs decision` until the owner chooses the
rule and migration scope. Validation evidence and the proposal are in the brief;
the implementation PR must add the owning ADR once the choice is agreed.
