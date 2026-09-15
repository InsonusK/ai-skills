---
name: stack-specific-links-direction
description: Whether a stack-agnostic skill may wikilink the stack-specialized skills that extend it, versus only naming them in plain text
problem: A stack-agnostic skill (e.g. cucmber-testing) is extended by one stack-specialized skill per stack (cucmber-testing-in-go, -in-dotnet, -in-python, -in-typescript). ai-skill-manager resolves every link a loaded skill carries and loads its targets. A project loads one stack-specialized skill for the stack it actually uses, optionally plus the stack-agnostic base it extends. If the stack-agnostic skill links all of its stack-specialized extensions, loading it for one stack pulls in every other stack's skill too.
decision: A stack-agnostic skill never wikilinks a stack-specialized skill that extends it — it names such skills only in plain text (backticked), together with a recommendation to ask the user which stack-specialized skill to load. A stack-specialized skill still links back to the stack-agnostic base it extends, since loading the specialized skill for the one stack in use is expected to also load its single agnostic base.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

[[./cross-skill-links-scope.md|cross-skill-links-scope]] already restricts a skill's links to inputs, required sub-steps, and applied standards. That rule alone does not resolve one more case: a stack-agnostic skill (`cucmber-testing`) is extended by several stack-specialized skills, one per stack (`cucmber-testing-in-go`, `cucmber-testing-in-dotnet`, `cucmber-testing-in-python`, `cucmber-testing-in-typescript`). Each specialized skill genuinely needs the agnostic one — it says so in its own `# Scope` section, and per `cross-skill-links-scope.md` that link is legitimate (a required standard the specialized skill applies).

The reverse direction is the problem. `ai-skill-manager` loads a skill for a project by resolving the links inside it. A project using Go loads `cucmber-testing-in-go`, which links `cucmber-testing` — both load, correctly. But if `cucmber-testing` itself links all four specialized skills (to point the reader at "the right one for your stack"), loading `cucmber-testing` for *any* reason pulls in `cucmber-testing-in-go`, `-in-dotnet`, `-in-python`, and `-in-typescript` together — three of which are irrelevant to that project's stack and cost context for nothing.

# Selected variant

**Selected variant:** [[#Agnostic skill names, never links; specialized skill keeps its link up]]

# Searched variants

## Agnostic skill names, never links; specialized skill keeps its link up (selected)

### Description
A stack-agnostic skill mentions its stack-specialized extensions only as plain text — the skill name in backticks (`` `cucmber-testing-in-go` ``), never a wikilink — and recommends asking the user which one to load for the stack at hand. A stack-specialized skill keeps its existing link to the stack-agnostic base it extends (already justified as a required standard under `cross-skill-links-scope.md`).

### Benefits
- Loading a stack-agnostic skill never drags in every stack's specialized extension — only the specialized skill actually selected for the project loads, and it pulls in its one agnostic base.
- The agnostic skill still tells the reader such extensions exist and how they are named, so nothing is silently hidden — it just does not make the loader fetch all of them.
- Matches how `ai-skill-manager` is actually used: a project declares one stack-specialized skill and optionally its agnostic base, never "all stacks at once".

### Costs
- The agnostic skill's mention of a specialized skill is not a clickable/resolvable link, so a human reading the file must locate the file by name manually if they want to open it directly (an agent instead asks the user which stack applies, per the added rule).

## Link both directions

### Description
The stack-agnostic skill wikilinks every stack-specialized skill that extends it, symmetric with the specialized skill linking back.

### Benefits
- Fully navigable in both directions with one click.

### Costs
- Loading the agnostic skill for any one project pulls in every other stack's specialized skill, defeating the reason `ai-skill-manager` lets a project pick one stack-specialized skill plus its agnostic base — most of what loads is irrelevant to the project.
- Rejected: this is exactly the cost `cross-skill-links-scope.md` already exists to avoid, applied to a case that ADR did not by itself resolve.
