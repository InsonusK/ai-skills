---
name: skill-design
description: Rules for writing skills that AI agents can understand and apply correctly
whenToUse: when you create a new skill or update an existing one
updated: 20260906
tags:
  - skill/core
  - stack
  - concern/documentation
adr:
  - adr/rules-as-headings.md

---

# Goal
- Define how to describe skills so that an AI agent can decide when to use them and how to follow them.
- Standardize skill structure, naming, and cross-references across the repository.

# Scope
This skill applies to every skill-writing task in the repository. It defines the baseline goals, principles, and rules that must be followed when creating or updating any skill. If a domain-specific skill provides its own template or workflow, use it, but still satisfy the baseline requirements from this skill. Use the generic [skill.template.md](./templates/skill.template.md) only when no domain-specific template or skill exists.

# Core Principle
- Write every skill as instructions you would need to execute the task yourself.
- If you cannot tell when the skill applies by reading `whenToUse`, the skill is not clear enough.
- **Agent clarity and convenience are the key success factors.** Every rule, example, and checklist must make the skill easier for an AI agent to understand and apply. If a skill is confusing, hard to follow, or forces the agent to guess, rewrite or split it.
- Write skill in English

# Rule

## MUST

### Only MUST, SHOULD, MAY
Under `# Rule`, use only `## MUST`, `## SHOULD`, and `## MAY` subsections — never `## MUST NOT`/`## SHOULD NOT` headings. Express a prohibition as a negatively-phrased rule ("Never...", "Do not...") inside `## MUST` or `## SHOULD`, at whichever strength it actually carries; do not maintain a separate `# Anti-patterns` section.
- Risk: without one convention, some skills explain rules with a separate anti-pattern narrative while others don't, and readers have to guess whether a positively- or negatively-phrased rule is "critical" or "recommended" from inconsistent section names across the repository.
- Fix: pick the rule's strength (MUST/SHOULD/MAY) based on how mandatory it is, and its polarity (positive/negative) based on wording alone — the heading only ever names the strength.

### Rules are headings
Write every rule under `# Rule` as a `###` heading — the rule's name, a short noun phrase per the naming rule — followed by the imperative statement as a plain paragraph, followed by its elaboration bullets. A skill being created or updated uses this format; a skill you are not otherwise touching keeps its legacy bullet format until its next update — never mix both formats in one file. Decision recorded in [adr/rules-as-headings.md](./adr/rules-as-headings.md).
- Risk: bullet-format rules are not addressable — workflow steps and checklists reference them by quoted name, findable only by text search — and a long rule section has no outline to skim; mixing both formats in one file gives the reader two patterns to parse.
- Fix: one `###` heading per rule, imperative paragraph under it; when migrating a legacy skill, convert the whole file, never a single rule.

### Elaboration is Violation, Risk, Fix
Nest an elaboration under a rule using exactly `Violation`/`Risk`/`Fix` bullets, defined relative to the violation, not the rule's own polarity: `Violation` (optional) is what not following the rule looks like — an omission or wrong attempt for a positively-phrased rule, the forbidden action itself for a negatively-phrased one; `Risk` is what breaks because of that violation; `Fix` is the correct action that replaces it. Every `## MUST` rule requires `Risk` and `Fix` (`Violation` stays optional); `## SHOULD` rules carry the elaboration only when the rule is non-obvious; `## MAY` rules never carry it — permission has nothing to violate.
- Risk: without a shared definition, "Fix" reads as "instead of the forbidden action" for a prohibition but has no obvious meaning for a positively-phrased rule, so different skill authors invent different, incompatible interpretations.
- Fix: always phrase `Risk`/`Fix` around "the violation described (or implied) by `Violation`," which reads identically regardless of whether the rule itself is phrased as an obligation or a prohibition.

