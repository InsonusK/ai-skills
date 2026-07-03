---
name: example-decision-record
description: Example architecture decision record showing the expected structure and level of detail.
problem: When a solution requires an architecture decision, the ADR must document the problem, the selected variant, and all alternatives that were considered.
decision: Use a lightweight ADR file inside the solution skill's `adr/` folder, linked from the skill header and body.
---

# Problem

The solution introduces a choice between multiple valid design approaches. Without a written record, future maintainers cannot understand why a specific path was taken or evaluate whether the context has changed enough to revisit the decision.

We need a consistent format that:
- states the problem clearly;
- lists every considered variant;
- marks the selected variant;
- explains benefits and costs of each variant.

# Selected variant

**Selected variant:** [](.agents/skills/solution-create/templates/solution-{Solution}.skill.template/adr/example.adr.md#Keep ADRs inside the solution skill folder)

Store architecture decision records directly inside the solution skill's `adr/` folder, link them in the YAML header under `adr:`, and reference the selected variant in the skill body. This keeps the decision close to the solution it affects and uses the same review process as the skill itself.

# Searched variants

## Keep ADRs inside the solution skill folder

### Description

Create an `adr/` subfolder inside `solution-{SolutionName}.skill/` and add one markdown file per decision. Link the file from the `adr:` header property and briefly mention the selected variant in the skill body.

### Benefits

- Decisions live next to the solution they affect.
- The same skill review process covers ADRs.
- Links between solution body and ADR are short and stable.

### Costs

- A solution with many decisions can accumulate several ADR files.
- Developers must remember to update both the ADR and the skill body when a decision changes.

## Keep all ADRs in a single global folder

### Description

Maintain one central `docs/adr/` folder at repository root and reference ADRs from every skill by absolute path.

### Benefits

- Single place to browse all architecture decisions.
- Easier to enforce naming and indexing conventions.

### Costs

- ADRs are decoupled from the solutions they affect.
- Links from skills to ADRs become longer and more fragile.
- Moving or renaming a solution does not move its ADRs automatically.

## Do not write ADRs at all

### Description

Document decisions only in the skill body or in pull request descriptions.

### Benefits

- Less documentation overhead.
- No separate files to maintain.

### Costs

- Historical context is lost when the skill is refactored.
- Alternatives that were rejected are not recorded, making future revisits harder.
- Pull request descriptions are not versioned with the code.
