---
name: testing-skills-grouped-by-topic
description: Where testing skills live — every stack's testing skills together under `skills/testing/`, grouped by topic, instead of under `skills/common-workflow/` and `skills/{stack}/`
problem: Testing skills were split by stack like every other skill, so tracing a failing test meant reading the stack-agnostic skill, the stack's skill, and other stacks' skills in three directories, with more testing rules restated inside architecture catalogs — where should testing skills live so one place answers how a program is tested?
decision: All testing skills live in `skills/testing/{skill-name}/` — the stack-agnostic `{skill-name}` beside every `{skill-name}-in-{stack}`; a testing skill written for one stack is also named `-in-{stack}`; a skill there links only inside `skills/testing/`.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A skill's stack decided its directory: the base in `skills/common-workflow/test/`, each extension in `skills/{stack}/test/`. For testing this worked against how the skills are used (owner, 2026-10-07): when a test fails, the reader goes through the stack skill, the agnostic skill, and other stacks' skills for comparison — three directories — and part of the testing rules sat in architecture catalogs as well. The aim is that an agent applies the testing skills to any program in a stack and gets a stable result, which requires testing to be one self-contained set. Where should testing skills live?

# Selected variant
[[#One directory, grouped by topic]]

# Searched variants

## One directory, grouped by topic

**Selected.**

### Description
`skills/testing/{skill-name}/` holds the stack-agnostic `{skill-name}` skill and every `{skill-name}-in-{stack}` extension side by side. A testing skill that exists for one stack only is still named `{skill-name}-in-{stack}`, because its directory no longer states its stack. A skill under `skills/testing/` links only to files under `skills/testing/`; the direction rule between a base and its extensions is unchanged. Tags are unchanged, so a skillset still resolves by `stack/{value} & concern/testing`.

### Benefits
- One directory to read when a test fails; a topic's base and all its stack variants are adjacent, and a missing variant is visible as a gap.
- No link leaves the directory, so loading a testing skill never pulls in an architecture catalog, and the set can be applied to a program not built from one.

### Costs
- Testing is the one concern not placed by stack; the placement rule in skill-design carries an exception.
- A one-stack testing skill gets a `-in-{stack}` suffix with no base to extend.
- A testing skill cannot link a skill outside the directory even when it relies on it; such a dependency is named as plain text or moved in.

## Keep testing skills by stack

### Description
The status quo: base in `skills/common-workflow/test/`, extensions in `skills/{stack}/test/`.

### Benefits
- One placement rule for every skill.

### Costs
- The three-directory trace described in the problem.
- Nothing marks the testing skills as one set, so links into architecture catalogs accumulate unnoticed.

## One directory, grouped by stack

### Description
`skills/testing/common/`, `skills/testing/{stack}/`.

### Benefits
- Everything for one stack is in one folder.

### Costs
- A base and its extensions stay apart, which is the comparison a reader makes most often.
