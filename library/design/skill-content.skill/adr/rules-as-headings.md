---
name: rules-as-headings
description: Rules under `# Rule` are written as `###` headings (name, imperative paragraph, elaboration bullets) instead of bold-named list bullets
problem: Rules written as bold-named bullets cannot be referenced by anchor, do not appear in the document outline, and scan poorly once a skill accumulates many rules
decision: Every rule under `# Rule` is a `###` heading — the name is the heading, the imperative statement a paragraph, Violation/Risk/Fix bullets under it; MUST requires Risk and Fix; legacy bullet-format skills migrate fully the next time they are touched
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Skills reference their own rules from workflow steps and checklists by quoting the rule's bold name — a plain-text mention findable only by search, not a link. Once a skill grows past a handful of rules, the bulleted `# Rule` section stops scanning: bold names inside list items do not appear in the document outline, so a 15-rule skill offers no table of contents. The laconicity pass on `feature-map-create` also showed that rule names carry the full explanatory load — a heading carries it better than an inline bold prefix.

# Selected variant
**Selected variant:** [[#Rules as level-3 headings]]
- Uniform across MUST/SHOULD/MAY.
- Legacy bullet-format skills migrate lazily: a skill is converted fully the next time it is touched for any reason, never partially.

# Searched variants

## Rules as level-3 headings

### Description
Each rule under `# Rule` is a `###` heading carrying the rule's name; the imperative statement follows as a paragraph; `Violation`/`Risk`/`Fix` are bullets under it. `## MUST`/`## SHOULD`/`## MAY` remain the only `##` headings under `# Rule`.

### Benefits
- Rules are anchor-linkable from workflow steps, checklists, and other skills.
- The document outline becomes a table of contents of every rule.
- Heading-based chunking gives each rule its own addressable unit for retrieval and reading.
- One uniform pattern for all three strengths.

### Costs
- Files grow by ~2 lines per rule (heading plus blank lines).
- Renaming a rule silently breaks anchor links (bold names broke references the same way, but anchor breakage is invisible in rendered output).
- Every existing skill needs migration; until touched, legacy skills show the old format.
- Authors may invent body-layout variants unless the template pins the format down — mitigated by writing the exact format into `skill.template.md`.

## Bold-named bullets (status quo)

### Description
Each rule is a list bullet prefixed `**{Name}** - ` with nested `Violation`/`Risk`/`Fix` bullets.

### Benefits
- Compact; the whole `# Rule` section reads as one continuous list.
- Already the repository-wide convention — zero migration cost.

### Costs
- No anchors: references to a rule are quoted names, found only by text search.
- Rules do not appear in the document outline; a long rule section has no skimmable structure.
- Nested-bullet elaboration reads heavily once a rule carries all of Violation/Risk/Fix.

## Headings for MUST only

### Description
`## MUST` rules become `###` headings; `## SHOULD`/`## MAY` rules stay bullets.

### Benefits
- Fewer new headings; SHOULD/MAY stay lightweight.

### Costs
- Two rule shapes inside one section — an agent or author must hold both patterns.
- Breaks the moment a SHOULD rule grows a Risk/Fix elaboration and needs promotion mid-file.
- Inconsistent outline: some rules listed, some not.
