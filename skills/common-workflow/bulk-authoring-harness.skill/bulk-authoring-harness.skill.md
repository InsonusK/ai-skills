---
name: bulk-authoring-harness
description: How to run a large authoring or migration task that produces many interdependent artifacts, so the user reviews an anchor document and objective checks instead of every file
whenToUse: when one task will create or migrate more artifacts than the user can realistically review file-by-file — a catalog of skills or solution skills, a plateau/solution tree, a large documentation set, or a code migration spanning many files — especially when the artifacts cross-reference each other
updated: 20260909
tags:
  - skill/core
  - stack
  - concern/architecture
  - concern/documentation
---

# Goal
- **Review the shape, not the volume** - The user's review load shifted from "read every produced file" to "review one short anchor document, then trust mechanical checks and a fresh-eyes audit".
- Architectural drift, broken cross-references, and format violations caught by a failing check rather than reaching the user.
- The user kept in the loop only for genuine architectural forks, not for volume.

# Core Principle
- **The anchor document is the contract** - Before authoring the bulk, write one short document stating every invariant the produced artifacts must satisfy — structure, naming, link conventions, the domain rules they encode, what a copied artifact must change. The user reviews this once; every later artifact is checked against it, not read on its own.
- **Objective checks over human reading** - A cross-reference either resolves or it does not; a format rule is either followed or not; a build either passes or not. Prefer a check that answers yes/no over asking the user to notice a problem.
- **Fresh eyes per batch, not per file** - A reviewer (subagent or separate pass) that did not author the batch catches drift the author is blind to. Run it once per batch against the anchor document and the relevant design skills — not once per file.
- **Block only on real forks** - Most work is execution against the anchor document. Record every choice in a decisions log; interrupt the user only for entries that are genuine architectural forks with real trade-offs, marked distinctly.
- **Harness is additive to the design skills** - This skill governs how the work is organised and verified; it does not replace the design skills that govern each artifact's content (e.g. [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]], [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]).

# Workflow
1. **Scope and plan.** List every artifact to produce, classify each (copy-as-is / copy-and-modify / new), and group them into batches ("waves") small enough to review as a diff. Write the plan to a status document in the target tree.
2. **Write the anchor document** (`INVARIANTS.md` or similar) in the target tree: the baseline every artifact assumes, the fixed vocabulary, the link/naming conventions, the per-classification change checklist, and the mapping from units-of-work (features, requirements, VPs) to artifacts. Get the user's review before authoring the bulk.
3. **Write the check script** (`check.sh` or similar) in the target tree: every wiki/relative link resolves; every declared dependency/reference resolves; no forbidden format construct is present; every unit-of-work maps to at least one artifact and vice versa. It exits non-zero on any failure and is re-runnable.
4. **Start the decisions log** (`DECISIONS.md`): one line per choice made during execution, with genuine architectural forks marked distinctly (e.g. a leading `⚠️`).
5. **Per wave:** author the wave's artifacts against the anchor document; run the check script and fix every failure; run a fresh-eyes audit of the wave against the anchor document plus the relevant design skills and fix confirmed defects; update the status document; commit the wave as one commit with a skimmable summary.
6. **Ground truth.** Where the artifacts have an objective test — code that builds, tests that run, diagrams that render, an example application — run it and require it to pass before the task is done.
7. **Hand-off.** Point the user at the anchor document, the check script's clean output, the per-wave commits, the decisions log (⚠️ entries especially), and the ground-truth result.

# Rule

## MUST

### Anchor document before bulk
Write the anchor document and have the user review it before authoring more than the first sample artifact.
- Risk: without an agreed contract, every artifact encodes the author's in-the-moment assumptions, they drift apart across the batch, and the only way to find the drift later is the file-by-file review this skill exists to avoid.
- Fix: write the invariants down first; treat the user's review of that one document as the review of the whole batch's shape.

### Every batch passes the check script
Run the check script after each wave and fix every failure before committing that wave.
- Risk: a broken cross-reference or missing dependency committed in wave 2 is discovered in wave 6, after five more waves built on top of it.
- Fix: make the check script a gate on each commit, not a final step.

### Fresh-eyes audit per batch
Have a reviewer that did not author the batch audit it against the anchor document and the governing design skills, and fix every confirmed defect before moving on.
- Risk: the author cannot see their own consistent-but-wrong assumption; it propagates through every remaining wave unchallenged.
- Fix: a subagent or a deliberately separate pass, once per wave, checking conformance not style.

### Commit each wave separately
Make one commit per wave, with a message summarising what it produced and changed.
- Risk: a single giant commit at the end cannot be reviewed by diffstat, cannot be bisected, and cannot be partially reverted.
- Fix: commit per wave; the user reviews the message and diffstat and dives into the diff only when something looks off.

### Log decisions, gate on forks
Record every non-mechanical choice in the decisions log, mark genuine architectural forks distinctly, and stop for the user only on those.
- Risk: either the user is interrupted for trivial choices until they stop paying attention, or a real fork is decided silently and discovered much later.
- Fix: log everything, escalate only the marked forks, keep the rest moving.

### Run the ground-truth test where one exists
When the artifacts can be objectively exercised — a build, a test suite, a render, a runnable example — do it and require it to pass.
- Risk: a catalog of documents that all pass link and format checks can still describe a system that does not work; nothing but execution catches that.
- Fix: build the example, run the tests, render the diagrams — make the objective check part of "done".

### Do not lower the per-artifact standard
Apply the normal design skills to every artifact; the harness reduces review effort, not authoring rigor.
- Risk: "there is a safety net" becomes an excuse for shallow artifacts, and the net was only ever designed to catch drift and broken links, not sloppy content.
- Fix: each artifact still meets its own design skill's checklist; the harness is additive.

## SHOULD

### Keep the anchor document short
Keep the anchor document short enough to review in one sitting — it is a contract, not a manual.

### Make the check script cheap to run
Make the check script cheap to run so it is run often, not saved for the end.

### Seed the audit for conformance
Seed the fresh-eyes audit with the specific design skills and the anchor document, so it checks conformance rather than re-deriving opinions.

### Keep the status document current
Keep the status document current so an interrupted task can resume without re-deriving where it stopped.

## MAY

### Skeleton for consumer-less artifacts
Reduce depth on artifacts that have no consumer yet (aspirational or placeholder units): a complete skeleton plus an explicit "draft contract" marker, with full authoring deferred until a real consumer exists.

### Combine tracking files for a small task
Combine the status document, the anchor document, and the decisions log into one file for a smaller task.

# Check list
- [ ] Every artifact to produce is listed, classified, and grouped into review-sized waves in a status document.
- [ ] An anchor document states every invariant the artifacts must satisfy, and the user has reviewed it.
- [ ] A re-runnable check script verifies link resolution, dependency resolution, format compliance, and unit-of-work coverage, and exits non-zero on failure.
- [ ] A decisions log exists; architectural forks are marked distinctly and were the only things escalated to the user.
- [ ] Each wave was checked, audited by fresh eyes, and committed as its own commit.
- [ ] The ground-truth test (build / tests / render / runnable example) was run and passed, where one exists.
- [ ] Each artifact still satisfies its own governing design skill's checklist.
