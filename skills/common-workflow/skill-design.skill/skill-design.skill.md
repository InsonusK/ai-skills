---
name: skill-design
description: Rules for writing skills that AI agents can understand and apply correctly
whenToUse: when you create a new skill or update an existing one
tags:
  - skill/core
  - stack
  - concern/documentation

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
- Under `# Rule`, use only `## MUST`, `## SHOULD`, and `## MAY` subsections — never `## MUST NOT`/`## SHOULD NOT` headings. Express a prohibition as a negatively-phrased bullet ("Never...", "Do not...") inside `## MUST` or `## SHOULD`, at whichever strength it actually carries; do not maintain a separate `# Anti-patterns` section.
  - Risk: without one convention, some skills explain rules with a separate anti-pattern narrative while others don't, and readers have to guess whether a positively- or negatively-phrased rule is "critical" or "recommended" from inconsistent section names across the repository.
  - Fix: pick the bullet's strength (MUST/SHOULD/MAY) based on how mandatory it is, and its polarity (positive/negative) based on wording alone — the heading only ever names the strength.
- Nest an elaboration directly under a `# Rule` bullet using exactly `Violation`/`Risk`/`Fix`, defined relative to the violation, not the rule's own polarity: `Violation` (optional) is what not following the rule looks like — an omission or wrong attempt for a positively-phrased rule, the forbidden action itself for a negatively-phrased one; `Risk` is what breaks because of that violation; `Fix` is the correct action that replaces it. Every `## MUST` bullet requires `Risk` and `Fix` (`Violation` stays optional); `## SHOULD` bullets carry the elaboration only when the rule is non-obvious; `## MAY` bullets never carry it — permission has nothing to violate.
  - Risk: without a shared definition, "Fix" reads as "instead of the forbidden action" for a prohibition but has no obvious meaning for a positively-phrased rule, so different skill authors invent different, incompatible interpretations.
  - Fix: always phrase `Risk`/`Fix` around "the violation described (or implied) by `Violation`," which reads identically regardless of whether the rule itself is phrased as an obligation or a prohibition.
- Use exactly one `# Goal`, one `# Core Principle`, one `# Rule`, and one `# Check list` top-level section per skill file — never repeat a top-level section for a sub-topic within the same skill. Keep the `## MUST`/`## SHOULD`/`## MAY` subsections under `# Rule` at a consistent `##` heading level throughout the skill; never drop a later occurrence to `###` or deeper.
  - Risk: a repeated `# Rule` block, or a `## MUST` that silently becomes `### MUST` further down the file, makes an agent scanning for "## MUST" miss requirements that exist under the wrong heading level.
  - Fix: if a skill has two conditionally-triggered halves, split it into two skills (see the bundling rule below) instead of repeating sections in one file.
- Treat this skill as the baseline for every skill-writing task, even when a domain-specific skill provides its own template or workflow.
  - Violation: "I am following `solution-create.skill`, so I do not need to check `skill-design`."
  - Risk: the resulting skill may have vague `whenToUse`, broken links, a missing checklist, an inconsistent format, or instructions that are hard for an agent to apply.
  - Fix: use the domain-specific skill for specialized guidance, but verify that the baseline requirements from this skill are still met.
- Use [skill.template.md](./templates/skill.template.md) as the starting point only when no domain-specific template or skill exists for the skill you are writing.
  - Risk: reinventing structure ad hoc when a domain-specific template already exists produces a skill inconsistent with its siblings.
  - Fix: check for a domain-specific template/skill first; fall back to the generic template only when none exists.
- When you follow a domain-specific skill, still satisfy the baseline requirements of this skill: clear `whenToUse`, actionable rules, valid links, correct format, and a filled `# Check list`.
  - Risk: a skill can pass its domain-specific review while still having vague `whenToUse`, broken links, a missing checklist, or an inconsistent format — none of which the domain-specific skill checks for.
  - Fix: run this skill's own `# Check list` against the result even after following a domain-specific skill.
- Choose the correct skill format:
  - **Human Flat**: a single file named `{skill-name}.skill.md`. Use for self-contained skills that do not need additional files.
  - **Human Dir**: a folder named `{skill-name}.skill/` containing a file named `{skill-name}.skill.md`. Use when the skill references its own supporting files (templates, examples, diagrams, ADRs, etc.).
  - Violation: a flat skill that also creates a `templates/` folder next to it without converting to Human Dir.
  - Risk: files are scattered and the skill structure is unclear.
  - Fix: convert to Human Dir when the skill needs supporting files.
