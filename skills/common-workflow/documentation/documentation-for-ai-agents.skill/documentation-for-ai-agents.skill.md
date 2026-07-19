---
name: documentation-for-ai-agents
description: How to document a library, CLI tool, or API so another AI agent can use it correctly
whenToUse: when you need to create or update documentation for a library, CLI, or API that will be consumed by AI agents
tags:
  - skill/documentation/for-ai
---

# Goal
- Create documentation in skill format that gives an AI agent a clear, actionable understanding of how to use a library, CLI, or API.
- Make the documentation discoverable and reusable by other agents inside this repository.

# Core Principle
- Documentation for AI agents must be executable instructions, not explanatory text for humans.
- The agent should read the skill and immediately know what actions to take, what parameters to pass, and what output to expect.
- Skill discovery has a cost: every skill's `name`/`description`/`whenToUse` is loaded up front so an agent can decide what to use, but only the triggered skill's body (and the files it links to) is read in full. Choose the single-skill or skill-group shape below so that cost stays low and `whenToUse` stays precise.

# One skill or a skill group?

Decide the shape before writing any content.

## Single skill

**When to apply**

Use when the target has one coherent domain and a small-to-medium surface:
- roughly ten or fewer entry points;
- one trigger condition covers every capability an agent would need;
- all methods share the same installation, authentication, and conventions.

**Skill structure**

```
docs/skills/<domain>/<library>.skill/
├── <library>.skill.md              # root skill
├── installation.md                 # attached file, never its own skill
└── method-<name>.md                # one fragment per method/endpoint
```

**Templates to use**

- Root skill: [templates/library.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/library.skill.template.md)
- Method fragment format: follow [templates/method-calls.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-calls.template.md) and copy the level of detail from [examples/simple_skill/mylib.skill/method-process_data.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/simple_skill/mylib.skill/method-process_data.md) and [examples/simple_skill/mylib.skill/method-fetch_records.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/simple_skill/mylib.skill/method-fetch_records.md).

**Worked example**

See [examples/simple_skill/mylib.skill/](./examples/simple_skill/mylib.skill/) for a complete single-skill documentation of a fictional small library.

## Skill group

**When to apply**

Use when any of these is true:
- The API has multiple functional domains an agent would use independently (for example, `auth`, `billing`, `webhooks`).
- The surface is large enough that a single index would be hard to navigate (rule of thumb: more than ~10–15 entry points, or several unrelated domains).
- Domains have different prerequisites (different credentials, different install extras, different environments).

**Skill structure**

```
docs/skills/<domain>/<library>.skill/
├── <library>.skill.md              # root/overview skill
└── installation.md                 # attached file, never its own skill

docs/skills/<domain>/<library>-<domain>.skill.md    # one child skill per domain
```

**Templates to use**

- Root skill: [templates/library.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/library.skill.template.md)
- Domain child skill: [templates/method-group.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-group.skill.template.md)
- Method fragment format inside each child skill: follow [templates/method-calls.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-calls.template.md).

**Worked example**

See [examples/complex_skill/](./examples/complex_skill/) for a complete skill group documenting a fictional API with `auth` and `billing` domains.

# Structure

This skill is split into focused sections. Read them in order when writing a new skill, or jump to the relevant section when updating an existing one.

- [templates/method-calls.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-calls.template.md) — General requirements that apply to every documented method, command, or endpoint.
- [templates/installation.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/installation.md) — Rules and example format for the attached `installation.md` file of a library or API skill.
- [templates/library.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/library.skill.template.md) — Fill-in-the-blank template for the root/overview skill (single-skill shape, or the root of a skill group).
- [templates/method-group.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-group.skill.template.md) — Fill-in-the-blank template for a child skill covering one functional domain (skill-group shape only).
- [examples/simple_skill/mylib.skill/](./examples/simple_skill/mylib.skill/) — Worked example of the single-skill shape.
- [examples/complex_skill/](./examples/complex_skill/) — Worked example of the skill-group shape.

# Rule

