---
name: documentation-for-ai-agents
description: How to document a library, CLI tool, or API so another AI agent can use it correctly
whenToUse: when you need to create or update documentation for a library, CLI, or API that will be consumed by AI agents
tags:
  - documentation
  - skill
  - common-workflow
  - ai-agent
---

# Goal
- Create documentation in skill format that gives an AI agent a clear, actionable understanding of how to use a library, CLI, or API.
- Make the documentation discoverable and reusable by other agents inside this repository.

# Core Principle
- Documentation for AI agents must be executable instructions, not explanatory text for humans.
- The agent should read the skill and immediately know what actions to take, what parameters to pass, and what output to expect.

# Structure
This skill is split into focused sections. Read them in order when writing a new skill, or jump to the relevant section when updating an existing one.

- [installation.md](./installation.md) — How to document installation, import, environment setup, and access for the library, CLI, or API.
- [method-calls.md](./method-calls.md) — General requirements that apply to every documented method, command, or endpoint.
- [method-a.md](./method-a.md) — Example documentation for a primary method (`process_data`). Shows exact signature, parameters, return value, errors, and a runnable example.
- [method-b.md](./method-b.md) — Example documentation for a secondary method (`fetch_records`). Shows exact signature, parameters, return value, errors, and a runnable example.

Use [method-a.md](./method-a.md) and [method-b.md](./method-b.md) as templates when you document real methods of the target library, CLI, or API.

# Rule

## MUST
- Write the documentation as a skill following [skill-design.skill](../skill-design.skill/skill-design.skill.md).
- Save the skill in the `docs/skills/` directory under the correct domain (for example, `docs/skills/python/`, `docs/skills/dotnet/`, `docs/skills/devops/`, or `docs/skills/common-workflow/` if it is cross-domain).
- Write `whenToUse` as concrete trigger conditions, not as a generic description.
- Provide actionable rules that an agent can execute directly.
- Cover all information another agent needs to call the library, CLI, or API:
  - How to install, import, or access it ([installation.md](./installation.md)).
  - Entry points: functions, commands, endpoints, or classes ([method-calls.md](./method-calls.md)).
  - Required and optional parameters with their types and default values.
  - Return values, response format, or output shape.
  - Error handling and common failure modes.
  - At least one minimal working example for every entry point.

## SHOULD
- Use the [skill.template.md](../skill-design.skill/templates/skill.template.md) when no domain-specific template exists.
- Keep one skill focused on one library, tool, or API.
- Add tags that help other agents discover the skill.
- Link to official human-readable documentation only as supplementary context; do not rely on it as the primary instruction source.

## SHOULD NOT
- Place AI-agent documentation only in `.agents/skills/`, `.claude/skills/`, `README.md`, or wiki pages.
- Write long prose that explains concepts without giving the agent concrete commands or code.

## MUST NOT
- Write documentation for a human reader using marketing language or deep conceptual explanations without instructions.
- Create documentation that is not in skill format.

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

# Check list
- [ ] The documentation is saved as a skill in the `docs/skills/` directory.
- [ ] `whenToUse` clearly states when another agent should apply the skill.
- [ ] The skill covers installation/import ([installation.md](./installation.md)), entry points, parameters, return/output, errors, and examples.
- [ ] Every method follows the general requirements in [method-calls.md](./method-calls.md).
- [ ] Examples show exact code, commands, or requests, not only descriptions.
- [ ] No human-oriented marketing or conceptual-only text remains.
- [ ] The skill follows [skill-design.skill](../skill-design.skill/skill-design.skill.md).
