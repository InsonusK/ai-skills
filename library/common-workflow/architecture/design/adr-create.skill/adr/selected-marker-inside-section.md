---
name: selected-marker-inside-section
description: Decide where an ADR marks its selected variant so the link from `# Selected variant` to that variant's heading resolves.
problem: Marking the selected variant as `## X (selected)` changes the heading's anchor, so `[[#X]]` from `# Selected variant` is a broken link. Where does the marker go?
decision: Keep the heading plain and put a `**Selected.**` line as the first line of the selected variant's section.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The previous rule suggested marking the selected variant with "(selected)" in its heading. Authors then linked it from `# Selected variant` by the plain variant name (`[[#X]]`), while the heading's real anchor is `X (selected)` — the link resolved to nothing. The marker must stay visible without breaking the link.

# Selected variant
**Selected variant:** [[#Marker line inside the section]]
- The heading — and so the anchor — is the plain variant name, which is what an author naturally writes in the link.
- Changing the decision later moves one line, not an anchor that other links depend on.

# Searched variants

## Marker line inside the section

**Selected.**

### Description
The selected variant's heading is plain (`## X`); its section starts with the line `**Selected.**`. The link is `[[#X]]`.

### Benefits
- Link text equals the variant name — no special spelling to remember.
- The anchor is stable when the selection changes.
- The marker is still visible at the top of the section and greppable.

### Costs
- The marker is not visible in a heading-only outline view.
- Existing ADRs with "(selected)" in the heading need a one-time migration.

## Marker in the heading, link includes it

### Description
Keep `## X (selected)` and require the link `[[#X (selected)]]`.

### Benefits
- Marker visible in the outline.
- No migration of headings, only of links.

### Costs
- Authors must remember the suffix in the link; the original bug came from exactly this.
- Changing the selection renames two headings and breaks every link to either anchor.
