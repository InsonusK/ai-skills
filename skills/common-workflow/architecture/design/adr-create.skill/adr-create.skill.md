---
name: adr-create
description: Define how to record an architecture decision as an ADR (architecture decision record) inside the skill that owns the decision
whenToUse: when an architecture decision is made while building or editing a skill and the selected variant together with the considered alternatives must be recorded
tags:
  - skill/architecture/design
  - stack
  - concern/architecture
  - concern/documentation
---

# Goal
- Preserve why an architecture decision was made: the problem, the selected variant, and every considered alternative with its benefits and costs.
- Keep the decision record next to the skill it affects, so it is reviewed and versioned together with that skill.

# Core Principle
- One ADR file per decision, stored in an `adr/` folder inside the skill folder that owns the decision.
- An ADR records not only what was decided, but also what was rejected — so future maintainers can understand the trade-offs and revisit the decision when the context changes.

# How to create an ADR
1. Create an `adr/` folder inside the owning skill folder if it does not exist yet.
2. Add one ADR file per decision using [adr.template.md](./adr.template.md) and fill it with real content:
   - YAML header: `name`, `description`, `problem`, `decision`.
   - `# Problem` — the problem or question the decision solves.
   - `# Selected variant` — the chosen variant, explicitly named and linked to its entry in `# Searched variants`.
   - `# Searched variants` — every considered variant, including the selected one, each with `Description`, `Benefits`, `Costs`.
3. Remove all `hint`, `example` and `code example` blocks from the final ADR file.
4. List every created ADR in the `adr:` property of the owning skill's YAML header.
5. In the owning skill's body, briefly summarize the decision and link to the ADR file.

See a complete filled example: [examples/example.adr.md](./examples/example.adr.md).

# Rule
## MUST
- Store ADR files in an `adr/` folder inside the skill folder that owns the decision.
- List the selected variant in `# Searched variants` together with the rejected variants and clearly mark it as selected (for example by adding "(selected)" to its heading).
- Explicitly name and link the selected variant from `# Selected variant` to its entry in `# Searched variants`.
- Describe every variant with `Description`, `Benefits` and `Costs` subsections.
- List every created ADR in the `adr:` property of the owning skill's YAML header and link it from the skill body.
- Remove all `hint`, `example` and `code example` blocks from the final ADR file.

## SHOULD
- Prefer bullet lists for `Benefits` and `Costs`.
- Write one ADR per decision instead of mixing several decisions into one file.

## MUST NOT
- Leave an ADR unlinked: an ADR that is neither registered in the `adr:` YAML property nor linked from the skill body is considered missing.
- Record only the chosen variant without the rejected alternatives.

# Anti-patterns
- **Selected variant missing from `# Searched variants`**
  - Example: `# Selected variant` names "Handle conflicts in HTTP middleware", but `# Searched variants` lists only the rejected options.
  - Consequence: The reader cannot compare the chosen variant against the alternatives on equal terms.
  - Instead: List the selected variant with the same `Description`/`Benefits`/`Costs` structure and mark it as selected.

- **Decision recorded only in the skill body**
  - Example: The skill body states "we use X" without an ADR file.
  - Consequence: The rejected alternatives and their trade-offs are lost, and the decision cannot be revisited later.
  - Instead: Create an ADR file and link it from the skill body.

- **Orphan ADR file**
  - Example: `adr/mutation-tool-choice.md` exists but is absent from the `adr:` YAML property and no body section links to it.
  - Consequence: The decision is invisible to an agent reading the skill.
  - Instead: Register every ADR in the `adr:` property and link it from the skill body.

- **Leaving template hints in the final ADR**
  - Example: Keeping `hint` and `example` blocks after filling the template.
  - Consequence: The final ADR is noisy and harder to follow.
  - Instead: Remove all `hint`, `example`, and `code example` blocks from the final ADR file.

# Check list
- [ ] ADR file is created from [adr.template.md](./adr.template.md) inside the owning skill's `adr/` folder
- [ ] YAML header is filled: `name`, `description`, `problem`, `decision`
- [ ] `# Problem` states the problem or question the decision solves
- [ ] `# Selected variant` explicitly names and links to a variant listed in `# Searched variants`
- [ ] The selected variant appears in `# Searched variants` and is clearly marked as selected
- [ ] Every variant has `Description`, `Benefits` and `Costs` subsections
- [ ] All `hint`, `example` and `code example` blocks are removed from the final ADR file
- [ ] The ADR is registered in the owning skill's `adr:` YAML property
- [ ] The skill body briefly summarizes the decision and links to the ADR file
