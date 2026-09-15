---
name: skill-name
description: Short description of what the skill does and why it exists
whenToUse: Concrete conditions that tell an agent when to apply this skill
updated: 20260101
tags:
  - tag1
  - tag2
---

# How Apply this template
1. Decide the skill format:
   - **Human Flat**: use for self-contained skills. Save as `{skill-name}.skill.md`.
   - **Human Dir**: use when the skill references extra files (templates, examples, diagrams, etc.). Save as `{skill-name}.skill/{skill-name}.skill.md` and keep all supporting files inside `{skill-name}.skill/`.
2. Fill the front matter. Make `whenToUse` concrete enough that an agent can decide to use the skill just by reading it; set `updated:` to today (YYYYMMDD).
3. Fill each section following the `hint` blocks.
4. Apply [[skills/design/skill-content.skill/skill-content.skill.md|skill-content]] to the prose and [[skills/design/skill-tags.skill/skill-tags.skill.md|skill-tags]] to the `tags:` list; run their `# Check list`s alongside this template's.
5. Remove all `hint`, `example`, and `code example` blocks, and this `# How Apply this template` section before finalizing the skill.

# Goal
```hint
The verifiable outputs the skill produces — what exists after the skill is applied,
one bullet per deliverable. Never working principles (those belong to
# Core Principle), scope boundaries, or meta-intentions. Prefix a bullet with
`**{Name}** - ` (short English noun phrase, self-explanatory without the skill's
history, not a restatement of the description's opening words) only when the
bullet's own text exceeds ~20 words; otherwise leave it unnamed.
```

# Core Principle
```hint
Core principles the agent should follow when applying this skill. Same ~20-word
naming threshold as # Goal applies here.
```

# Rule
```hint
Use only three subsections: MUST, SHOULD, MAY — no MUST NOT/SHOULD NOT headings.
Phrase a prohibition as a negatively-worded rule ("Never...", "Do not...") inside
MUST or SHOULD, whichever strength it actually carries. If a category has no rules,
skip it — do not write an empty subblock.

Every rule is a `###` heading — the rule's name (a short English noun phrase,
self-explanatory without the skill's history) — followed by the imperative
statement as a plain paragraph, followed by elaboration bullets:

### {Rule name}
{The rule itself, as a plain imperative statement — positive or negative}
- Violation: {what not following the rule looks like — an omission/wrong attempt
  for a positive rule, the forbidden action itself for a negative one} (optional)
- Risk: {what breaks because of that violation}
- Fix: {the correct action that replaces the violation}

Every rule under MUST requires Risk and Fix (Violation stays optional). SHOULD
rules carry the elaboration only when the rule is non-obvious. MAY rules never
carry it — permission has nothing to violate.

There is no separate "Anti-patterns" section. State each fact exactly once —
workflow steps and prose reference a rule by anchor link ([Rule name](#rule-name))
instead of restating it.
```

## MUST

### Choose the correct skill format
{Human Flat for self-contained skills; Human Dir when the skill references supporting files.}
- Risk: {what breaks otherwise}
- Fix: {the correct action}

### Keep supporting files inside the skill folder
{When using Human Dir, every referenced template, example, and ADR lives inside `{skill-name}.skill/`.}
- Risk: {what breaks otherwise}
- Fix: {the correct action}

### Use resolvable links
{Links are relative to the skill file or to the repository root.}
- Risk: {what breaks otherwise}
- Fix: {the correct action}

### Make every rule actionable
{Rules, workflows, and checklists tell the agent exactly what to do.}
- Risk: {what breaks otherwise}
- Fix: {the correct action}

## SHOULD

## MAY

# Check list
- [ ] The skill uses the correct format (Human Flat or Human Dir).
- [ ] Front matter is filled: `whenToUse` is concrete and `updated: YYYYMMDD` is set.
- [ ] All rules are actionable for an AI agent.
- [ ] All links use relative or repository-root-relative markdown/wikilink syntax.
- [ ] `# Goal` lists verifiable deliverables — no principles, scope notes, or meta-intentions.
- [ ] Every rule under `# Rule` is a `###` heading followed by the imperative statement as a paragraph; every `## MUST` rule carries `Risk` and `Fix` bullets (`Violation` optional); there is no separate `# Anti-patterns` section and no `## MUST NOT`/`## SHOULD NOT` heading.
- [ ] Any `# Goal`/`# Core Principle` bullet over ~20 words starts with `**{Name}** - ` (name self-explanatory, distinct from the description's opening words).
- [ ] All template hints and example blocks are removed from the final skill.
- [ ] [[skills/design/skill-content.skill/skill-content.skill.md|skill-content]]'s and [[skills/design/skill-tags.skill/skill-tags.skill.md|skill-tags]]'s check lists pass.
