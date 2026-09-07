---
name: skill-tags
description: The controlled facet vocabulary for a skill's frontmatter tags — the namespaces (stack, framework, app-type, artifact, concern, solution, element), the rules for combining them, and the self-check before agreeing a new value — so a skillset resolves by boolean tag-expression query instead of a hand-maintained path list
whenToUse: when you set or review a skill's frontmatter `tags:` list, or when a new facet value needs to be agreed and registered
updated: 20260906
tags:
  - skill/core
  - stack
  - concern/documentation
---

# Goal
- A `tags:` list carrying at least one `concern/*` value and exactly one `stack/<value>` tag, or the bare `stack` tag for a stack-agnostic skill.
- Every applicable `framework/*`, `app-type/*`, and `artifact/*` facet present as its own tag — never two facets chained into one `/`-path.
- Every nested facet value (`concern/testing/unit`) accompanied by its parent (`concern/testing`) as a separate tag.
- Any newly agreed facet value registered as a row in [facet-vocabulary.csv](./facet-vocabulary.csv).

# Core Principle
- **Tags resolve the skillset** - Tags exist so an agent's skillset is resolved by a boolean tag-expression query (`stack/typescript & concern/testing`, via `ai-skill-manager`'s filter — `&`, `|`, `!`, grouping, `facet/*` wildcards), not a hand-maintained list of folder paths.
- **One facet, one axis** - Each facet is an independent axis; combine facets by listing several tags on the skill and joining them with `&` in a query — never by chaining `angular/component` or `stack/dotnet/service`.
- **The engine expands queries down, not tags up** - A query term expands into its own sub-segments, but a skill's own tags are never expanded upward — so a nested value must carry its parent value explicitly as a second tag.
- **CSV is the value list** - [facet-vocabulary.csv](./facet-vocabulary.csv) holds one row per agreed value; this skill defines what each namespace means and how values combine, not the list itself.

# Facet namespaces

## `stack/*`
The language/ecosystem a skill's content is written for. Mirrors the top-level `skills/` folder names. Agreed values: rows with facet `stack` in [facet-vocabulary.csv](./facet-vocabulary.csv).
- A skill tied to exactly one stack carries exactly one `stack/<value>` tag.
- A stack-agnostic skill carries the bare tag `stack` (no value) instead of omitting it — so `stack/typescript & concern/testing` still matches it, and `stack & concern/testing` selects only the generic skills.
- `dotnet` names the ecosystem, not the language — C#, F#, VB.NET alike. Never add a competing language-name tag (`csharp`) for the same axis.

## `framework/*`
A concrete named framework/library layered on top of a stack. Independent from `stack` — combine both. Agreed values: rows with facet `framework` in the CSV.
- Add a value only when a skill's content is genuinely about that concrete framework/library, not a general application shape (`app-type/*` is for that). A skill about ASP.NET Core routing/middleware gets `framework/aspnet-core`, not `framework/dotnetCore` — a runtime generation is not a framework you build with, the same way Node.js is a runtime not a framework.

## `app-type/*`
The kind of program being built — orthogonal to `stack` and `framework`, because the same shape recurs across every stack. Agreed values: rows with facet `app-type` in the CSV.

## `artifact/*`
The concrete program element or deliverable the skill is about — orthogonal to `stack`/`framework`/`app-type`, because the same element appears in every ecosystem. Values so far: `class`, `interface`, `component`, `template`, `project`, `solution` (`.sln` file). Use it when the skill's content is tied to a specific kind of element rather than a general concern or application shape.

## `solution/*`
The concrete solution skill a file belongs to — the owning `solution-{name}.skill/` folder. Resolves every file of one solution with a single query term.
- Value: the folder name without the `solution-` prefix and `.skill` suffix, kebab-case (`solution-sln-structure.skill` → `solution/sln-structure`).
- Every file inside a solution skill folder carries exactly one `solution/<value>` tag: the main skill file, every `Implementation/` file, every `adr/` file.
- Values are dynamic (one per solution), so the pattern — not each value — is registered in the CSV.

## `element/*`
The concrete program element an `Implementation/` file of a solution skill describes. Complements `artifact/*`: `artifact/*` names the *kind*, `element/*` names the *concrete* element.
- Value: the element file name, lowercased kebab-case, every run of non-alphanumerics collapsed to one dash (`Shared.csproj` → `element/shared-csproj`, `{Module}.Domain.csproj` → `element/module-domain-csproj`, `{feature}.facade.ts` → `element/feature-facade-ts`).
- Carried by `Implementation/` files alongside `solution/<value>`. Dynamic — the pattern is registered in the CSV, not each value.

## `concern/*`
What aspect of building software the skill addresses. Multi-value is expected — a skill that sets up a testing quality gate through architectural changes carries both `concern/architecture` and `concern/testing`. Agreed values: rows with facet `concern` in the CSV.