### Name dense bullets
Name a bullet under `# Goal` or `# Core Principle` when its own text exceeds ~20 words: prefix it as `**{Name}** - {description}`, where `{Name}` is a short English noun phrase (2-4 words) that a reader can decode without knowing the skill's history — name the contrast or mechanism in concrete terms, never an abstract label whose meaning lives in backstory, and never a near-restatement of the description's opening words. Rule names under `# Rule` follow the same discipline as `###` headings. Leave short bullets unnamed; `# Check list` items stay unnamed verification checks.
- Violation: a principle named "Baseline, not precedent", where "precedent" decodes only with history the reader does not have; or naming a bullet `**Format** - Use links that are resolvable from the skill file`, parroting the description's first word.
- Risk: a cryptic name forces re-reading the full text to recall what the bullet was about; a parroting name adds a line without any scanning benefit.
- Fix: compress the bullet's actual point into a short, self-explanatory name whose both halves the reader can see.

### One of each top-level section
Use exactly one `# Goal`, one `# Core Principle`, one `# Rule`, and one `# Check list` top-level section per skill file — never repeat a top-level section for a sub-topic, and never nest one inside another heading (a `## Rule` under `# Workflow`, or a `### MUST` under `## Rule`). Under `# Rule`, the only `##` headings are `## MUST`, `## SHOULD`, `## MAY`; rule names sit at `###`.
- Risk: a repeated `# Rule` block, or a `MUST` subsection nested at `###` somewhere downstream, makes an agent scanning for "## MUST" miss requirements that exist under the wrong heading level.
- Fix: if a skill has two conditionally-triggered halves, split it into two skills (see One trigger per skill) instead of repeating sections in one file.

