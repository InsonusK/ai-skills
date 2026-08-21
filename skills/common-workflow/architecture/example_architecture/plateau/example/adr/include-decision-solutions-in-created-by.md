---
name: include-decision-solutions-in-created-by
description: Whether solution-condition-ownership belongs in plateau-example's created_by even though it contributes no Implementation/*.create.md or *.extend.md project/class file
problem: solution-condition-ownership produces no code — its Implementation/ folder holds a decision table, not a create/extend file for any project or class. Deciding whether plateau assembly should still list it.
decision: Include solution-condition-ownership in created_by and in the plateau root skill's Core Principles/Usecases, following plateau-create-by-solutions's existing rule for classification/decision solutions.
tags:
  - plateau/example
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

[[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] assembles project/class-tier skill files from `Implementation/{Project}.csproj.{create,extend}.md` and nested class files. `solution-condition-ownership` has neither — its only `Implementation/` file is `decision-table.md`, which is not a project or class mutation. A literal reading of the assembly steps could skip it entirely.

But `solution-condition-ownership` is exactly why `EmailRule` exists at all in this plateau instead of two independent local checks — omitting it from `created_by` would make the plateau's `Email`/`ChangeCustomerEmailCommandValidator` classes look like an arbitrary design choice instead of the result of a documented decision.

# Selected variant

**Selected variant:** [[#Include it, per the existing classification/decision rule (selected)]]

# Searched variants

## Include it, per the existing classification/decision rule (selected)

### Description

List `solution-condition-ownership` in `plateau-example`'s `created_by`, and summarize its decision in the root skill's Core Principles and Usecases — even though no `structure/` file under `{Module}.Domain`/`{Module}.Application` is attributed to it directly.

### Benefits

- Matches [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Solution selection|plateau-create-by-solutions's own rule]]: "Classification, taxonomy, or policy solutions may affect only the repository skill and the plateau root skill... Include them in `created_by` even if they have no direct code files." No new rule was needed — the existing process already covers this case.
- An agent reading `plateau-example.skill.md` sees *why* `EmailRule` exists, not just that it does.

### Costs

- `created_by` now contains one entry with no corresponding `structure/` file anywhere, which could look like an error to someone unfamiliar with the classification/decision-solution rule until they read the plateau root skill's Core Principles.

## Omit it, since it produces no structure/ file

### Description

Leave `solution-condition-ownership` out of `created_by`; `plateau-example` is assembled only from the six solutions that directly create or extend a project/class file.

### Benefits

- `created_by` maps one-to-one with `structure/` file provenance — every entry corresponds to at least one concrete file.

### Costs

- Contradicts the existing, already-documented `plateau-create-by-solutions` rule for classification/decision solutions.
- Hides the reason `EmailRule` exists behind an undocumented gap — a reader has no signal that a decision, not just a mechanism, produced this shape.
