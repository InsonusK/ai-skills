---
name: skill-design
description: How a skill file is organized so an AI agent can find, load, and follow it — Human Flat vs Dir, folder and file naming, the top-level section set, cross-skill links, supporting files, and ADRs; the entry point that also requires skill-content and skill-tags
whenToUse: when you create a new skill, or change how one is organized — its Human Flat/Dir format, file and folder layout, top-level sections, cross-skill links, or ADRs
updated: 20260913
tags:
  - skill/core
  - stack
  - concern/documentation
adr:
  - adr/cross-skill-links-scope.md
  - adr/allow-extra-top-level-sections.md
  - adr/stack-specific-links-direction.md
---

# Goal
- A skill in the correct format (Human Flat or Human Dir) whose folder and main-file names both match the `name` field.
- Frontmatter carrying a concrete `whenToUse`, `updated: YYYYMMDD`, and — per [[skills/common-workflow/skill-tags.skill/skill-tags.skill.md|skill-tags]] — facet tags.
- Exactly one `# Goal`, `# Core Principle`, `# Rule`, and `# Check list` — plus at most one optional `# Scope`, `# Workflow`, and `# Example`; `# Rule` using only `## MUST`/`## SHOULD`/`## MAY`.
- Every cross-skill link an input, a required sub-step, an applied standard/template, or an active prohibition — nothing that only runs after this skill's artifact is done.
- Every decision made while writing the skill recorded as an ADR in the owning skill's `adr/` folder.
- The `# Check list`s of [[skills/common-workflow/skill-content.skill/skill-content.skill.md|skill-content]] and [[skills/common-workflow/skill-tags.skill/skill-tags.skill.md|skill-tags]] also passed.

# Core Principle
- **Instructions you could execute** - Write every skill as the instructions you would need to do the task yourself; if `whenToUse` alone does not tell you when the skill applies, it is not clear enough.
- **Reader is an agent** - Agent clarity and convenience are the measure — a skill that forces the agent to guess, or is too large to skim, is rewritten or split.
- **Three skills, one baseline** - Every skill-writing task satisfies this skill (organization), [[skills/common-workflow/skill-content.skill/skill-content.skill.md|skill-content]] (how the text reads), and [[skills/common-workflow/skill-tags.skill/skill-tags.skill.md|skill-tags]] (frontmatter tags); a domain-specific skill or template exempts none of the three.
- **Fixed top-level section set** - The allowed top-level sections are `# Goal`, `# Core Principle`, `# Rule`, `# Check list`, plus at most one optional `# Scope`, `# Workflow`, and `# Example` — see [ADR: allow-extra-top-level-sections](./adr/allow-extra-top-level-sections.md) for the trade-offs.
- Write skills in English.

# Rule

## MUST

### Also satisfy skill-content and skill-tags
Apply [[skills/common-workflow/skill-content.skill/skill-content.skill.md|skill-content]] and [[skills/common-workflow/skill-tags.skill/skill-tags.skill.md|skill-tags]] on every skill you create or change, and run their `# Check list`s alongside this one.
- Risk: a skill can pass this skill's structural checks while its prose is padded and unscannable, or its tags leave it invisible to the queries that should surface it.
- Fix: treat the three as one baseline — organization here, prose in skill-content, tags in skill-tags.

### This skill is the baseline
Treat this baseline as mandatory for every skill-writing task, even when a domain-specific skill provides its own template or workflow.
- Violation: "I am following `solution-create.skill`, so I do not need to check `skill-design`."
- Risk: the resulting skill may have vague `whenToUse`, broken links, a missing checklist, an inconsistent format, or instructions hard for an agent to apply — none of which the domain-specific skill checks for.
- Fix: use the domain-specific skill for specialized guidance; run this baseline's `# Check list` against the result regardless.

### Generic template as fallback only
Start from [skill.template.md](./templates/skill.template.md) only when no domain-specific template or skill exists for the skill you are writing.
- Risk: reinventing structure ad hoc when a domain-specific template already exists produces a skill inconsistent with its siblings.
- Fix: check for a domain-specific template/skill first; fall back to the generic template only when none exists.

