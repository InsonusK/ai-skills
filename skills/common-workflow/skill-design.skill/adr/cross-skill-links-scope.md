---
name: cross-skill-links-scope
description: Which cross-skill links a skill file may carry — its inputs and the standards it applies, plus an explicit "do not use X" prohibition; never a link to a consumer, a next pipeline step, or a topic marked out of scope
problem: A skill mentioning a neighbouring skill by name tends to link it regardless of why it is mentioned. The skill loader pulls every linked skill into the agent's context, so a link that buys the reader nothing functional still costs context — and non-goals are unbounded, so each new neighbour would add another such link.
decision: A skill links what it needs to finish its own artifact — the skill whose output it reads as input, a sub-step it must run (even when that sub-step is its own skill), and the standards/templates it applies — plus, deliberately kept, an active "do not apply X here" prohibition. It never links a skill that only runs once its artifact is complete (a later pipeline stage, a consumer of the output), or one named only to say a topic lives elsewhere. The test: could an agent finish this skill's artifact without knowing the linked skill? Sequencing across finished artifacts lives in the pipeline's non-skill README, which the loader does not pull in.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Skill files reference neighbouring skills for several unrelated reasons, and the default reflex is to wikilink every one of them. Two costs follow:

1. **Context.** The skill loader (`ai-skill-manager`) resolves a loaded skill's links and pulls the linked skills into the agent's working context. A link that only says "that is handled elsewhere" or "run this next" loads a whole skill the agent does not need for the task in front of it.
2. **Unbounded growth.** A skill's non-goals are open-ended — there is always another neighbouring concern it does *not* cover. If "not my job" links are allowed, every new sibling skill earns one, until the file references a large fraction of the repository.

The distinction that matters is *why* a skill is mentioned, and the test is one question: **could an agent complete this skill's artifact without knowing the linked skill?**

- `variability-map-create` reads the `feature-model.md` that `feature-map-create` produces — a required input, linked.
- `variability-map-create` cannot fill its `Realized by` column without running `delta-conflict-detection` — a required sub-step, its own skill, linked even though it "comes later" in reading order, because the map is not finished without it.
- `feature-map-create` saying "after this, run `variability-map-create`" makes it an orchestrator of a pipeline it is only one step of — `feature-model.md` is complete without knowing `variability-map-create`, so no link.
- A `# Core Principle` bullet saying "the plateau↔VP view is not here, see `plateau-map-create`" is a pure non-goal marker — no link.
- "This is not a Plateau Component — do not follow `plateau-component-create`" is an active instruction: the reader is told what *not* to do, and the link lets them check exactly what — linked.

# Selected variant

**Selected variant:** [[#Link inputs, required sub-steps, and standards]]

- A link stays when an agent could not finish this skill's artifact without the target — an input it reads, a sub-step it must run, a standard it applies.
- The active "do not use X" case is kept deliberately, despite being a boundary statement, because it carries an instruction and the reader needs to see what is being ruled out.
- Links to a later pipeline stage (something that runs only after this artifact is done), consumers, and non-goal markers are dropped: they cost context and buy nothing, and sequencing has a home the loader does not pull in — the pipeline's non-skill `README.md`.

# Searched variants

## Link inputs, required sub-steps, and standards

### Description
A skill may link: the skill whose artifact it reads as input ("use the `{artifact}` produced by `[[X]]`"); a sub-step it must run to finish its own artifact, even when that sub-step is its own skill (`fill this column via [[X]]`); a standard or template it applies (`[[skill-design]]`, `[[adr-create]]`, `[[mermaid-diagram]]`); and an active prohibition (`do not apply [[X]] here`). It must not link: a skill that only runs once this one's artifact is complete (a later pipeline stage, a consumer of the output); or a skill named only to mark a topic as out of scope. The test for every candidate link: could the agent complete this skill's artifact without knowing the target? Sequencing across finished artifacts lives in that pipeline's `README.md` — a plain index file, not a skill, that the loader does not resolve.

### Benefits
- Every load of the skill pulls in only skills the agent needs to finish the current artifact.
- The set of allowed links is bounded and rule-checkable by one yes/no question.
- Sequencing across artifacts still exists and is discoverable, in the one place built for it, without inflating every step skill.
- A step skill that genuinely needs another skill to complete its work still points there — the rule does not force required sub-steps into prose.

### Costs
- A reader inside a step skill who wants the whole pipeline shape must open the `README.md` — one extra hop.
- "Required sub-step or later stage?" occasionally needs judgement for a skill that both feeds this one's artifact and continues past it: resolve it by the artifact test — link only if this skill's own output is incomplete without the target.

## Link every skill mentioned by name

### Description
Whenever a skill's prose names another skill, wikilink it, regardless of the reason.

### Benefits
- Uniform and needs no judgement — mention implies link.
- Every named skill is one click away.

### Costs
- The loader pulls all of them into context, including skills irrelevant to the task.
- Non-goal links accumulate without limit as the catalog grows; the skill drifts toward referencing everything adjacent to it.
- This is the status quo the repository already drifted into — `feature-map-create` carried forward links to `variability-map-create` and `delta-conflict-detection` purely to say "that is downstream".

## No cross-skill links except standards

### Description
Link only the shared standards a skill applies (`skill-design`, `adr-create`). Express every other relationship — including inputs — in plain words.

### Benefits
- Minimal context cost; the smallest possible link set.

### Costs
- An agent applying a skill that consumes another skill's artifact cannot navigate to the format of that artifact — it must search for it by name.
- Input relationships are real functional dependencies; hiding them as prose makes the pipeline's data flow invisible and unmaintainable.
