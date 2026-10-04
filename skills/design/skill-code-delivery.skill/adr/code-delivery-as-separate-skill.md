---
name: code-delivery-as-separate-skill
description: The rules for how a skill delivers code live in their own skill, linked from skill-content, instead of inside skill-content or skill-design
problem: Skills carry inline code examples that agents reinterpret differently on every run; the rules that turn fixed code into delivered files need a home
decision: A separate skill `skill-code-delivery`, triggered by the presence of code in a skill being created or revised, reached from skill-content's illustration rule
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`skill-content` moves a code block out of the skill file by length (~15 lines) and by whether it illustrates a rule or defines a contract. Neither axis asks whether the code is the same in every project — the property that decides if an agent should retype it or copy it. Where do the rules for that decision live, and to which skills do they apply?

# Selected variant
**Selected variant:** [[#Separate skill, applied to new and revised skills (selected)]]
- The decision has its own trigger (the skill contains code) and its own procedure (variance test, form table), neither of which is about prose.
- Existing skills are not swept; each is classified the next time it is revised.

# Searched variants

## Separate skill, applied to new and revised skills (selected)

### Description
`skill-code-delivery` owns the variance test, the delivery-form table, and the rules for templates, verification, and reference lines. `skill-content`'s "Illustrations move out, contracts stay inline" rule applies it first to every code unit. A skill nobody is editing keeps its code until it is next revised; then every code unit in it is classified.

### Benefits
- A skill with no code never loads these rules.
- The form table and its folder convention have one owner.
- No bulk migration: the ~450 existing inline code blocks are classified as their skills are touched.

### Costs
- A fourth skill to satisfy when writing a skill that contains code.
- Old and new conventions coexist until every code-carrying skill has been revised.
- `skill-content` gains a cross-skill link, so the new skill loads with the baseline.

## Rules added to skill-content

### Description
Two or three extra rules inside `skill-content`, next to the illustration rule.

### Benefits
- No new skill; the baseline stays at three.
- The conflict with the illustration rule is resolved in one file.

### Costs
- `skill-content` is about how prose reads; folder layout, placeholder syntax, and running code before shipping are not prose concerns.
- The form table and seven rules load for every skill-writing task, including skills with no code.

## Separate skill plus a one-off migration of existing skills

### Description
The same skill, followed by a bulk pass that classifies every existing code block in the repository.

### Benefits
- One convention everywhere, immediately.

### Costs
- A wide migration across solution and plateau skills whose `Implementation/` layout is owned by other skills.
- Each extracted file must be executed or built in its own stack toolchain — unaffordable as a single pass.
