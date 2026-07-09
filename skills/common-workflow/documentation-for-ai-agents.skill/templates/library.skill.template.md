---
name: library-name
description: How to install, access, and call {library-name} — the root/overview skill, with links to method-group skills when it has more than one
whenToUse: when an agent needs to know how to install, import, or access {library-name}, or which skill covers a specific capability of it
tags:
  - documentation
  - ai-agent
---

# How Apply this template
1. Decide the skill shape first:
   - **Single skill** — {library-name} has one coherent domain and roughly ten or fewer entry points. Document every method as a fragment file inside this same skill folder (see [method-a.md](../method-a.md) and [method-b.md](../method-b.md) for the fragment format) and skip `# Method-group skills` below.
   - **Skill group** — {library-name} has several independent functional domains, or more entry points than fit one index. Use this template only for the root/overview skill. Create one child skill per domain from [method-group.skill.template.md](./method-group.skill.template.md), and fill `# Method-group skills` with links to them.
2. Keep installation/access instructions attached to this skill as `installation.md` in the same folder (see [installation.md](../installation.md) for the format). Installation is never its own skill — every child skill links back here for it instead of repeating it.
3. Fill every section below with real content about {library-name}.
4. Remove all `hint` and `example` blocks, and this `# How Apply this template` section, before finalizing the skill.

# Goal
```hint
What an agent can accomplish by using {library-name}, and why this skill exists.
```
```example
- Give an agent everything needed to install, authenticate, and call the {library-name} SDK correctly.
```

# Core Principle
```hint
The non-obvious constraints or conventions that apply to every call into {library-name} (auth model, versioning, idempotency, rate limits, etc.).
```

# Installation and access
```hint
Point to the attached installation.md instead of inlining it here, so installation stays a single source of truth even when child skills are added later.

MUST:
- Link to `installation.md` in this same folder.
- Never turn installation into its own skill file.
```
```example
See [installation.md](./installation.md) for the install command, import statement, and environment prerequisites.
```

# Method-group skills
```hint
Fill this table only for the skill-group shape. One row per functional domain, each linking to its child skill. Omit this section entirely for the single-skill shape.

MUST:
- One row per domain skill, not per method.
- A short description precise enough that an agent can pick the right child skill without opening it.
```
```example
| Domain    | Skill                                              | Covers                                |
| --------- | --------------------------------------------------- | -------------------------------------- |
| Auth      | [mylib-auth.skill.md](../mylib-auth.skill.md)       | Login, token refresh, logout           |
| Billing   | [mylib-billing.skill.md](../mylib-billing.skill.md) | Charges, refunds, invoices             |
```

# Rule

## MUST
```hint
Actionable rules an agent must follow when using {library-name} in general, or when deciding whether a new capability belongs in this root skill or a new child skill.
```

## SHOULD

## MAY

## SHOULD NOT

## MUST NOT
```hint
At minimum, keep: never duplicate installation instructions inside a child skill; never add a method directly here once the skill has grown into a skill group — add it to the right child skill instead.
```

# Anti-patterns
```hint
Concrete wrong ways to use or extend this skill and their consequences. See documentation-for-ai-agents.skill.md's own Anti-patterns for the two shape-level ones (monolithic skill for a large API; one skill per method) — do not repeat those here unless {library-name} has a domain-specific variant.
```

# Check list
- [ ] Installation/access is documented once in `installation.md` and linked from here (and from every child skill, if any).
- [ ] The single-skill-vs-skill-group decision matches {library-name}'s actual surface (coherent/small vs. multi-domain/large).
- [ ] `# Method-group skills` lists every child skill, or is omitted for the single-skill shape.
- [ ] `whenToUse` lets an agent tell this root skill apart from its child skills.