## MUST
- Write the documentation as a skill following [skill-design.skill](skills/common-workflow/skill-design.skill/skill-design.skill.md).
- Save the skill in the `docs/skills/` directory under the correct domain (for example, `docs/skills/python/`, `docs/skills/dotnet/`, `docs/skills/devops/`, or `docs/skills/common-workflow/` if it is cross-domain).
- Decide between a single skill and a skill group using [# One skill or a skill group?](#one-skill-or-a-skill-group) before writing any content.
- Write `whenToUse` as concrete trigger conditions, not as a generic description. In a skill group, make each child skill's `whenToUse` specific to its own domain so it does not overlap with sibling skills.
- Provide actionable rules that an agent can execute directly.
- Cover all information another agent needs to call the library, CLI, or API:
  - How to install, import, or access it ([templates/installation.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/installation.md)).
  - Entry points: functions, commands, endpoints, or classes ([templates/method-calls.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-calls.template.md)).
  - Required and optional parameters with their types and default values.
  - Return values, response format, or output shape.
  - Error handling and common failure modes.
  - At least one minimal working example for every entry point.
- In a skill group, document installation/access once in the root skill's attached `installation.md` and link to it from every child skill instead of duplicating it.
- Keep installation/access as a file attached to the root/library skill (`installation.md`). Never create a separate skill whose only purpose is installation instructions.

## SHOULD
- Use [templates/library.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/library.skill.template.md) for the root/overview skill and [templates/method-group.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-group.skill.template.md) for each domain child skill; fall back to the generic [skill.template.md](skills/common-workflow/skill-design.skill/templates/skill.template.md) only if neither fits.
- Keep one skill focused on one library, tool, or API — or, for a skill group, one skill focused on one domain within that library, tool, or API.
- Add tags that help other agents discover the skill.
- Link to official human-readable documentation only as supplementary context; do not rely on it as the primary instruction source.

## SHOULD NOT
- Place AI-agent documentation only in `.agents/skills/`, `.claude/skills/`, `README.md`, or wiki pages.
- Write long prose that explains concepts without giving the agent concrete commands or code.

## MUST NOT
- Write documentation for a human reader using marketing language or deep conceptual explanations without instructions.
- Create documentation that is not in skill format.
- Create one skill per individual method when methods belong to the same domain; group them into one domain skill instead.

# Anti-patterns

- **Writing for a human reader instead of an AI agent**
  - Example: "This library provides a powerful and flexible way to process data."
  - Consequence: the agent does not know how to import the library or which function to call.
  - Instead: write "To process data, import `process_data` from `mylib.core` and call it with `process_data(source: str, limit: int = 100)`.

- **Saving documentation outside the `docs/skills/` directory**
  - Example: creating `doc/api-usage.md`.
  - Consequence: consumers do not know the document is meant for an agent and may not include it in the agent context.
  - Instead: create `docs/skills/<domain>/<tool>.skill.md`.

- **Vague `whenToUse`**
  - Example: "Use this for API documentation."
  - Consequence: the agent cannot decide whether the skill applies to the current task.
  - Instead: "Use this skill when calling the X API to authenticate or fetch user data."

- **Describing behavior without showing exact calls**
  - Example: "The function returns a list of records."
  - Consequence: the agent must guess the call signature and parameter order.
  - Instead: include the exact signature, a runnable example, and a sample output.

- **Relying on external documentation as the primary source**
  - Example: "See the official docs for details."
  - Consequence: the agent may not fetch or parse the external page correctly.
  - Instead: copy the minimal essential facts into the skill and link to the official docs only for deeper reference.

- **One monolithic skill for a large, multi-domain API**
  - Example: a single `stripe.skill.md` that inlines every endpoint of payments, customers, subscriptions, and webhooks under one generic `whenToUse: "when using the Stripe API"`.
  - Consequence: the agent must load or scan an oversized skill for a narrow task, and unrelated domains dilute `whenToUse` so the agent cannot tell if the skill matches the current task.
  - Instead: split into a root/overview skill plus one child skill per domain, following [# One skill or a skill group?](#one-skill-or-a-skill-group).

- **A separate skill per individual method**
  - Example: `mylib-process-data.skill.md`, `mylib-fetch-records.skill.md`, one file per function of the same cohesive module.
  - Consequence: skill discovery gets noisy with many near-duplicate `whenToUse` entries, and the agent loses the shared context (installation, error conventions) that ties the methods together.
  - Instead: group related methods into one domain skill with one method fragment per entry point, as shown in [examples/complex_skill/](./examples/complex_skill/).

# Check list
- [ ] The documentation is saved as a skill in the `docs/skills/` directory.
- [ ] The single-skill-vs-skill-group decision was made deliberately using [# One skill or a skill group?](#one-skill-or-a-skill-group), not defaulted without thought.
- [ ] The root/library skill was built from [templates/library.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/library.skill.template.md); any domain child skill was built from [templates/method-group.skill.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-group.skill.template.md).
- [ ] `whenToUse` clearly states when another agent should apply the skill, and in a skill group, each child's `whenToUse` is specific to its own domain.
- [ ] The skill covers installation/import ([templates/installation.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/installation.md)), entry points, parameters, return/output, errors, and examples.
- [ ] Installation/access lives only in `installation.md`, attached to the root/library skill — it is never its own skill file.
- [ ] In a skill group, installation/access is documented once in the root skill and linked from every child skill.
- [ ] Every method follows the general requirements in [templates/method-calls.template.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/templates/method-calls.template.md).
- [ ] Examples show exact code, commands, or requests, not only descriptions.
- [ ] No human-oriented marketing or conceptual-only text remains.
- [ ] The skill follows [skill-design.skill](skills/common-workflow/skill-design.skill/skill-design.skill.md).
