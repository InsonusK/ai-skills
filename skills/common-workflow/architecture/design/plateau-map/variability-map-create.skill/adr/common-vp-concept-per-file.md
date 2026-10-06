---
name: common VP concept in its own file
description: Where the concept and contract of a common Variation Point live relative to the common Variability Map
problem: The common map held every VP's concept section inline, so the file grew with every admitted VP and every reader loaded all concepts to use one — how should concepts be stored so the map stays a small index?
decision: The common map keeps only the table (ID, Status, question, Variants, Constraint, Realization depends on) and the bound stack maps; each VP's concept lives in `vp/vp-c###-{name}/vp-c###-{name}.md`, its contract beside it as `vp-c###-{name}.contract.md`, and candidates in `candidates.md`; the ID cell links the concept file.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The common map carried a `### VP-C### {Name}` concept section per VP below its table. At 11 VPs the concepts were most of the file, and every admitted VP adds one. Its readers work one VP at a time — detailing a VP on a stack, realizing it in a solution, classifying deltas — or need only the table (plateau matrices, Constraint checks), yet each loaded every concept. Stack maps linked each concept by an anchor into that one file. How should the concepts be stored?

# Selected variant
[[#Concept file per VP]]

# Searched variants

## Concept file per VP

**Selected.**

### Description
The common map is an index: the `## Common Variation Points` table and the bound stack maps. Each VP has a folder `vp/vp-c###-{name}/` (`{name}` = VP name in lower case) holding its concept `vp-c###-{name}.md` and, when it has one, its contract `vp-c###-{name}.contract.md`. The table's ID cell links the concept file, and stack maps link the same file. The row's question is the VP's short description; no separate description column. Candidates, which only the admission discussion reads, live in `candidates.md`.

### Benefits
- A reader loads the table plus the one or two concepts it works on; the map grows by one row per VP, not by a section.
- A VP's concept and contract sit together; the path follows from the ID and name, so `check.sh` verifies it both ways (a row without a file, a file without a row).

### Costs
- A VP whose concept builds on another (VP-C005 on VP-C004, VP-C007–VP-C009 on VP-C006) needs two files loaded; the concept names its parent in its first line.
- Reading all concepts in one pass for cross-VP consistency means opening every file.
- Renaming a VP renames its folder and files.

## Inline concept sections

### Description
Keep each concept as a `### VP-C### {Name}` section under the table.

### Benefits
- One file to read and review; cross-VP contradictions are visible in one pass.

### Costs
- The file and every reader's context grow with each VP, though most reads need one concept.

## Description column plus concept files

### Description
Split the concepts out and add a short "description" column to the table.

### Benefits
- A summary next to each ID.

### Costs
- The VP column's question already is that summary; a second one states the same fact twice and drifts from it.
