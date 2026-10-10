---
name: skill-stack-split
description: How a skill whose implementation differs across stacks is split into a stack-agnostic base and `-in-{stack}` extensions — what each part holds, how it is named and tagged, and which direction the links between them run
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
- A stack-agnostic base `{skill-name}`, tagged with the bare `stack`, holding the requirements and interfaces every stack shares.
- One extension `{skill-name}-in-{stack}` per stack, tagged with one `stack/<value>`, holding that stack's implementation of the base.
- The base stating that the implementation is described in its stack-specialized extensions, naming them only as plain backticked text.
- Every extension linking its base and stating that it details the base's implementation for its stack.

# Core Principle
- **Shared once, per stack once** - A requirement every stack follows lives in the base; an extension makes it concrete for one stack and never restates it.
- **One name ties the family** - The base's name is the prefix of every extension's name, so a search for the base finds every stack variant and a new stack starts from the base.
- **Links run up, never down** - `ai-skill-manager` installs every skill a selected skill links, so an extension linking its base ships one extra skill, while a base linking its extensions ships every stack's skill to a project that uses one stack.
- **Location follows the situation** - Where the base and each extension live is decided by the category they belong to, not by this skill.

# Scope
Covers how a multi-stack skill is split. Whether a skill is split at all is decided by `skill-design`'s rule "Split by stack only for a second, differing stack".

# Rule

## MUST

### Base points to its extensions without linking them
State in the base that the implementation is described in its stack-specialized extensions, naming them only as plain backticked text — never a wikilink or markdown link. Decision recorded in [stack-specific-links-direction](./adr/stack-specific-links-direction.md).
- Violation: `cucumber-testing`'s `# Scope` linking `cucumber-testing-in-go`, `-in-dotnet`, `-in-python`, and `-in-typescript` by path.
- Risk: every other stack's extension is installed into a project that uses only one of them.
- Fix: write `` `cucumber-testing-in-go`, `cucumber-testing-in-dotnet`, `cucumber-testing-in-python`, `cucumber-testing-in-typescript` `` as plain text, and tell the agent to ask the user which one to load for the project's stack.

### Extension links its base
Link the base from every extension, stating that the extension details the base's implementation for its stack.
- Violation: an extension that names its base in backticks only, or restates the base's rules instead of linking them.
- Risk: a project that selects only the extension gets the stack's implementation without the shared requirements it implements.
- Fix: link the base where the extension states what it details, and write only the stack's own additions.

## SHOULD

### Base holds what every stack shares
Write the base as `{skill-name}` with the bare `stack` tag, holding the requirements and interfaces every stack shares and no stack's implementation. Decision recorded in [stack-agnostic-base-and-extensions](./adr/stack-agnostic-base-and-extensions.md).
- Violation: `solution-app-logging` (dotnet) and `solution-go-app-logging` (go) describe the same capability under unrelated names, each restating the shared rules.
- Risk: the stack-independent requirements are duplicated per stack and drift apart, and an agent adding another stack has no base to start from.
- Fix: move the shared goal, rules, and contract into `solution-app-logging`.

### Extension makes the base concrete for one stack
Write one extension per stack as `{skill-name}-in-{stack}` with one `stack/<value>` tag, holding only that stack's implementation of the base.
- Violation: `solution-go-app-logging` — the stack sits in the middle of the name, so the base's name is not its prefix.
- Risk: a name search for the base does not find the extension, and nothing shows the two skills are one capability.
- Fix: rename to `solution-app-logging-in-go` and keep only the Go implementation in it.

## MAY

### Category skills may fix the location
A category-specific skill (e.g. `solution-create`) may fix where its bases and extensions live and detail what a base must hold, and its rule wins for that category.

# Check list
- [ ] The base is named `{skill-name}`, carries the bare `stack` tag, and holds the shared requirements and interfaces with no stack's implementation.
- [ ] Each extension is named `{skill-name}-in-{stack}`, carries one `stack/<value>` tag, and holds only that stack's implementation of the base.
- [ ] The base states that the implementation is described in its extensions and names them only as plain backticked text, never a link.
- [ ] Every extension links its base, states that it details the base's implementation for its stack, and restates none of the base's rules.
