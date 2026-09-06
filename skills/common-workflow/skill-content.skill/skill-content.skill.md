---
name: skill-content
description: How the prose inside a skill file must read — Goal as a deliverables list, rules as named headings with one imperative sentence and Violation/Risk/Fix, every fact stated once, no authoring narrative, illustrations moved out
whenToUse: when you write or revise the text of a skill — the Goal and Core Principle bullets, the rule statements and their Violation/Risk/Fix elaboration — or when you tighten an existing skill for concision
updated: 20260906
tags:
  - skill/core
  - stack
  - concern/documentation
adr:
  - adr/rules-as-headings.md
---

# Goal
- A `# Goal` section listing only verifiable outputs — the artifacts and verdicts that exist after the skill is applied.
- Every rule under `# Rule` written as a `###` heading: a decodable name, one imperative sentence, then `Violation`/`Risk`/`Fix` bullets.
- Every fact, mechanism, or criterion stated exactly once; every other mention a reference link to the one statement.
- Every `# Goal`/`# Core Principle` bullet over ~20 words prefixed `**{Name}** - ` with a name a reader decodes without the skill's history.
- Illustrations over ~15 lines living in `examples/`/`templates/`; content that defines a rule or contract staying inline.
- No authoring narrative in justifications; no leftover `hint`/`example` blocks.

# Core Principle
- **Reader is an agent under load** - The reader is an AI agent skimming this file mid-task; every sentence competes for its attention, so a sentence that does not change what the agent does is removed.
- **State it once** - A fact lives in the one section or rule that owns it; every other place points to it, never restates it.
- **Statement, then reasoning** - A rule's paragraph says what to do; why it matters goes in `Risk`, the correction in `Fix` — never folded back into the statement.
- **Names carry the scan** - A bullet or rule name is the reader's index into the text; a name that needs backstory to decode, or that parrots the description, is worse than no name.

# Rule

## MUST

### Goal lists deliverables
Write `# Goal` as the list of verifiable outputs the skill produces — what exists after the skill is applied.
- Violation: a Goal bullet reading "make the split defensible against a written baseline" — a principle duplicated from `# Core Principle`, not an output; or "turn a fuzzy sense into a reviewable artifact" — a meta-intention, unverifiable.
- Risk: after applying the skill, its Goal cannot serve as acceptance criteria, and an idea stated in both `# Goal` and `# Core Principle` drifts.
- Fix: each Goal bullet names a concrete artifact or verdict the reader can check off; working principles move to `# Core Principle`, scope boundaries to a scope rule in plain words.

### Rules are headings
Write every rule under `# Rule` as a `###` heading — the rule's name — followed by the imperative statement as a plain paragraph, then its elaboration bullets.
- Risk: bullet-format rules are not addressable — workflow steps and checklists reference them by quoted name, findable only by text search — and a long rule section has no outline to skim; mixing both formats in one file gives the reader two patterns to parse.
- Fix: one `###` heading per rule, imperative paragraph under it; a skill you are otherwise touching migrates its whole `# Rule` section, never a single rule; a skill you are not touching keeps its legacy bullet format until then. Decision recorded in [adr/rules-as-headings.md](./adr/rules-as-headings.md).

### Rule statement is one imperative sentence
Keep the paragraph under a `###` rule heading to a single imperative sentence stating what to do; move consequences, rationale, and qualifiers into the `Risk`/`Fix` bullets.
- Violation: a rule statement that runs three sentences, half of them explaining why the rule exists or what breaks without it.
- Risk: reasoning mixed into the statement makes the agent parse an argument to extract the instruction, and the same reasoning then reappears in `Risk` — two copies to keep aligned.
- Fix: cut the statement to the imperative core; the "why" is `Risk`, the "how to comply" is `Fix`.

### Elaboration is Violation, Risk, Fix
Nest a rule's elaboration using exactly `Violation`/`Risk`/`Fix` bullets, defined relative to the violation: `Violation` (optional) is what not following the rule looks like — an omission or wrong attempt for a positive rule, the forbidden action for a negative one; `Risk` is what breaks; `Fix` is the correct action that replaces it.
- Risk: without a shared definition, "Fix" reads as "instead of the forbidden action" for a prohibition but has no obvious meaning for a positive rule, so authors invent incompatible interpretations.
- Fix: phrase `Risk`/`Fix` around "the violation described (or implied) by `Violation`"; every `## MUST` rule carries `Risk` and `Fix`, `## SHOULD` only when non-obvious, `## MAY` never.

