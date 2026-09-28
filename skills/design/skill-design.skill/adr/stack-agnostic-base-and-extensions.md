---
name: stack-agnostic-base-and-extensions
description: How a skill whose implementation differs across stacks is split and named — a stack-agnostic `{skill-name}` base plus `{skill-name}-in-{stack}` extensions — and when the split does not apply
problem: The same skill is written for several stacks with different implementations, but nothing says whether it is one skill, independent per-stack skills, or a base plus extensions, nor how those are named. The repository already holds all three shapes (cucmber-testing + -in-{stack}; solution-app-logging vs solution-go-app-logging; devops-*-in-{stack} with no base).
decision: When a skill's implementation differs between stacks and it is written — or is committed to be written — for more than one stack, it SHOULD be a stack-agnostic base `{skill-name}` under skills/common-workflow/ plus one `{skill-name}-in-{stack}` extension per stack under skills/{stack}/. A single-stack skill with no second stack planned, and a skill identical for every stack, are not split. The `-in-{stack}` suffix appears only on an extension of an existing base. A category-specific skill may override or detail the split. Existing skills are brought in line when they are next edited, not in a bulk migration.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Some skills describe a capability whose goal and rules are the same in every stack while the implementation differs — conformance testing, Cucumber testing, application logging, Kafka producers. The repository has no rule for these, and three shapes coexist:

- a stack-agnostic base plus `-in-{stack}` extensions (`cucmber-testing`, `solution-conformance-testing`);
- independent per-stack skills with unrelated names and no base (`solution-app-logging` in dotnet, `solution-go-app-logging` in go);
- `-in-{stack}` suffixes with no base to extend (`devops-github-action-check-version-in-{stack}`).

An agent writing a new multi-stack skill, or adding a stack to an existing one, cannot tell which shape to produce or what to call it. [[./stack-specific-links-direction.md|stack-specific-links-direction]] already assumes the base-plus-extensions shape for links but does not require it.

# Selected variant

**Selected variant:** [[#Conditional base plus `-in-{stack}` extensions, overridable per category (selected)]]

# Searched variants

## Conditional base plus `-in-{stack}` extensions, overridable per category (selected)

### Description
A SHOULD rule in skill-design. The split applies when (1) the implementation differs between stacks and (2) the skill is written, or is committed to be written, for more than one stack. The base `{skill-name}` (bare `stack` tag, under `skills/common-workflow/`) holds the shared goal, rules, and contract. Each extension `{skill-name}-in-{stack}` (one `stack/<value>` tag, under `skills/{stack}/`) holds only that stack's implementation. A single-stack skill keeps its plain name, and a stack-identical skill stays one agnostic skill. The `-in-{stack}` suffix marks an extension only. A category-specific skill (e.g. solution-create) may override or detail the split. Existing skills are migrated when next edited.

### Benefits
- Shared rules live once, so stacks cannot drift apart on the contract.
- Base and extensions share a name prefix, so an agent finds every stack variant by name and a new stack starts from the base.
- Consistent with the existing `cucmber-testing` / `solution-conformance-testing` precedent and with the link direction in `stack-specific-links-direction`.
- The conditions keep out empty bases and pointless per-stack copies.
- A SHOULD with a category override leaves room for areas that need a different split, without the generic rule blocking them.

### Costs
- "Committed to be written for another stack" is a judgement call, and the agent has to ask the user when it is unclear.
- Until they are next edited, existing skills keep violating the rule (`solution-go-*`, `devops-*-in-{stack}` without a base).
- Renaming an existing solution into a base or extension cascades into plateau `depends_on`/`created_by`, `solution/{name}` tags, and variability-map references.

## Independent per-stack skills, free naming (status quo)

### Description
Each stack writes its own skill under its own name. There is no base and no naming rule.

### Benefits
- No coordination across stacks, and no renames.

### Costs
- The stack-independent rules are copied into each stack's skill and drift apart.
- Unrelated names (`solution-app-logging` vs `solution-go-app-logging`) hide that the skills are the same capability.
- Rejected: this is the inconsistency the rule exists to remove.

## Mandatory split for every skill used by more than one stack

### Description
A MUST: every skill touching more than one stack gets a base and extensions, and every stack-specific skill carries `-in-{stack}`.

### Benefits
- Fully uniform naming, with no judgement calls.

### Costs
- Creates empty bases for single-stack skills and per-stack copies of stack-identical text.
- `-in-{stack}` on a skill with no base promises a base that does not exist.
- Rejected: the user wants a strong recommendation, not a hard requirement, and the unconditional form adds files without adding rules.

## Stack prefix instead of suffix (`{stack}-{skill-name}`, `solution-go-*`)

### Description
Mark the stack at the front of the name, as the go catalog's `solution-go-app-logging` does.

### Benefits
- Groups all of one stack's skills together when names are sorted.

### Costs
- The base name is no longer a prefix of its extensions, so a name search for the base does not find them.
- Conflicts with the existing `-in-{stack}` precedent used by `cucmber-testing` and `solution-conformance-testing`.
- Rejected: stack grouping already comes from the `skills/{stack}/` folder and the `stack/<value>` tag.

## One skill with a section per stack

### Description
Keep one skill file and give each stack its own section inside it.

### Benefits
- One file to find.

### Costs
- Loading the skill for one stack pulls every stack's implementation into context, which is the cost `stack-specific-links-direction` avoids.
- The skill has one condition-gated block per stack, which violates skill-design's One trigger per skill rule.
- Rejected.
