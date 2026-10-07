---
name: skill-stack-split
description: How a skill whose implementation differs across stacks is split into a stack-agnostic base and `-in-{stack}` extensions — what each part holds, where it lives, how it is named and tagged, and which direction the links between them run
whenToUse: when you write a skill for more than one stack, add a stack to an existing skill, or edit a stack-agnostic base skill or one of its `-in-{stack}` extensions
updated: 20261007
tags:
  - skill/core
  - stack
  - concern/documentation
adr:
  - adr/stack-agnostic-base-and-extensions.md
  - adr/stack-specific-links-direction.md
---

# Goal
- A stack-agnostic base `{skill-name}` under `skills/common-workflow/`, tagged with the bare `stack`, holding what every stack shares.
- One extension `{skill-name}-in-{stack}` per stack under `skills/{stack}/`, tagged with one `stack/<value>`, holding only that stack's implementation.
- The base naming its extensions only as plain backticked text, with an instruction to ask the user which one to load.
- Every extension linking the base it extends.

# Core Principle
- **Shared once, per stack once** - A rule every stack follows lives in the base; an extension adds its stack's implementation and never restates the base.
- **One name ties the family** - The base's name is the prefix of every extension's name, so a search for the base finds every stack variant and a new stack starts from the base.
- **Links run up, never down** - `ai-skill-manager` installs every skill a selected skill links, so an extension linking its base ships one extra skill, while a base linking its extensions ships every stack's skill to a project that uses one stack.

# Scope
Covers how a multi-stack skill is split. Whether a skill is split at all is decided by `skill-design`'s rule "Split by stack only for a second, differing stack".

# Rule

## MUST

### Base names its extensions as plain text
Name a stack-specialized extension inside its stack-agnostic base only as plain backticked text — never a wikilink or markdown link — and tell the agent to ask the user which one to load for the stack in use. Decision recorded in [stack-specific-links-direction](./adr/stack-specific-links-direction.md).
- Violation: `cucmber-testing`'s `# Scope` linking `cucmber-testing-in-go`, `-in-dotnet`, `-in-python`, and `-in-typescript` by path.
- Risk: every other stack's extension is installed into a project that uses only one of them.
- Fix: write `` `cucmber-testing-in-go`, `cucmber-testing-in-dotnet`, `cucmber-testing-in-python`, `cucmber-testing-in-typescript` `` as plain text, and add a rule or note telling the agent to ask the user which one to load for the project's stack.

### Extension links its base
Link the stack-agnostic base from every stack-specialized extension of it.
- Violation: an extension that names its base in backticks only, or restates the base's rules instead of linking them.
- Risk: a project that selects only the extension gets the stack's implementation without the shared rules it implements.
- Fix: link the base where the extension states what it extends, and state only the stack's own additions.

## SHOULD

### Base holds what every stack shares
Write the base as `{skill-name}` under `skills/common-workflow/` with the bare `stack` tag, holding the goal, rules, and contract every stack shares and no stack's implementation. Decision recorded in [stack-agnostic-base-and-extensions](./adr/stack-agnostic-base-and-extensions.md).
- Violation: `solution-app-logging` (dotnet) and `solution-go-app-logging` (go) describe the same capability under unrelated names, each restating the shared rules.
- Risk: the stack-independent rules are duplicated per stack and drift apart; an agent adding another stack has no base to start from.
- Fix: move the shared goal, rules, and contract into `solution-app-logging`.

### Extension holds one stack's implementation
Write one extension per stack as `{skill-name}-in-{stack}` under `skills/{stack}/` with one `stack/<value>` tag, holding only that stack's implementation.
- Violation: `solution-go-app-logging` — the stack sits in the middle of the name, so the base's name is not its prefix.
- Risk: a name search for the base does not find the extension, and nothing shows the two skills are one capability.
- Fix: rename to `solution-app-logging-in-go` and keep only the Go implementation in it.

## MAY

### Category skills may refine the split
A category-specific skill (e.g. `solution-create`) may override or detail where its bases live and what a base must hold, and its rule wins for that category.

# Check list
- [ ] The base is named `{skill-name}`, lives under `skills/common-workflow/`, carries the bare `stack` tag, and holds no stack's implementation (unless a category skill overrides it).
- [ ] Each extension is named `{skill-name}-in-{stack}`, lives under `skills/{stack}/`, carries one `stack/<value>` tag, and holds only that stack's implementation.
- [ ] The base names its extensions only as plain backticked text, never a link, and tells the agent to ask the user which one to load.
- [ ] Every extension links its base and restates none of its rules.
