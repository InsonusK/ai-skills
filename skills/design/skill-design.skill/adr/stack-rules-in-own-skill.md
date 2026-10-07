---
name: stack-rules-in-own-skill
description: Where the rules for splitting a skill into a stack-agnostic base and `-in-{stack}` extensions live — inside skill-design or in a skill of their own — and what skill-design keeps
problem: skill-design carried five rules about stack-agnostic bases and stack-specialized extensions, spread over MUST, SHOULD, MAY, and the check list. They apply only to a skill written for more than one stack, yet every skill author read them, and a reader looking for them had no skill name or description to search by.
decision: The rules for how a multi-stack skill is split live in skill-stack-split, together with their two ADRs. skill-design keeps one rule — split by stack only when a second, differing stack implementation exists or is planned, otherwise one skill with no `-in-{stack}` suffix — and links skill-stack-split from it.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

skill-design held the stack rules in four places: a MUST on link direction, three SHOULD rules on the split, a MAY on category overrides, and two check-list items. Three costs followed:

- **Findability.** No skill's name or description mentioned the topic, so the rules were found only by text search inside skill-design.
- **Trigger.** The rules apply when a skill is written for more than one stack. skill-design's own "One trigger per skill" and "Short skills over monoliths" call for a separate skill when a part has its own trigger and its own reasons to change — two of skill-design's four ADRs concerned only this part.
- **Load.** An agent's context holds a skill's body only once the skill is invoked. Inside skill-design the stack rules were read on every skill-writing task; in their own skill they stay at description level until a multi-stack skill is on the table.

One part cannot leave: an author of a single-stack skill never opens a stack skill, and still must not split the skill or name it `-in-{stack}`.

# Selected variant

**Selected variant:** [[#Own skill, with the when-to-split rule kept in skill-design]]

# Searched variants

## Own skill, with the when-to-split rule kept in skill-design

**Selected.**

### Description
skill-stack-split owns how a multi-stack skill is split: what the base and the extensions hold, where they live, how they are named and tagged, and which direction their links run. skill-design keeps one SHOULD rule stating when the split applies and what a skill that is not split looks like, and links skill-stack-split from that rule.

### Benefits
- The topic has a name and a `whenToUse`, so it is found from the skill list.
- A single-stack author reads one rule instead of five.
- The two prohibitions that bind a single-stack author stay in the skill that author loads.
- The link makes `ai-skill-manager` install skill-stack-split wherever skill-design is installed, so the rule never points at a missing skill; skill-stack-split is stack-agnostic, so the link ships no stack's skills.

### Costs
- The when-to-split condition and the split itself sit in two files.
- Inbound links to the moved rules and ADRs had to be repointed.

## Keep every stack rule in skill-design (status quo)

### Description
The five rules and both ADRs stay in skill-design.

### Benefits
- One file holds everything about organizing a skill.
- No links to repoint.

### Costs
- The rules stay unfindable by skill name or description.
- Every skill-writing task reads rules that apply to a minority of skills.
- skill-design keeps a condition-gated block, against its own "One trigger per skill".

## Own skill holding every stack rule, named in skill-design as plain text

### Description
All five rules move, including the two that forbid a needless split and a base-less `-in-{stack}` suffix. skill-design mentions skill-stack-split in backticks only.

### Benefits
- skill-design carries no stack rule at all.

### Costs
- A single-stack author does not open skill-stack-split, so nothing stops a needless split or a `-in-{stack}` name with no base.
- Without a link `ai-skill-manager` does not install skill-stack-split alongside skill-design, so the mention can name a skill the project does not have.
- Rejected: the plain-text form exists to keep other stacks' skills out of a project, and skill-stack-split is not stack-specific.
