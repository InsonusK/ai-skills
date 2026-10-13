---
name: goal-by-skill-genre
description: A skill's `# Goal` takes one of two forms by the skill's genre — verifiable outputs in a procedure skill, the properties the element secures in an element skill
problem: The one rule "Goal lists deliverables" fits skills describing work the agent performs, but in a skill describing an element of the target system the only deliverable is the element itself, which `description` already names
decision: A procedure skill's Goal lists verifiable outputs; an element skill's Goal — solution, plateau, plateau component, or one of their structure elements — lists the properties the system has because the element exists
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`skill-content` required every `# Goal` to list verifiable outputs. Of the 636 skills with a `# Goal`, roughly half of all Goal bullets state an intent instead ("Give every layer one project it may reference…", "Provide one reusable named query per entity…"), nearly all of them in solution skills and plateau trees, where the solution and plateau templates ask for the goals pursued by the element. `validation-config.yaml` excludes those trees from the `skill-content` pass, while `skill-design` states that a domain-specific template exempts no skill from `skill-content` — so the standard and the practice contradicted each other.

# Selected variant
**Selected variant:** [[#Two Goal forms by skill genre]]
- An element skill's deliverable is already its `description`; the purpose is the only content its Goal can add.
- No existing solution or plateau skill has to be rewritten.

# Searched variants

## Two Goal forms by skill genre
**Selected.**

### Description
A procedure skill — one describing work the agent performs — keeps Goal as a list of verifiable outputs. An element skill — one describing an element of the target system: a solution, a plateau, a plateau component, or one of their structure elements — writes Goal as the properties the system has because the element exists.

### Benefits
- Each Goal carries content its `description` cannot: acceptance criteria in a procedure skill, the invariant to preserve in an element skill.
- Matches what the solution and plateau templates already produce, so the standard describes the catalog instead of contradicting it.
- The agent keeps a statement of what an element is for when a case falls outside its rules.

### Costs
- An author must classify the skill before writing its Goal.
- A property is harder to verify mechanically than an artifact; a vague purpose ("improve quality") passes more easily than a vague deliverable.

## Deliverables in every skill

### Description
Keep the single rule and rewrite every solution and plateau Goal as a list of artifacts.

### Benefits
- One Goal form across the repository.
- Every Goal usable as acceptance criteria.

### Costs
- An element skill's Goal collapses into "the element exists at this path" — a copy of `description`.
- Several hundred skills to rewrite, each losing its statement of purpose.

## No Goal in element skills

### Description
Drop `# Goal` from element skills and keep purpose in `# Core Principle` or `# Capabilities`.

### Benefits
- No second Goal form to define.

### Costs
- `# Core Principle` says how to think while building the element, not what the element is for; merging the two blurs both.
- Plateau structure skills merge the Goals of every contributing solution into one section with a per-section solutions trailer; removing the section removes that merge point.
- The same rewrite cost as the previous variant.