- Never put supporting files for a Human Flat skill outside the single markdown file.
  - Risk: files placed outside the single file defeat the reason for choosing "Flat" — a reader or agent following just that file never discovers them.
  - Fix: convert to Human Dir instead, and place the supporting files inside `{skill-name}.skill/`.
- For Human Dir skills, keep all referenced supporting files inside the skill folder.
  - Risk: a supporting file placed outside the skill folder can be moved, renamed, or deleted independently of the skill, silently breaking its links.
  - Fix: keep every template, example, and ADR the skill references inside `{skill-name}.skill/`.
- Match the folder name and the main skill file name exactly: `{skill-name}.skill/{skill-name}.skill.md`.
  - Risk: tooling and cross-skill links that assume this exact pattern cannot resolve the file.
  - Fix: name both the folder and the main file after the skill's `name` field, exactly.
- Make `whenToUse` describe concrete trigger conditions, not vague marketing text like "when needed" or "for development". An agent must read it and know whether to apply the skill.
  - Violation: "Use this skill for best practices."
  - Risk: the agent cannot decide whether the skill applies to the current task.
  - Fix: "Use this skill when you add logging to code or choose a log level."
- Keep the skill actionable: rules, workflows, and checklists must tell the agent exactly what to do, not describe the topic for a human reader.
  - Violation: "This skill explains the importance of clean code."
  - Risk: the agent does not know what actions to take or when to take them.
  - Fix: "Apply these rules when you create or refactor a class: ..."
- Use links that are resolvable from the skill file:
  - Relative to the skill file: `[label](./path/to/file.md)` or `[[./path/to/file.md|label]]`.
  - Relative to the repository root: `[label](skills/.../file.md)` or `[[skills/.../file.md|label]]`.
  - Violation: `[template](C:\Users\...\skill.template.md)` or `[[skill.template.md]]` used from a different folder.
  - Risk: the agent cannot find related files.
  - Fix: use relative links from the skill file or repository-root-relative links.
- Use wikilinks or standard markdown links consistently within one skill.
  - Risk: mixing link syntaxes within one file means tooling that only renders one syntax leaves some links unstyled or unresolved.
  - Fix: pick one syntax per skill file and use it throughout.
- Never link to another skill's file as an example for this skill.
  - Violation: `See [some-example](../other-skill.skill/other-skill.skill.md) for an example.`
  - Risk: creates an unnecessary dependency between skills that have no real relationship — the other skill can be renamed, restructured, or removed independently, silently breaking this skill's example.
  - Fix: create an `examples/` folder inside this skill's own folder (Human Dir), place the example there, and link to `[example](./examples/example.md)`.
- Never bundle two independently-triggered procedures into one skill just because they are related or often used together. If `description`/`whenToUse` needs "plus/also/and separately" to introduce a second condition-gated capability, split into two skills and cross-link them via wikilinks instead of branching the same `# Rule`/`# Check list` on that condition.
  - Violation: a PR-validation skill whose `whenToUse` reads "...or when a project following X needs Y wired in", with a second `# Rule`/`# Check list` block gated by "if the project follows X" appended after the first.
  - Risk: the agent must mentally filter every rule and checklist item by an invisible precondition instead of trusting that everything in the file applies; duplicated section headings drift out of sync, and the file grows too large to skim.
  - Fix: split into a base skill carrying the unconditional rules, and an extension skill whose `whenToUse` states the precondition explicitly (e.g. "when a project following `[[other-skill]]` needs..."). Cross-link the two with wikilinks; each keeps its own single `# Rule`/`# Check list`.
- Move an illustrative code block or table (a full runnable workflow/config file, a multi-step script, a sample end-to-end implementation) longer than ~15 lines into `examples/` or `templates/` inside the skill's own folder (Human Dir), and leave only a link with a one-line caption of what it shows in the skill body. Keep a code block, snippet, or table inline when it defines part of the rule/contract itself (e.g. a table of required fields, a 3-line config flag) rather than merely illustrating one.
  - Violation: a 120-line GitHub Actions YAML workflow pasted directly under `# Example` instead of `./examples/<name>.example.md` (should have been extracted); or, the opposite mistake, a `make`-target contract table moved into `examples/contract.md`, leaving `# Rule` say only "see the example" (should have stayed inline).
  - Risk: in the first case, the example's length and formatting dominate the file, burying the actual rules the agent needs to skim; in the second, the agent must open a second file just to learn a rule it is required to follow.
  - Fix: move only content that illustrates or demonstrates a rule; keep content that states or defines the rule itself inline.
