---
name: skill-name
description: Short description of what the skill does and why it exists
whenToUse: Concrete conditions that tell an agent when to apply this skill
tags:
  - tag1
  - tag2
---

# How Apply this template
1. Decide the skill format:
   - **Human Flat**: use for self-contained skills. Save as `{skill-name}.skill.md`.
   - **Human Dir**: use when the skill references extra files (templates, examples, diagrams, etc.). Save as `{skill-name}.skill/{skill-name}.skill.md` and keep all supporting files inside `{skill-name}.skill/`.
2. Fill the front matter. Make `whenToUse` concrete enough that an agent can decide to use the skill just by reading it.
3. Fill each section following the `hint` blocks.
4. Remove all `hint`, `example`, and `code example` blocks, and this `# How Apply this template` section before finalizing the skill.

# Goal
```hint
List of goals that are pursued by the creation of this skill. Prefix a bullet with
`**{Name}** - ` (short English noun phrase, not a restatement of the description's
opening words) only when the bullet's own text exceeds ~20 words; otherwise leave
it unnamed.
```

# Core Principle
```hint
Core principles the agent should follow when applying this skill. Same ~20-word
naming threshold as # Goal applies here.
```

# Rule
```hint
Use only three subsections: MUST, SHOULD, MAY — no MUST NOT/SHOULD NOT headings.
Phrase a prohibition as a negatively-worded bullet ("Never...", "Do not...") inside
MUST or SHOULD, whichever strength it actually carries. If a category has no rules,
skip it — do not write an empty subblock.

There is no separate "Anti-patterns" section. Nest an elaboration directly under a
rule's bullet using exactly these fields, defined relative to the violation (not the
rule's own polarity):

- {The rule itself, as a plain imperative statement — positive or negative}
  - Violation: {what not following the rule looks like — an omission/wrong attempt
    for a positive rule, the forbidden action itself for a negative one} (optional)
  - Risk: {what breaks because of that violation}
  - Fix: {the correct action that replaces the violation}

Every MUST bullet requires Risk and Fix (Violation stays optional). SHOULD bullets
carry the elaboration only when the rule is non-obvious. MAY bullets never carry
it — permission has nothing to violate.

Prefix a bullet with `**{Name}** - ` when its own text (excluding Violation/Risk/Fix)
exceeds ~20 words, or whenever it carries a nested Violation/Risk/Fix at all,
regardless of the bullet's own length. {Name} is a short English noun phrase,
distinct in wording from the description's opening — not a restatement of it.
```

## MUST
- Choose the correct skill format.
- Keep all supporting files inside the skill folder when using Human Dir.
- Use links that are relative to the skill file or to the repository root.
- Make every rule actionable for an AI agent.

## SHOULD

## MAY

# Check list
- [ ] The skill uses the correct format (Human Flat or Human Dir).
- [ ] Front matter is filled, and `whenToUse` clearly states when to apply the skill.
- [ ] All rules are actionable for an AI agent.
- [ ] All links use relative or repository-root-relative markdown/wikilink syntax.
- [ ] Every `## MUST` bullet carries a nested `Risk` and `Fix` (`Violation` optional); there is no separate `# Anti-patterns` section and no `## MUST NOT`/`## SHOULD NOT` heading.
- [ ] Any `# Goal`/`# Core Principle`/`# Rule` bullet over ~20 words, or any `# Rule` bullet with a nested `Violation`/`Risk`/`Fix`, starts with `**{Name}** - ` (name distinct from the description's opening words).
- [ ] All template hints and example blocks are removed from the final skill.