### Name dense bullets
Prefix a bullet under `# Goal` or `# Core Principle` as `**{Name}** - {description}` when its own text exceeds ~20 words, where `{Name}` is a 2-4 word English noun phrase a reader decodes without the skill's history.
- Violation: a principle named "Baseline, not precedent", where "precedent" decodes only with history the reader lacks; or `**Format** - Use links resolvable from the skill file`, parroting the description's first word.
- Risk: a cryptic name forces re-reading the full text to recall the bullet's point; a parroting name adds a line with no scanning benefit.
- Fix: compress the bullet's actual point into a short name whose both halves the reader can see; leave short bullets and `# Check list` items unnamed.

### State each fact once
State every fact, mechanism, or criterion exactly once; everywhere else reference it by anchor link instead of restating it.
- Violation: the same mechanism described in a workflow step, a spec section, a rule, and the checklist — four copies to keep in sync.
- Risk: copies drift out of sync, and the reader must verify they agree before trusting any of them.
- Fix: the full statement lives in its owning section or rule; a short pointer (`[Rule name](#anchor)`) everywhere else — a workflow step points to the rule that governs a decision rather than embedding it.

### Rules are actionable
Write every rule, workflow step, and checklist item as an instruction to the agent — what to do and when — not a description of the topic for a human reader.
- Violation: "This skill explains the importance of clean code."
- Risk: the agent does not know what actions to take or when to take them.
- Fix: "Apply these rules when you create or refactor a class: ...".

### Illustrations move out, contracts stay inline
Move an illustrative code block or table longer than ~15 lines (a full runnable workflow/config file, a multi-step script, a sample end-to-end implementation) into `examples/` or `templates/` inside the skill's own folder, leaving a link with a one-line caption.
- Violation: a 120-line GitHub Actions YAML pasted under `# Example` instead of `./examples/<name>.example.md`; or the opposite — a `make`-target contract table moved to `examples/contract.md`, leaving `# Rule` saying only "see the example".
- Risk: an over-long example buries the rules the agent must skim; a moved-out contract forces the agent to open a second file to learn a rule it must follow.
- Fix: move only content that illustrates a rule; keep a code block, snippet, or table inline when it defines part of the rule/contract itself (a table of required fields, a 3-line config flag).

## SHOULD

### Justifications stay impersonal
Keep rule justifications free of authoring narrative ("found the hard way", "as happened here"); include at most one concrete precedent per `Risk`, phrased as a fact about the artifact, not the writing session.
- Risk: session narrative inflates the skill and buries the operative content — the reader learns about the author's past instead of the rule's boundary.
- Fix: keep the precedent, drop the story — "(`EntityBehaviour` was wrongly marked common this way)" carries the warning; "we learned this the hard way while building this skill" adds nothing.

### No leftover template hints
Remove every `hint`, `example`, and `code example` block, and the `# How Apply this template` section, from the final skill file.
- Risk: the final skill is noisy and the agent cannot tell binding rules from authoring aids.
- Fix: delete all such blocks before committing.

# Check list
- [ ] `# Goal` lists verifiable deliverables — no working principles, scope notes, or meta-intentions.
- [ ] Every rule under `# Rule` is a `###` heading followed by one imperative sentence, then `Violation`/`Risk`/`Fix`.
- [ ] No rule statement carries reasoning, consequences, or qualifiers that belong in `Risk`/`Fix`.
- [ ] Every `## MUST` rule has `Risk` and `Fix` (`Violation` optional); `## SHOULD` only where non-obvious; `## MAY` none.
- [ ] Any `# Goal`/`# Core Principle` bullet over ~20 words starts with `**{Name}** - `; the name is self-explanatory without backstory and not a restatement of the description's opening.
- [ ] No fact is stated in two places; other mentions are anchor links to the one statement.
- [ ] No inline code block or table over ~15 lines unless it defines part of the rule/contract; longer illustrations live in `examples/`/`templates/` with a one-line pointer.
- [ ] Justifications carry no authoring narrative; at most one precedent per `Risk`, stated as a fact about the artifact.
- [ ] No `hint`/`example`/`code example` block and no `# How Apply this template` section remain.