- Tag every skill's frontmatter `tags:` using the facet vocabulary defined in [facet-vocabulary.md](./facet-vocabulary.md): at least one `concern/*` value, and either one `stack/<value>` tag or the bare `stack` tag for skills that apply regardless of stack. Add `framework/*` and `app-type/*` tags when they apply. Never chain two different facets into one `/`-path, and when using a nested facet value also add its parent value as its own tag.
  - Violation: tagging a skill `angular/component` (two different facets — framework and app shape — forced into one chain) instead of separate `framework/angular` and `app-type/*` tags; or tagging only `concern/testing/unit` without also adding `concern/testing`.
  - Risk: an agent resolving its skillset with a tag-expression query (e.g. `stack/typescript & concern/testing`) silently misses the skill, or a query for the parent concern misses every skill that only carries the narrower child value — the skill becomes invisible to exactly the agents that need it.
  - Fix: tag each facet independently, combine facets on one skill by adding multiple tags, and duplicate the parent tag whenever a nested value is used. Run the self-check in facet-vocabulary.md before inventing a new facet or value.

## SHOULD
- Provide a `# Check list` so the agent can verify it has followed the skill.
- Add free-form tags beyond the required facet tags (e.g. `xunit`, `mediatr`) when they help a reader skim the skill's specific topic; keep them outside the controlled facet vocabulary in facet-vocabulary.md.
- Prefer short, focused skills over large monolithic ones.
- Do not use absolute file-system paths or URLs that depend on the local machine.
  - Risk: a path or link that only resolves on the author's machine/checkout is broken for every other agent or contributor who opens the skill.
  - Fix: use paths relative to the skill file or the repository root, as required under `## MUST`.
- Do not leave empty hint/example blocks in the final skill file.
  - Violation: keeping `hint` and `example` blocks after filling the template.
  - Risk: the final skill is noisy and harder to follow.
  - Fix: remove all `hint`, `example`, and `code example` blocks, and the `# How Apply this template` section before committing.

## MAY
- Add diagrams, templates, or ADRs inside the skill folder when they make the skill easier to apply.

# Check list
- [ ] The skill is written with the agent's understanding and convenience as the primary measure of quality.
- [ ] If a domain-specific skill/template is used, the baseline requirements of this skill are still satisfied.
- [ ] The skill uses the correct format (Human Flat or Human Dir).
- [ ] The skill file name and folder name match the `name` in the front matter.
- [ ] `whenToUse` clearly states when the skill should be applied.
- [ ] All rules are actionable for an AI agent.
- [ ] All links are relative to the skill file or repository root and use markdown or wikilink syntax.
- [ ] All supporting files are inside the skill folder (for Human Dir).
- [ ] Template hints and example blocks are removed from the final skill.
- [ ] `# Check list` is filled; there is no separate `# Anti-patterns` section and no `## MUST NOT`/`## SHOULD NOT` heading anywhere.
- [ ] Every `## MUST` bullet carries a nested `Risk` and `Fix` (`Violation` optional); `## SHOULD` carries them only where non-obvious; `## MAY` carries none.
- [ ] Examples referenced by this skill live in this skill's own `examples/` folder, not in another skill.
- [ ] The skill contains exactly one `# Rule` and one `# Check list` section, each at a consistent heading depth.
- [ ] `description`/`whenToUse` does not join two independently-triggered procedures with "plus/also/and separately"; if it does, the skill has been split.
- [ ] No inline code block or table exceeds ~15 lines unless it defines part of the rule/contract itself; longer illustrative examples live in `examples/`/`templates/` with a one-line pointer.
- [ ] Tags follow the facet vocabulary in [facet-vocabulary.md](./facet-vocabulary.md): at least one `concern/*`, a `stack/*` value or the bare `stack` tag, no two facets chained in one `/`-path, and the parent tag duplicated alongside any nested value.
