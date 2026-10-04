---
name: skill-code-delivery
description: How code reaches the agent from a skill — a script it runs, a library the project installs, a file it copies verbatim, a template with identifier-only placeholders, or an inline instruction — chosen by what differs between the projects the skill is applied to
whenToUse: when a skill you create or revise contains a code block, a script, or a config file — inline or as a supporting file
updated: 20261004
tags:
  - skill/core
  - stack
  - concern/documentation
adr:
  - adr/code-delivery-as-separate-skill.md
  - adr/folder-per-delivery-form.md
---

# Goal
- Every code block, script, and config file of the skill assigned exactly one delivery form from [One delivery form per code unit](#one-delivery-form-per-code-unit).
- **Fixed code as files** - Code identical in every project stored as a real file under `scripts/` or `assets/`, or published as a library — executed or built once before the skill ships.
- **Templates with declared placeholders** - Code differing only in identifiers and literals stored under `templates/`, every placeholder listed in the skill with what to substitute.
- **One reference line** - Each moved file referenced from the skill by one line naming the action — run, install, copy verbatim, or fill and copy — with no inline copy left.
- **Inline only what varies** - Inline code remaining only where structure or logic differs between projects, or where the block is a fragment merged into a file the project already owns.

# Core Principle
- **Fixed code is delivered, not described** - Code an agent retypes from an example is re-derived on every run and comes out different each time; code it runs or copies is identical every time.
- **Variance decides the form** - What differs between two projects decides where code lives — not its length, and not whether it reads as an illustration.
- **Least freedom first** - Run beats install beats copy beats fill beats write: pick the form that leaves the agent the fewest decisions.
- **A false extraction is worse than an example** - Code shipped as "copy verbatim" that actually needs adapting lands unadapted in every project.

# Scope
- Covers code that ends up in a project file or is executed as a program, in a skill being created or revised; revising a skill classifies every code unit in it, not only the touched one.
- Does not cover a skill nobody is editing — its code stays as is until the skill is next revised. Decision recorded in [adr/code-delivery-as-separate-skill.md](./adr/code-delivery-as-separate-skill.md).
- Does not cover a command line the agent types, a diagram, or a sample of output.

# Workflow
1. List every code block, script, and config file the skill carries.
2. Run the [variance test](#variance-test-on-every-code-unit) on each.
3. Assign the form per [One delivery form per code unit](#one-delivery-form-per-code-unit); for the Library form, [ask first](#ask-before-creating-a-library).
4. Move the code into the form's folder as [a real file](#code-lives-in-its-real-file-type); for a template, [declare its placeholders](#placeholders-are-identifiers-and-literals-only).
5. [Execute or build](#delivered-code-runs-before-it-ships) what you moved.
6. Replace the inline code with [one reference line](#one-reference-line-no-inline-copy).

# Rule

## MUST

### Variance test on every code unit
For each code block, script, and config file in the skill, write down what would differ if the skill were applied to a second, unrelated project on the same stack.
- Violation: moving a block to `assets/` because it is long, or leaving one inline because it is short, with no list of what differs.
- Risk: without the list the form is picked by feel, so fixed code stays an example the agent reinterprets, or varying code is frozen into a file.
- Fix: name the differing parts explicitly — "nothing", "service name and port", "the set of handlers" — and pick the form from that answer.

### One delivery form per code unit
Assign each code unit the first form in this table whose row matches the variance-test answer.

| What differs between projects | What the project needs | Form | Lives in | Reference verb |
|---|---|---|---|---|
| nothing | only the effect — an output, a check, a generated file | Script | `scripts/` | Run |
| nothing | the code, updated later by a version bump | Library | a package registry, outside the skill | Install |
| nothing | the code, owned by the project after copying | Asset | `assets/` | Copy verbatim |
| identifiers and literals only — names, paths, descriptions, versions | the code | Template | `templates/` | Fill and copy |
| structure or logic; or the unit is a fragment merged into a file the project already owns | the code | Instruction | inline in the skill | — |

- Risk: a fixed unit left as an instruction is rewritten differently by every agent; a unit split across two forms has no single source.
- Fix: one form per unit, taken top-down from the table; `examples/` holds only illustrations the agent reads and never copies. Decision recorded in [adr/folder-per-delivery-form.md](./adr/folder-per-delivery-form.md).

### Never extract code whose structure varies
Never store a code unit as a script, asset, or template when the variance test names a difference in structure or logic.
- Violation: a `templates/handler.ts` whose handler list, error branches, or dependency set must be rewritten per project.
- Risk: the reference line says "copy", so the agent copies — and the project gets code built for a different project.
- Fix: keep it an instruction — rules stating what the code must do, plus an illustration handled per [skill-content](skills/design/skill-content.skill/skill-content.skill.md).

### Placeholders are identifiers and literals only
Write every template placeholder as `{kebab-case-name}` standing for one name, path, description, or version, and list each in the skill with what to substitute.
- Violation: `{optional-auth-block}` standing for a statement, a branch, or a block the agent may omit; or a placeholder token that also occurs in the file as real code.
- Risk: a placeholder that changes control flow turns the template back into an instruction no one can check by reading the filled values.
- Fix: split a structural variation into two templates or make the unit an instruction; rename a token that collides with the file's own syntax.

### Code lives in its real file type
Store every script, asset, and template as a file with its native extension, never as a fenced block inside a markdown file.
- Violation: `templates/unit-test.sh.md` holding a bash script inside a code fence.
- Risk: a fenced script cannot be executed, linted, or copied byte-for-byte, so the agent retypes it.
- Fix: `scripts/unit-test.sh` as a plain file; what the markdown wrapper explained moves to the reference line or a comment header in the file.

### Delivered code runs before it ships
Execute each script, build or lint each asset, and fill each template with sample values and build the result, before committing the skill.
- Risk: delivered code is copied unread, so one defect reaches every project that applies the skill.
- Fix: run it in the skill's own stack toolchain; when it cannot be run here, tell the user it is unverified.

### One reference line, no inline copy
Replace the moved code in the skill with one line: the form's verb, a link to the file, and — for an asset or template — the target path in the project.
- Violation: "See `[retry.ts](./assets/retry.ts)` for an example of the retry helper", or the file's content repeated under the link.
- Risk: "see … for an example" invites adaptation, and an inline copy drifts from the file.
- Fix: "Copy `[retry.ts](./assets/retry.ts)` verbatim to `src/shared/retry.ts`; do not modify it." — for a library, the install command with a pinned version.

### Ask before creating a library
Ask the user before choosing the Library form, and apply every other form without asking.
- Risk: a library is a new artifact with its own repository, release cycle, and owner; asking about each script, asset, and template stalls the authoring task on decisions that have one answer.
- Fix: state the unit, why a version-bump update path is worth it, and the Asset alternative; proceed with Asset until the user approves.

## SHOULD

### Source skill named in the delivered file
Start each asset and template with a comment naming the skill it came from.
- Risk: a copy with no origin cannot be traced back when the skill's file changes.
- Fix: one comment line in the file's own comment syntax — `# Source: skill {skill-name}`.

### Leftover-placeholder check in the consuming skill
Add a check-list item to the skill that owns a template: after filling, searching the target file for each listed placeholder returns nothing.

## MAY

### Category skills may map forms to their own folders
A category-specific skill (e.g. solution-create) may name its own folders for these forms; its layout wins for that category.

# Check list
- [ ] Every code block, script, and config file has a written variance-test answer.
- [ ] Each unit has exactly one form, taken top-down from the table in [One delivery form per code unit](#one-delivery-form-per-code-unit).
- [ ] No script, asset, or template contains code whose structure or logic differs between projects.
- [ ] Every template placeholder is `{kebab-case-name}`, stands for a name, path, description, or version, collides with nothing else in the file, and is listed in the skill.
- [ ] Every script, asset, and template is a file with its native extension — none wrapped in markdown.
- [ ] Each script was executed, each asset built or linted, each template filled with sample values and built — or the user was told it is unverified.
- [ ] Each moved unit is referenced by one line with the form's verb, a link, and the target path; no inline copy remains.
- [ ] A Library form was approved by the user; no other form was put to the user as a question.
- [ ] `examples/` holds only illustrations — nothing the skill tells the agent to copy.
