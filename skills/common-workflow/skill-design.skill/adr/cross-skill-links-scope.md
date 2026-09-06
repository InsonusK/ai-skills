---
name: cross-skill-links-scope
description: Which cross-skill links a skill file may carry — its inputs and the standards it applies, plus an explicit "do not use X" prohibition; never a link to a consumer, a next pipeline step, or a topic marked out of scope
problem: A skill mentioning a neighbouring skill by name tends to link it regardless of why it is mentioned. The skill loader pulls every linked skill into the agent's context, so a link that buys the reader nothing functional still costs context — and non-goals are unbounded, so each new neighbour would add another such link.
decision: A skill links only what it functionally relates to — the skill whose output it reads as input, and the standards/templates it applies — plus, deliberately kept, an active "do not apply X here" prohibition. It never links a downstream consumer, a "next, run Y" step, or a skill named only to say a topic lives elsewhere. Sequencing lives in the pipeline's non-skill README, which the loader does not pull in.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Skill files reference neighbouring skills for several unrelated reasons, and the default reflex is to wikilink every one of them. Two costs follow:

1. **Context.** The skill loader (`ai-skill-manager`) resolves a loaded skill's links and pulls the linked skills into the agent's working context. A link that only says "that is handled elsewhere" or "run this next" loads a whole skill the agent does not need for the task in front of it.
2. **Unbounded growth.** A skill's non-goals are open-ended — there is always another neighbouring concern it does *not* cover. If "not my job" links are allowed, every new sibling skill earns one, until the file references a large fraction of the repository.

The distinction that matters is *why* a skill is mentioned, not *whether* it is:

- `variability-map-create` reads the `feature-model.md` that `feature-map-create` produces — it genuinely needs to point at its input.
- `feature-map-create` saying "after this, run `variability-map-create`" makes it an orchestrator of a pipeline it is only one step of.
- A `# Core Principle` bullet saying "the plateau↔VP view is not here, see `plateau-map-repository`" is a pure non-goal marker.
- "This is not a Plateau Component — do not follow `plateau-component-create`" is an active instruction: the reader is being told what *not* to do, and the link lets them check exactly what.

# Selected variant

**Selected variant:** [[#Link inputs and standards, plus active prohibitions]]

- Functional relationships (input consumed, standard applied) are linked because the agent must navigate to them to do the work.
- The active "do not use X" case is kept deliberately, despite being a boundary statement, because it carries an instruction and the reader needs to see what is being ruled out.
- Consumer links, "next step" links, and non-goal markers are dropped: they cost context and buy nothing, and sequencing has a home that the loader does not pull in — the pipeline's non-skill `README.md`.

# Searched variants

## Link inputs and standards, plus active prohibitions

### Description
A skill may link: the skill whose artifact it reads as input ("use the `{artifact}` produced by `[[X]]`"); a standard or template it applies (`[[skill-design]]`, `[[adr-create]]`, `[[mermaid-diagram]]`); and an active prohibition (`do not apply [[X]] here`). It must not link: a downstream skill that consumes *its* output; a "next, run `[[Y]]`" step; or a skill named only to mark a topic as out of scope. Sequencing across a multi-step pipeline lives in that pipeline's `README.md` — a plain index file, not a skill, that the loader does not resolve.

### Benefits
- Every load of the skill pulls in only skills the agent needs for the current task.
- The set of allowed links is bounded and rule-checkable — "is this an input, a standard, or an active prohibition?" is a yes/no question.
- Sequencing still exists and is discoverable, in the one place built for it, without inflating every step skill.
- A step skill stays a step skill: it describes how to produce its own artifact, not where that artifact goes next.

### Costs
- A reader inside a step skill who wants the whole pipeline shape must open the `README.md` — one extra hop.
- "Is this link a genuine input or a disguised next-step pointer?" occasionally needs judgement (e.g. a skill that both reads an upstream artifact *and* is the next step): resolve it by keeping the link only for the input relationship and phrasing any forward reference in plain words.

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