## `skill/*`, `plateau/*` — reserved
Owned by the `plateau-create-by-solutions`/`solution-create` skill-generation tooling (structural role of a generated element, which plateau instance it belongs to). Not part of this facet system — never repurpose or restructure them.

## `workflow/*` — reserved, not populated
Reserved for a future facet describing process-orchestration skills (a skill that walks an agent through a chain of steps A→B, e.g. a CI/CD pipeline). Do not invent values under it until that facet is planned separately.

# Self-check before a new facet value
Run these five questions before adding a value to an existing facet, or introducing a new facet; once a value passes and is agreed, register it as a CSV row.
1. **is-a test** — can you honestly say "this value is a kind of [facet name]"? If not, it belongs to a different facet.
2. **no-parent test** — would this value ever appear without the facet you are about to nest it under? If yes, it is an independent facet, not a child value.
3. **order-flips test** — if you are unsure which of two words comes first in a chain, that uncertainty means you have two facets, not one hierarchy.
4. **combinatorial-growth test** — would the list of legal combinations multiply (not just add one) as new stacks/frameworks appear? If yes, split into separate facets.
5. **containment test** — is the "parent" a concrete container (a specific module contains this specific class) rather than an abstract category? If so, it is a structural link between specific skills, not a facet value at all.

# Rule

## MUST

### At least one concern tag
Give every skill at least one `concern/*` tag, and more than one when it genuinely spans several aspects.
- Risk: a query for a concern misses a skill that never declared it, so the skill is invisible to the agents that need it.
- Fix: add every `concern/*` value the skill's content addresses.

### Exactly one stack tag
Give every skill either exactly one `stack/<value>` tag or the bare `stack` tag — never both, never neither.
- Violation: omitting the tag entirely on a stack-agnostic skill.
- Risk: with no `stack` tag, `stack/typescript & concern/testing` cannot match a generic skill it should; with two, the axis is ambiguous.
- Fix: one `stack/<value>` for a stack-specific skill; the bare `stack` for a stack-agnostic one.

### Never chain two facets
Give each facet its own tag; never combine two different facets into one `/`-chain.
- Violation: `angular/component` (framework + artifact in one chain) or `stack/dotnet/service` (stack + app-type).
- Risk: the query engine treats the chain as one facet's nested value, so neither `framework/angular` nor `artifact/component` matches.
- Fix: separate tags (`framework/angular`, `artifact/component`); combine them in a query with `&`.

### Duplicate a nested value's parent
When tagging a nested value within one facet (`concern/testing/unit`), also add the parent value (`concern/testing`) as its own tag.
- Risk: the query engine expands a query term downward but never expands a skill's own tags upward, so a query for `concern/testing` misses a skill that only carries `concern/testing/unit`.
- Fix: add both the nested value and its parent as separate tags.

### Register agreed values in the CSV
Register every agreed facet value as a row in [facet-vocabulary.csv](./facet-vocabulary.csv); a value that exists only in prose is not part of the vocabulary.
- Risk: unregistered values drift — two skills spell the same concept differently and no query matches both.
- Fix: add the CSV row when the value is agreed; the dynamic facets (`solution/*`, `element/*`) register the pattern, not each value.

### Leave reserved and free-form tags alone
Never repurpose `skill/*` or `plateau/*`, and never fold a fine-grained technical keyword (`xunit`, `mediatr`) into the facet vocabulary.
- Risk: repurposing a tooling-owned namespace breaks skill generation; forcing a keyword into a facet pollutes the controlled vocabulary.
- Fix: leave `skill/*`/`plateau/*` untouched; keep free-form keywords as plain extra tags outside this vocabulary.

### Self-check before a new value
Run the five questions in [# Self-check before a new facet value](#self-check-before-a-new-facet-value) before adding any value to an existing facet or introducing a new facet.
- Risk: a value that fails the is-a or no-parent test creates a false hierarchy that queries cannot navigate.
- Fix: run all five; when one fails, the value belongs to a different facet, is an independent facet, or is a structural link — not a nested value.

## SHOULD

### Free-form tags beyond facets
Add free-form tags beyond the required facet tags (`xunit`, `mediatr`) when they help a reader skim the skill's topic; keep them outside the controlled vocabulary in the CSV.

# Check list
- [ ] The skill carries at least one `concern/*` tag.
- [ ] The skill carries exactly one `stack/<value>` tag or the bare `stack` tag — not both, not neither.
- [ ] No tag chains two different facets into one `/`-path.
- [ ] Every nested facet value is accompanied by its parent value as a separate tag.
- [ ] Every newly agreed value is a row in [facet-vocabulary.csv](./facet-vocabulary.csv); `solution/*`/`element/*` register the pattern only.
- [ ] `skill/*`/`plateau/*` are untouched; free-form keywords sit outside the facet vocabulary.
- [ ] Any new facet value passed all five self-check questions before being added.