### One of each top-level section
Use exactly one `# Goal`, one `# Core Principle`, one `# Rule`, and one `# Check list` top-level section — never repeat one for a sub-topic, never nest one inside another heading (a `## Rule` under `# Workflow`, a `### MUST` under `## Rule`).
- Risk: a repeated `# Rule` block, or a `MUST` nested at the wrong level, makes an agent scanning for `## MUST` miss requirements.
- Fix: if a skill has two conditionally-triggered halves, split it into two skills (see [One trigger per skill](#one-trigger-per-skill)) instead of repeating sections.

### Optional Scope, Workflow, Example only
Beyond the mandatory four, add at most one `# Scope`, at most one `# Workflow`, and at most one `# Example` top-level section — no other top-level section is allowed. `# Scope` states only what the skill covers and does not cover. `# Workflow` only describes the process; every normative requirement in it must also appear as a `## MUST`/`## SHOULD`/`## MAY` bullet under `# Rule`. `# Example` contains only one-line links to files inside the skill's own `examples/` or `templates/` folder — never inline code blocks or tables longer than ~15 lines. Decision recorded in [adr/allow-extra-top-level-sections.md](./adr/allow-extra-top-level-sections.md).
- Violation: a `# Scope` section drifts into rules, a `# Workflow` section carries its own `## MUST` rules, an `# Example` section holds a multi-line code block, or a second `# Scope`/`# Workflow`/`# Example` section appears.
- Risk: an agent scanning `## MUST` under `# Rule` misses requirements buried elsewhere, and scope/workflow sections become a shadow rule set.
- Fix: keep `# Scope` to coverage boundaries, move every normative requirement to a `## MUST`/`## SHOULD`/`## MAY` bullet, keep `# Example` to one-line links, and use each optional section at most once.

### Only MUST, SHOULD, MAY
Under `# Rule`, use only `## MUST`, `## SHOULD`, and `## MAY` subsections; express a prohibition as a negatively-phrased rule ("Never...", "Do not...") inside one of them at its actual strength, never as a `## MUST NOT`/`## SHOULD NOT` heading or a separate `# Anti-patterns` section.
- Risk: without one convention, some skills carry a separate anti-pattern narrative and readers guess a rule's strength from inconsistent section names across the repository.
- Fix: pick strength (MUST/SHOULD/MAY) by how mandatory the rule is, polarity (positive/negative) by wording alone — the heading only names the strength.

### Pick Flat or Dir
Choose the skill's format: **Human Flat** — a single file `{skill-name}.skill.md`, for a self-contained skill with no supporting files; **Human Dir** — a folder `{skill-name}.skill/` containing `{skill-name}.skill.md`, when the skill references its own templates, examples, diagrams, or ADRs.
- Violation: a flat skill that also creates a `templates/` folder beside it without converting to Human Dir.
- Risk: files scattered outside the single file are never discovered by an agent following just that file.
- Fix: convert to Human Dir the moment the skill needs a supporting file.

### Flat means one file
Never place a supporting file for a Human Flat skill outside its single markdown file.
- Risk: a file outside the single file defeats the reason for choosing Flat — nothing points to it.
- Fix: convert to Human Dir and put the supporting file inside `{skill-name}.skill/`.

### Supporting files stay inside
For a Human Dir skill, keep every referenced supporting file inside the skill folder.
- Risk: a supporting file outside the folder can be moved, renamed, or deleted independently, silently breaking the skill's links.
- Fix: keep every template, example, and ADR the skill references inside `{skill-name}.skill/`.

### Folder and file names match the skill name
Match the folder name and the main file name exactly to the skill's `name`: `{skill-name}.skill/{skill-name}.skill.md`.
- Risk: tooling and cross-skill links that assume this pattern cannot resolve the file.
- Fix: name both after the `name` field, exactly.

### whenToUse names concrete triggers
Write `whenToUse` as the concrete situations that must make an agent apply the skill, decidable from that sentence alone — never vague text like "when needed" or "for best practices".
- Violation: "Use this skill for best practices."
- Risk: the agent cannot decide whether the skill applies to the current task.
- Fix: name the tasks or situations — "when you add logging to code or choose a log level".

### Every skill carries its change date
Keep `updated: YYYYMMDD` (compact, no separators) in every skill's frontmatter and bump it in the same commit that changes the skill.
- Risk: without a bumped date, a changed skill — or a changed standard — triggers no re-validation, and the validation queue trusts a stale file.
- Fix: bump `updated` on every change; the skill-validation pass derives staleness from it.

### Links resolve from the skill file
Use links resolvable from the skill file — relative to the file (`[label](./path/file.md)`, `[[./path/file.md|label]]`) or to the repository root (`[label](skills/.../file.md)`, `[[skills/.../file.md|label]]`).
- Violation: `[template](C:\Users\...\skill.template.md)`, or a bare `[[skill.template.md]]` used from a different folder.
- Risk: the agent cannot find the related file.
- Fix: use a skill-file-relative or repository-root-relative link.

### One link syntax per skill
Use wikilinks or standard markdown links consistently within one skill file.
- Risk: a file mixing both leaves some links unstyled or unresolved by tooling that renders only one.
- Fix: pick one syntax per file and use it throughout.

### Examples live in the owning skill
Never link another skill's file as an example for this one.
- Violation: `See [some-example](../other-skill.skill/other-skill.skill.md) for an example.`
- Risk: an unnecessary dependency on a skill with no real relationship — it can be renamed or removed independently, breaking this skill's example.
- Fix: put the example in this skill's own `examples/` folder (Human Dir) and link `[example](./examples/example.md)`.

### One trigger per skill
Never bundle two independently-triggered procedures into one skill; if `description`/`whenToUse` needs "plus/also/and separately" to introduce a second condition-gated capability, split into two skills cross-linked via wikilinks.
- Violation: a PR-validation skill whose `whenToUse` reads "...or when a project following X needs Y wired in", with a second `# Rule`/`# Check list` block gated by "if the project follows X".
- Risk: the agent must filter every rule by an invisible precondition instead of trusting that everything in the file applies; duplicated headings drift; the file grows too large to skim.
- Fix: a base skill with the unconditional rules, and an extension skill whose `whenToUse` states the precondition ("when a project following `[[other-skill]]` needs..."); each keeps its own single `# Rule`/`# Check list`.

### Link what the artifact needs, not what comes after it
Link a skill this one needs to finish its own artifact — an input it reads, a sub-step it must run (even when that sub-step is its own skill), a standard or template it applies — and an active "do not apply X here" prohibition; never link a skill that only runs once this skill's artifact is already complete (the next stage of a pipeline, a consumer of the output) or one named only to mark a topic out of scope. Sequencing across finished artifacts lives in the pipeline's non-skill `README.md`, which the loader does not pull in. Decision recorded in [adr/cross-skill-links-scope.md](./adr/cross-skill-links-scope.md).
- Test: could an agent complete this skill's artifact without knowing the linked skill? If yes, drop the link; if no, keep it.
- Violation: a Goal or Core Principle bullet reading "once the map is done, proceed to `[[later-stage]]`", or "the X view is not here — see `[[other-skill]]`".
- Risk: the skill loader pulls every linked skill into the agent's context, so a link to a later stage or a "not my job" pointer inflates every load while buying nothing the current task needs; non-goals are unbounded, so each new neighbour adds another.
- Fix: keep links for inputs, required sub-steps, and applied standards; name a later stage or an out-of-scope boundary in plain words.

### Stack-agnostic skills never link their stack-specialized extensions
A stack-agnostic skill (bare `stack` tag) never wikilinks/markdown-links a stack-specialized skill that extends it (one `stack/<value>` tag) — name it only as plain backticked text, and recommend asking the user which one to load for the stack in use. A stack-specialized skill still links back to the stack-agnostic base it extends, per [Link what the artifact needs, not what comes after it](#link-what-the-artifact-needs-not-what-comes-after-it). Decision recorded in [[./adr/stack-specific-links-direction.md|stack-specific-links-direction]].
- Violation: `cucmber-testing`'s `# Scope` linking `[[skills/go/testing/cucmber-testing-in-go.skill.md|cucmber-testing-in-go]]`, `-in-dotnet`, `-in-python`, and `-in-typescript`.
- Risk: `ai-skill-manager` resolves every link a loaded skill carries, so linking all stack-specialized extensions from the agnostic skill pulls every other stack's skill into a project that only uses one of them.
- Fix: write `` `cucmber-testing-in-go`, `cucmber-testing-in-dotnet`, `cucmber-testing-in-python`, `cucmber-testing-in-typescript` `` as plain text, and add a rule/note telling the agent to ask the user which one to load for the project's stack.

### Record decisions as ADRs
When a choice between considered variants — each with real benefits and costs — is made while writing or updating a skill, record it as an ADR following [adr-create.skill.md](skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md), inside the skill folder that owns the decision.
- Violation: choosing between approaches during the session and moving on without an ADR file, leaving the reasoning only in conversation history.
- Risk: the rejected alternatives and trade-offs are lost, and the decision gets re-argued the next time someone touches the skill.
- Fix: create the ADR immediately, register it in the skill's `adr:` YAML property, and link it from the skill body.

## SHOULD

### Always ship a check list
Provide a `# Check list` so the agent can verify it followed the skill.

### Short skills over monoliths
Prefer short, focused skills over large monolithic ones — split when two parts have independent triggers and independent reasons to change.

### One question per section
Give each top-level section a single responsibility, and extract a reference vocabulary (a closed list of relation types, a set of layout mechanics) into its own named section when several rules or workflow steps cite it.
- Risk: a section mixing layout mechanics with a reference vocabulary forces the reader to hold two kinds of knowledge at once, and every citation points into the middle of an unrelated block.
- Fix: one section per reader question — what (`# Goal`), how to think (`# Core Principle`), where artifacts live, how to build, the contract (`# Rule`), verification (`# Check list`); a shared vocabulary gets its own anchored section.

### No machine-local paths
Never use an absolute file-system path or a URL that depends on the local machine.
- Risk: a path that resolves only on the author's checkout is broken for every other agent or contributor.
- Fix: use paths relative to the skill file or the repository root.

## MAY

### Supporting files welcome
Add diagrams, templates, or ADRs inside the skill folder when they make the skill easier to apply.

# Check list
- [ ] The skill uses the correct format (Human Flat or Human Dir); folder and main-file names both match `name`.
- [ ] Frontmatter is filled: concrete `whenToUse`, `updated: YYYYMMDD` present and bumped this change.
- [ ] Exactly one `# Goal`, `# Core Principle`, `# Rule`, and `# Check list`; none repeated or nested; `## MUST`/`## SHOULD`/`## MAY` are the only `##` under `# Rule`.
- [ ] Any extra top-level section is one of at most one `# Scope`, `# Workflow`, `# Example`, each within its constraints; no other top-level section exists.
- [ ] No `## MUST NOT`/`## SHOULD NOT` heading and no `# Anti-patterns` section anywhere; prohibitions are negative bullets inside MUST/SHOULD.
- [ ] All links resolve from the skill file or repository root; one link syntax used throughout.
- [ ] All supporting files are inside the skill folder (Human Dir); a Human Flat skill has none.
- [ ] Every cross-skill link is an input, a required sub-step, an applied standard/template, or an active prohibition — an agent could not finish this skill's artifact without it; no link to a later pipeline stage, a consumer, or an out-of-scope topic.
- [ ] A stack-agnostic skill names its stack-specialized extensions only as plain backticked text, never a link; a stack-specialized skill still links its stack-agnostic base.
- [ ] Examples referenced by this skill live in its own `examples/` folder, not another skill.
- [ ] `description`/`whenToUse` does not join two independently-triggered procedures with "plus/also/and separately".
- [ ] Every decision made while writing this skill is an ADR following [adr-create](skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md), registered in `adr:` and linked from the body.
- [ ] [[skills/common-workflow/skill-content.skill/skill-content.skill.md|skill-content]]'s `# Check list` passes for this skill's prose.
- [ ] [[skills/common-workflow/skill-tags.skill/skill-tags.skill.md|skill-tags]]'s `# Check list` passes for this skill's tags.