### Goal lists deliverables
Write `# Goal` as the list of verifiable outputs the skill produces — what exists after the skill is applied — never meta-intentions ("turn a fuzzy sense into a reviewable artifact"), working principles (those belong to `# Core Principle`), or scope boundaries (those belong to a scope rule or a downstream skill's link).
- Violation: a Goal bullet reading "make the split defensible against a written baseline" — a principle duplicated from `# Core Principle`, not an output.
- Risk: after applying the skill, its Goal cannot serve as acceptance criteria — a fuzzy intention is unverifiable — and the same idea stated in two sections drifts.
- Fix: each Goal bullet names a concrete artifact or verdict the reader can check off.

### State each fact once
State every fact, mechanism, or criterion exactly once in the skill; everywhere else, reference it by anchor link instead of restating. A workflow step points to the rule that governs a decision instead of embedding the rule's detail.
- Violation: the same mechanism described in a workflow step, a spec section, a rule, and the checklist — four copies to keep in sync.
- Risk: copies drift out of sync, and the reader must verify they agree before trusting any of them.
- Fix: the full statement lives in the owning section or rule; a short pointer (`[Rule name](#anchor)`) everywhere else.

### This skill is the baseline
Treat this skill as the baseline for every skill-writing task, even when a domain-specific skill provides its own template or workflow.
- Violation: "I am following `solution-create.skill`, so I do not need to check `skill-design`."
- Risk: the resulting skill may have vague `whenToUse`, broken links, a missing checklist, an inconsistent format, or instructions that are hard for an agent to apply.
- Fix: use the domain-specific skill for specialized guidance, but verify that the baseline requirements from this skill are still met.

### Generic template as fallback only
Use [skill.template.md](./templates/skill.template.md) as the starting point only when no domain-specific template or skill exists for the skill you are writing.
- Risk: reinventing structure ad hoc when a domain-specific template already exists produces a skill inconsistent with its siblings.
- Fix: check for a domain-specific template/skill first; fall back to the generic template only when none exists.

### Domain skills don't exempt the baseline
When you follow a domain-specific skill, still satisfy the baseline requirements of this skill: clear `whenToUse`, actionable rules, valid links, correct format, and a filled `# Check list`.
- Risk: a skill can pass its domain-specific review while still having vague `whenToUse`, broken links, a missing checklist, or an inconsistent format — none of which the domain-specific skill checks for.
- Fix: run this skill's own `# Check list` against the result even after following a domain-specific skill.

### Pick Flat or Dir
Choose the correct skill format:
- **Human Flat**: a single file named `{skill-name}.skill.md`. Use for self-contained skills that do not need additional files.
- **Human Dir**: a folder named `{skill-name}.skill/` containing a file named `{skill-name}.skill.md`. Use when the skill references its own supporting files (templates, examples, diagrams, ADRs, etc.).
- Violation: a flat skill that also creates a `templates/` folder next to it without converting to Human Dir.
- Risk: files are scattered and the skill structure is unclear.
- Fix: convert to Human Dir when the skill needs supporting files.

### Flat means one file
Never put supporting files for a Human Flat skill outside the single markdown file.
- Risk: files placed outside the single file defeat the reason for choosing "Flat" — a reader or agent following just that file never discovers them.
- Fix: convert to Human Dir instead, and place the supporting files inside `{skill-name}.skill/`.

### Supporting files stay inside
For Human Dir skills, keep all referenced supporting files inside the skill folder.
- Risk: a supporting file placed outside the skill folder can be moved, renamed, or deleted independently of the skill, silently breaking its links.
- Fix: keep every template, example, and ADR the skill references inside `{skill-name}.skill/`.

### Folder and file names match the skill name
Match the folder name and the main skill file name exactly: `{skill-name}.skill/{skill-name}.skill.md`.
- Risk: tooling and cross-skill links that assume this exact pattern cannot resolve the file.
- Fix: name both the folder and the main file after the skill's `name` field, exactly.

### whenToUse names concrete triggers
Make `whenToUse` describe concrete trigger conditions, not vague marketing text like "when needed" or "for development". An agent must read it and know whether to apply the skill.
- Violation: "Use this skill for best practices."
- Risk: the agent cannot decide whether the skill applies to the current task.
- Fix: "Use this skill when you add logging to code or choose a log level."

### Every skill carries its change date
Keep `updated: YYYYMMDD` (compact date, no separators) in every skill's frontmatter and bump it on every change to the skill. Validation tooling ([skill-validation](skills/common-workflow/skill-validation.skill/skill-validation.skill.md)) derives queue staleness from these dates.
- Risk: without a bumped `updated` date, a changed skill — or a changed standard — triggers no re-validation, and the validation queue silently trusts a stale file.
- Fix: bump `updated` in the same commit that changes the skill.

### Rules are actionable
Keep the skill actionable: rules, workflows, and checklists must tell the agent exactly what to do, not describe the topic for a human reader.
- Violation: "This skill explains the importance of clean code."
- Risk: the agent does not know what actions to take or when to take them.
- Fix: "Apply these rules when you create or refactor a class: ..."

### Links resolve from the skill file
Use links that are resolvable from the skill file:
- Relative to the skill file: `[label](./path/to/file.md)` or `[[./path/to/file.md|label]]`.
- Relative to the repository root: `[label](skills/.../file.md)` or `[[skills/.../file.md|label]]`.
- Violation: `[template](C:\Users\...\skill.template.md)` or `[[skill.template.md]]` used from a different folder.
- Risk: the agent cannot find related files.
- Fix: use relative links from the skill file or repository-root-relative links.

### One link syntax per skill
Use wikilinks or standard markdown links consistently within one skill.
- Risk: mixing link syntaxes within one file means tooling that only renders one syntax leaves some links unstyled or unresolved.
- Fix: pick one syntax per skill file and use it throughout.

### Examples live in the owning skill
Never link to another skill's file as an example for this skill.
- Violation: `See [some-example](../other-skill.skill/other-skill.skill.md) for an example.`
- Risk: creates an unnecessary dependency between skills that have no real relationship — the other skill can be renamed, restructured, or removed independently, silently breaking this skill's example.
- Fix: create an `examples/` folder inside this skill's own folder (Human Dir), place the example there, and link to `[example](./examples/example.md)`.

### One trigger per skill
Never bundle two independently-triggered procedures into one skill just because they are related or often used together. If `description`/`whenToUse` needs "plus/also/and separately" to introduce a second condition-gated capability, split into two skills and cross-link them via wikilinks instead of branching the same `# Rule`/`# Check list` on that condition.
- Violation: a PR-validation skill whose `whenToUse` reads "...or when a project following X needs Y wired in", with a second `# Rule`/`# Check list` block gated by "if the project follows X" appended after the first.
- Risk: the agent must mentally filter every rule and checklist item by an invisible precondition instead of trusting that everything in the file applies; duplicated section headings drift out of sync, and the file grows too large to skim.
- Fix: split into a base skill carrying the unconditional rules, and an extension skill whose `whenToUse` states the precondition explicitly (e.g. "when a project following `[[other-skill]]` needs..."). Cross-link the two with wikilinks; each keeps its own single `# Rule`/`# Check list`.

### Illustrations move out, contracts stay inline
Move an illustrative code block or table (a full runnable workflow/config file, a multi-step script, a sample end-to-end implementation) longer than ~15 lines into `examples/` or `templates/` inside the skill's own folder (Human Dir), and leave only a link with a one-line caption of what it shows in the skill body. Keep a code block, snippet, or table inline when it defines part of the rule/contract itself (e.g. a table of required fields, a 3-line config flag) rather than merely illustrating one.
- Violation: a 120-line GitHub Actions YAML workflow pasted directly under `# Example` instead of `./examples/<name>.example.md` (should have been extracted); or, the opposite mistake, a `make`-target contract table moved into `examples/contract.md`, leaving `# Rule` say only "see the example" (should have stayed inline).
- Risk: in the first case, the example's length and formatting dominate the file, burying the actual rules the agent needs to skim; in the second, the agent must open a second file just to learn a rule it is required to follow.
- Fix: move only content that illustrates or demonstrates a rule; keep content that states or defines the rule itself inline.

### Tags follow the facet vocabulary
Tag every skill's frontmatter `tags:` using the facet vocabulary defined in [facet-vocabulary.md](./facet-vocabulary.md): at least one `concern/*` value, and either one `stack/<value>` tag or the bare `stack` tag for skills that apply regardless of stack. Add `framework/*`, `app-type/*`, and `artifact/*` tags when they apply. Never chain two different facets into one `/`-path, and when using a nested facet value also add its parent value as its own tag.
- Violation: tagging a skill `angular/component` (two different facets — framework and artifact — forced into one chain) instead of separate `framework/angular` and `artifact/component` tags; or tagging only `concern/testing/unit` without also adding `concern/testing`.
- Risk: an agent resolving its skillset with a tag-expression query (e.g. `stack/typescript & concern/testing`) silently misses the skill, or a query for the parent concern misses every skill that only carries the narrower child value — the skill becomes invisible to exactly the agents that need it.
- Fix: tag each facet independently, combine facets on one skill by adding multiple tags, and duplicate the parent tag whenever a nested value is used. Run the self-check in facet-vocabulary.md before inventing a new facet or value.

### Record decisions as ADRs
When an architecture decision is made while writing or updating this skill — a choice made between considered variants, each with real benefits and costs — record it as an ADR following [adr-create.skill.md](skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md), inside the skill folder that owns the decision.
- Violation: choosing between approaches during the writing session and moving on without creating an ADR file, leaving the reasoning only in conversation history.
- Risk: the rejected alternatives and the trade-offs behind the choice are lost, and the same decision gets re-opened and re-argued the next time someone touches the skill.
- Fix: create the ADR immediately following adr-create.skill, register it in the skill's `adr:` YAML property, and link it from the skill body.

## SHOULD

### Always ship a check list
Provide a `# Check list` so the agent can verify it has followed the skill.

### Free-form tags beyond facets
Add free-form tags beyond the required facet tags (e.g. `xunit`, `mediatr`) when they help a reader skim the skill's specific topic; keep them outside the controlled facet vocabulary in facet-vocabulary.md.

### Short skills over monoliths
Prefer short, focused skills over large monolithic ones.

### One question per section
Give each top-level section a single responsibility, and extract a reference vocabulary (a closed list of relation types, a set of layout mechanics) into its own named section when several rules or workflow steps cite it.
- Risk: a section that mixes layout mechanics with a reference vocabulary forces the reader to hold two kinds of knowledge at once, and every citation points into the middle of an unrelated block.
- Fix: one section per reader question — what (`# Goal`), how to think (`# Core Principle`), where artifacts live, how to build, the contract (`# Rule`), verification (`# Check list`); a shared vocabulary gets its own section with its own anchor.

### Justifications stay impersonal
Keep rule justifications free of authoring narrative ("found the hard way", "as happened here"); at most one concrete precedent per `Risk`, phrased as a fact about the artifact, not about the writing session.
- Risk: session narrative inflates the skill and buries the operative content — the reader learns about the author's past instead of the rule's boundary.
- Fix: keep the precedent, drop the story — "(`EntityBehaviour` was wrongly marked common this way)" carries the warning; "we learned this the hard way while building this skill" adds nothing.

### No machine-local paths
Do not use absolute file-system paths or URLs that depend on the local machine.
- Risk: a path or link that only resolves on the author's machine/checkout is broken for every other agent or contributor who opens the skill.
- Fix: use paths relative to the skill file or the repository root, as required under `## MUST`.

### No leftover template hints
Do not leave empty hint/example blocks in the final skill file.
- Violation: keeping `hint` and `example` blocks after filling the template.
- Risk: the final skill is noisy and harder to follow.
- Fix: remove all `hint`, `example`, and `code example` blocks, and the `# How Apply this template` section before committing.

## MAY

### Supporting files welcome
Add diagrams, templates, or ADRs inside the skill folder when they make the skill easier to apply.

# Check list
- [ ] The skill is written with the agent's understanding and convenience as the primary measure of quality.
- [ ] If a domain-specific skill/template is used, the baseline requirements of this skill are still satisfied.
- [ ] The skill uses the correct format (Human Flat or Human Dir).
- [ ] The skill file name and folder name match the `name` in the front matter.
- [ ] Front matter is filled: `whenToUse` clearly states when to apply the skill, and `updated: YYYYMMDD` is present and bumped on every change.
- [ ] All rules are actionable for an AI agent.
- [ ] All links are relative to the skill file or repository root and use markdown or wikilink syntax.
- [ ] All supporting files are inside the skill folder (for Human Dir).
- [ ] Template hints and example blocks are removed from the final skill.
- [ ] `# Check list` is filled; there is no separate `# Anti-patterns` section and no `## MUST NOT`/`## SHOULD NOT` heading anywhere.
- [ ] Every rule under `# Rule` is a `###` heading followed by the imperative statement as a paragraph; `## MUST`/`## SHOULD`/`## MAY` are the only `##` headings under `# Rule`.
- [ ] Every `## MUST` rule carries `Risk` and `Fix` bullets (`Violation` optional); `## SHOULD` carries them only where non-obvious; `## MAY` carries none.
- [ ] `# Goal` lists verifiable deliverables — no working principles, scope notes, or meta-intentions.
- [ ] No fact is stated in two places; workflow steps and prose reference rules by anchor link instead of restating them.
- [ ] Any `# Goal`/`# Core Principle` bullet over ~20 words starts with `**{Name}** - `; the name is self-explanatory without the skill's backstory and not a restatement of the description's opening; rule names as `###` headings follow the same discipline.
- [ ] Examples referenced by this skill live in this skill's own `examples/` folder, not in another skill.
- [ ] The skill contains exactly one `# Goal`, one `# Core Principle`, one `# Rule`, and one `# Check list` top-level section; none is repeated or nested under another heading.
- [ ] `description`/`whenToUse` does not join two independently-triggered procedures with "plus/also/and separately"; if it does, the skill has been split.
- [ ] No inline code block or table exceeds ~15 lines unless it defines part of the rule/contract itself; longer illustrative examples live in `examples/`/`templates/` with a one-line pointer.
- [ ] Tags follow the facet vocabulary in [facet-vocabulary.md](./facet-vocabulary.md): at least one `concern/*`, a `stack/*` value or the bare `stack` tag, no two facets chained in one `/`-path, and the parent tag duplicated alongside any nested value.
- [ ] Every architecture decision made while writing this skill is recorded as an ADR following [adr-create](skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md) and linked from the skill body.
