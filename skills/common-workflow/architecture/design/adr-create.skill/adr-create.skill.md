---
name: adr-create
description: Define how to record an architecture decision as an ADR (architecture decision record) inside the skill that owns the decision
whenToUse: when an architecture decision is made while building or editing a skill and the selected variant together with the considered alternatives must be recorded
updated: 20260909
tags:
  - skill/architecture/design
  - stack
  - concern/architecture
  - concern/documentation
---

# Goal
- One ADR file per decision, created from [adr.template.md](./templates/adr.template.md) under the owning skill's `adr/` folder, recording the problem, the selected variant, and every considered alternative with its `Description`/`Benefits`/`Costs`.
- The selected variant listed in `# Searched variants` alongside the rejected ones and marked as selected, and named and linked from `# Selected variant`.
- The ADR's YAML `tags` list carrying `concern/documentation/adr`.
- No `hint`/`example`/`code example` block left in the final ADR file.
- Every ADR registered in the owning skill's `adr:` YAML property and linked from its body.

# Core Principle
- **One file per decision** - Exactly one ADR file per decision, in an `adr/` folder inside the skill folder that owns the decision, so it is reviewed and versioned with that skill.
- **Record the rejected options** - An ADR records what was rejected as well as what was decided, so future maintainers can weigh the trade-offs and revisit the decision when the context changes.

# Workflow
1. Create an `adr/` folder inside the owning skill folder if it does not exist yet.
2. Add one ADR file per decision from [adr.template.md](./templates/adr.template.md) and fill it with real content: the YAML header (`name`, `description`, `problem`, `decision`, `tags`), `# Problem` (the question the decision solves), `# Selected variant` (the chosen variant, named and linked to its `# Searched variants` entry), and `# Searched variants` (every considered variant including the selected one, each with `Description`, `Benefits`, `Costs`).
3. Remove every `hint`, `example`, and `code example` block from the final ADR file.
4. List every created ADR in the `adr:` property of the owning skill's YAML header.
5. In the owning skill's body, briefly summarize the decision and link to the ADR file.

See a complete filled example: [examples/example.adr.md](./examples/example.adr.md).

# Rule

## MUST

### Store the ADR next to its skill, at decision time
Store each ADR file in an `adr/` folder inside the skill folder that owns the decision, and create it as soon as the decision is made rather than only describing it in the skill body.
- Violation: the skill body states "we use X" with no corresponding ADR file under `adr/`.
- Risk: the rejected alternatives and their trade-offs are lost, and the decision cannot be revisited later.
- Fix: create an ADR file from [adr.template.md](./templates/adr.template.md) and link it from the skill body.

### List the selected variant among the rejected ones
List the selected variant in `# Searched variants` together with the rejected variants and mark it as selected (for example "(selected)" in its heading).
- Violation: `# Selected variant` names "Handle conflicts in HTTP middleware", but `# Searched variants` lists only the rejected options.
- Risk: the reader cannot compare the chosen variant against the alternatives on equal terms.
- Fix: list the selected variant with the same `Description`/`Benefits`/`Costs` structure and mark it selected.

### Link the selection to its entry
Name and link the selected variant from `# Selected variant` to its entry in `# Searched variants`.
- Risk: the reader cannot tell which searched variant was chosen.
- Fix: add the explicit name and link in `# Selected variant`.

### Describe every variant the same way
Describe every variant with `Description`, `Benefits`, and `Costs` subsections.
- Risk: an unevenly described variant cannot be compared against the others.
- Fix: give each variant all three subsections.

### Register and link every ADR
List every created ADR in the `adr:` property of the owning skill's YAML header and link it from the skill body — an ADR registered in neither place is considered missing.
- Violation: `adr/mutation-tool-choice.md` exists but is absent from the `adr:` YAML property and no body section links to it.
- Risk: the decision is invisible to an agent reading the skill.
- Fix: register every ADR in the `adr:` property and link it from the skill body.

### Tag the ADR
Include a `tags` list in the ADR's YAML header with the mandatory tag `concern/documentation/adr`, and keep the other tags the template carries (for example the bare `stack` tag for stack-agnostic decisions).
- Risk: without the tag, ADR files are invisible to tag-based views and queries.
- Fix: add `concern/documentation/adr` to the ADR's `tags` list.

### Strip template scaffolding
Remove every `hint`, `example`, and `code example` block from the final ADR file.
- Violation: keeping `hint` and `example` blocks after filling the template.
- Risk: the final ADR is noisy and harder to follow.
- Fix: delete all such blocks before committing.

## SHOULD

### Bullet lists for Benefits and Costs
Prefer bullet lists for `Benefits` and `Costs`.

### One decision per file
Write one ADR per decision instead of mixing several decisions into one file.

# Check list
- [ ] ADR file is created from [adr.template.md](./templates/adr.template.md) inside the owning skill's `adr/` folder.
- [ ] YAML header is filled: `name`, `description`, `problem`, `decision`, `tags`.
- [ ] The `tags` list contains the mandatory tag `concern/documentation/adr`.
- [ ] `# Problem` states the problem or question the decision solves.
- [ ] `# Selected variant` explicitly names and links to a variant listed in `# Searched variants`.
- [ ] The selected variant appears in `# Searched variants` and is clearly marked as selected.
- [ ] Every variant has `Description`, `Benefits`, and `Costs` subsections.
- [ ] All `hint`, `example`, and `code example` blocks are removed from the final ADR file.
- [ ] The ADR is registered in the owning skill's `adr:` YAML property.
- [ ] The skill body briefly summarizes the decision and links to the ADR file.
